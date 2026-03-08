using System.Buffers;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace KC.Tik4Net.Api;

internal sealed partial class ApiConnection : ITikConnection
{
    private const int API_DEFAULT_PORT = 8728;
    private const int APISSL_DEFAULT_PORT = 8729;

    private const int MaxWordLength = 16 * 1024 * 1024;

    private static readonly string[] OnReadEventArgs = ["=category=-1", "=message=connection closed"];
    private static readonly string[] QuitCommand = ["/quit"];
    private readonly Lock _executeLock = new();
    private readonly SentenceList _readSentences = new();
    private DecoderFallback _decoderFallback = DecoderFallback.ReplacementFallback;

    private Encoding _encoding = null!;

    private int _executeInFlight;

    private volatile bool _isOpened;
    private Decoder _latin1Decoder = null!;

    private TcpClient _tcpConnection = null!;
    private Stream _tcpConnectionStream = null!;

    private Decoder _utf8Decoder = null!;

    public ApiConnection(bool isSsl)
    {
        IsSsl = isSsl;
        SetEncoding(Encoding.UTF8);
        DebugEnabled = Debugger.IsAttached;
    }

    public Encoding Encoding
    {
        get => _encoding;
        set
        {
            Guard.ArgumentNotNull(value, nameof(value));
            SetEncoding(value);
        }
    }

    public bool EncodingStrict
    {
        get => ReferenceEquals(_decoderFallback, DecoderFallback.ExceptionFallback);
        set
        {
            _decoderFallback = value ? DecoderFallback.ExceptionFallback : DecoderFallback.ReplacementFallback;
            SetEncoding(_encoding);
        }
    }

    public bool IsSsl { get; }

    public bool DebugEnabled { get; set; }

    public bool IsOpened => _isOpened;

    public int SendTimeout { get; set; }

    public int ReceiveTimeout { get; set; }

    public async ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Close();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }

    public async Task ConnectAsync(string host, string user, string password,
        CancellationToken cancellationToken = default)
    {
        await ConnectAsync(host, IsSsl ? APISSL_DEFAULT_PORT : API_DEFAULT_PORT, user, password, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ConnectAsync(string host, int port, string user, string password,
        CancellationToken cancellationToken = default)
    {
        _tcpConnection = new TcpClient();
        if (SendTimeout > 0)
            _tcpConnection.SendTimeout = SendTimeout;
        if (ReceiveTimeout > 0)
            _tcpConnection.ReceiveTimeout = ReceiveTimeout;

        await _tcpConnection.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);

        var tcpStream = _tcpConnection.GetStream();
        if (!IsSsl)
        {
            _tcpConnectionStream = tcpStream;
        }
        else
        {
            var sslStream = new SslStream(tcpStream, false, ValidateServerCertificate, null);
            try
            {
                await sslStream.AuthenticateAsClientAsync(host, null, SslProtocols.Tls12 | SslProtocols.Tls13, false)
                    .ConfigureAwait(false);
            }
            catch (AuthenticationException ex)
            {
                throw new TikConnectionSSLErrorException(ex);
            }

            _tcpConnectionStream = sslStream;
        }

        _isOpened = true;
        await Login_v3Async(user, password, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_isOpened)
            Close();
    }

    public async Task<IReadOnlyList<ITikSentence>> ExecuteAsync(IEnumerable<string> commandRows,
        CancellationToken cancellationToken = default)
    {
        var list = new List<ITikSentence>();
        await foreach (var s in ExecuteStreamAsync(commandRows).WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
            list.Add(s);
        return list;
    }

    public async IAsyncEnumerable<ITikSentence> ExecuteStreamAsync(IEnumerable<string> commandRows)
    {
        if (Interlocked.CompareExchange(ref _executeInFlight, 1, 0) != 0)
            throw new InvalidOperationException("Concurrent Execute* on the same connection is not supported.");

        using var cancellationRegistration = CancellationToken.None.Register(static _ => { }, null);
        try
        {
            EnsureOpened();

            var tagOrEmptyString = string.Empty;
            var rows = commandRows as string[] ?? [.. commandRows];

            foreach (var row in rows)
            {
                var match = TagRegex().Match(row);
                if (match.Success)
                {
                    tagOrEmptyString = match.Groups["TAG"].Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(tagOrEmptyString))
            {
                tagOrEmptyString = TagSequence.Next().ToString();
                rows = [.. rows, $"{TikSpecialProperties.Tag}={tagOrEmptyString}"];
            }

            lock (_executeLock)
            {
                WriteCommandAsync(rows, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            }

            while (true)
            {
                ITikSentence sentence;
                lock (_executeLock)
                {
                    sentence = GetOneAsync(tagOrEmptyString, CancellationToken.None).AsTask().GetAwaiter().GetResult();
                }

                yield return sentence;

                if (sentence is ITikDoneSentence or ApiFatalSentence)
                    break;

                if (sentence is ApiTrapSentence trap)
                    throw new TikCommandTrapException(new ApiCommand(this), trap);
            }
        }
        finally
        {
            Volatile.Write(ref _executeInFlight, 0);
        }
    }

    public ITikCommand CreateCommand()
    {
        return new ApiCommand(this);
    }

    public ITikCommand CreateCommand(TikCommandParameterFormat defaultParameterFormat)
    {
        var result = CreateCommand();
        result.DefaultParameterFormat = defaultParameterFormat;
        return result;
    }

    public ITikCommand CreateCommand(string commandText, params ITikCommandParameter[] parameters)
    {
        return new ApiCommand(this, commandText, parameters);
    }

    public ITikCommand CreateCommand(string commandText, TikCommandParameterFormat defaultParameterFormat,
        params ITikCommandParameter[] parameters)
    {
        var result = CreateCommand(commandText, parameters);
        result.DefaultParameterFormat = defaultParameterFormat;
        return result;
    }

    public ITikCommand CreateCommandAndParameters(string commandText, params string[] parameterNamesAndValues)
    {
        var result = new ApiCommand(this, commandText);
        result.AddParameterAndValues(parameterNamesAndValues);
        return result;
    }

    public ITikCommand CreateCommandAndParameters(string commandText, TikCommandParameterFormat defaultParameterFormat,
        params string[] parameterNamesAndValues)
    {
        var result = CreateCommandAndParameters(commandText, parameterNamesAndValues);
        result.DefaultParameterFormat = defaultParameterFormat;
        return result;
    }

    public ITikCommandParameter CreateParameter(string name, string value)
    {
        return new ApiCommandParameter(name, value);
    }

    public ITikCommandParameter CreateParameter(string name, string value, TikCommandParameterFormat parameterFormat)
    {
        var result = CreateParameter(name, value);
        result.ParameterFormat = parameterFormat;
        return result;
    }

    private void SetEncoding(Encoding encoding)
    {
        if (!string.Equals(encoding.WebName, Encoding.UTF8.WebName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only UTF-8 encoding is supported.", nameof(encoding));

        _encoding = Encoding.UTF8;

        var utf8 = Encoding.GetEncoding(
            Encoding.UTF8.WebName,
            EncoderFallback.ExceptionFallback,
            _decoderFallback);

        _utf8Decoder = utf8.GetDecoder();
        _latin1Decoder = Encoding.Latin1.GetDecoder();
    }

    private void EnsureOpened()
    {
        if (!_isOpened)
            throw new TikConnectionNotOpenException("Connection has not been opened.");
    }

    public void Close()
    {
        try
        {
            if (IsOpened)
                WriteCommandAsync(QuitCommand, CancellationToken.None).AsTask().GetAwaiter().GetResult();
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }

        _tcpConnectionStream.Dispose();
        _tcpConnection.Dispose();
        _isOpened = false;
    }

    private async Task Login_v3Async(string user, string password, CancellationToken cancellationToken)
    {
        try
        {
            _ = await ExecuteAsync(
            [
                "/login",
                $"=name={user}",
                $"=password={password}"
            ], cancellationToken).ConfigureAwait(false);
        }
        catch (TikCommandTrapException ex)
        {
            if (ex.Message == "cannot log in")
                throw new TikConnectionLoginException(ex);
            if (ex.Message.StartsWith("invalid user name or password"))
                throw new TikConnectionLoginException(ex);
            throw;
        }
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private async ValueTask<int> ReadByteOrThrowAsync(CancellationToken cancellationToken)
    {
        var one = ArrayPool<byte>.Shared.Rent(1);
        try
        {
            var read = await _tcpConnectionStream.ReadAsync(one.AsMemory(0, 1), cancellationToken)
                .ConfigureAwait(false);
            if (read != 1)
                throw new IOException("Connection closed while reading.");
            return one[0];
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(one);
        }
    }

    private async ValueTask<long> ReadWordLengthAsync(CancellationToken cancellationToken)
    {
        var first = await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
        var readByte = (byte)first;
        int length;

        if ((readByte & 0x80) != 0x00)
        {
            if ((readByte & 0xC0) == 0x80)
            {
                length = ((readByte & 0x3F) << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
            }
            else if ((readByte & 0xE0) == 0xC0)
            {
                length = ((readByte & 0x1F) << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
                length = (length << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
            }
            else if ((readByte & 0xF0) == 0xE0)
            {
                length = ((readByte & 0xF) << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
                length = (length << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
                length = (length << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                length = await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
                length = (length << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
                length = (length << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
                length = (length << 8) + await ReadByteOrThrowAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            length = readByte;
        }

        return length;
    }

    private static async ValueTask ReadExactlyAsync(Stream stream, Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        var readTotal = 0;
        while (readTotal < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer[readTotal..], cancellationToken).ConfigureAwait(false);
            if (read <= 0)
                throw new IOException("Connection closed while reading word.");
            readTotal += read;
        }
    }

    private async ValueTask<string> ReadWordAsync(bool skipEmptyRow, CancellationToken cancellationToken)
    {
        string result;

        do
        {
            var wordLength = await ReadWordLengthAsync(cancellationToken).ConfigureAwait(false);

            if (wordLength < 0)
            {
                result = string.Empty;
                break;
            }

            if (wordLength > MaxWordLength)
                throw new IOException("Word length too large.");

            var byteBuffer = ArrayPool<byte>.Shared.Rent((int)wordLength);
            var charBuffer = ArrayPool<char>.Shared.Rent(_encoding.GetMaxCharCount((int)wordLength));
            try
            {
                await ReadExactlyAsync(_tcpConnectionStream, byteBuffer.AsMemory(0, (int)wordLength),
                    cancellationToken).ConfigureAwait(false);
                result = RouterOsStringCodec.DecodeWord(byteBuffer.AsSpan(0, (int)wordLength), charBuffer,
                    _utf8Decoder, _latin1Decoder);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(byteBuffer);
                ArrayPool<char>.Shared.Return(charBuffer);
            }
        } while (skipEmptyRow && string.IsNullOrWhiteSpace(result));

        if (DebugEnabled)
            Debug.WriteLine("< " + result);
        return result;
    }

    internal static byte[] EncodeWordForRouterOs(string row)
    {
        return RouterOsStringCodec.EncodeWordForRouterOs(row);
    }

    private async ValueTask<ITikSentence> ReadSentenceAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sentenceName = await ReadWordAsync(true, cancellationToken).ConfigureAwait(false);

            var sentenceWords = new List<string>();
            string sentenceWord;
            do
            {
                sentenceWord = await ReadWordAsync(false, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(sentenceWord))
                    sentenceWords.Add(sentenceWord);
            } while (!string.IsNullOrWhiteSpace(sentenceWord));

            return sentenceName switch
            {
                "!done" => new ApiDoneSentence(sentenceWords),
                "!trap" => new ApiTrapSentence(sentenceWords),
                "!re" => new ApiReSentence(sentenceWords),
                "!fatal" => new ApiFatalSentence(sentenceWords),
                "!empty" => new ApiEmptySentence(),
                "" => throw new IOException("Can not read sentence from connection"),
                _ => new ApiReSentence(sentenceWords)
            };
        }
        catch (IOException)
        {
            _isOpened = _tcpConnection.Connected;
            throw;
        }
    }

    private async ValueTask WriteCommandAsync(IEnumerable<string> commandRows, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var row in commandRows)
            {
                var bytes = EncodeWordForRouterOs(row);
                var length = ApiConnectionHelper.EncodeLength(bytes.Length);

                await _tcpConnectionStream.WriteAsync(length.AsMemory(0, length.Length), cancellationToken)
                    .ConfigureAwait(false);
                await _tcpConnectionStream.WriteAsync(bytes.AsMemory(0, bytes.Length), cancellationToken)
                    .ConfigureAwait(false);

                if (DebugEnabled)
                    Debug.WriteLine("> " + row);
            }

            await _tcpConnectionStream.WriteAsync(new byte[] { 0 }, cancellationToken).ConfigureAwait(false);
            await _tcpConnectionStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (IOException)
        {
            _isOpened = _tcpConnection.Connected;
            throw;
        }
    }

    private async ValueTask<ITikSentence> GetOneAsync(string tag, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (!_tcpConnection.Connected)
                _isOpened = false;

            if (!_isOpened)
                return new ApiTrapSentence(OnReadEventArgs);

            if (_readSentences.TryDequeue(tag, out var result))
                return result;

            ITikSentence sentenceFromTcp;
            lock (_readSentences)
            {
                if (_readSentences.TryDequeue(tag, out result))
                    return result;

                sentenceFromTcp = null!;
            }

            sentenceFromTcp = await ReadSentenceAsync(cancellationToken).ConfigureAwait(false);

            lock (_readSentences)
            {
                if (sentenceFromTcp.Tag == tag)
                    return sentenceFromTcp;

                _readSentences.Enqueue(sentenceFromTcp);
            }
        }
    }

    [GeneratedRegex($"^{TikSpecialProperties.Tag}=(?<TAG>.+)$")]
    private static partial Regex TagRegex();
}