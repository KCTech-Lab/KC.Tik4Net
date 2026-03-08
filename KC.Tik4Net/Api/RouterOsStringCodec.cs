using System.Text;

namespace KC.Tik4Net.Api;

internal static class RouterOsStringCodec
{
    public static byte[] EncodeWordForRouterOs(string row)
    {
        // WinBox-compatible "ANSI" behavior (ISO-8859-1 / Latin-1).
        // RouterOS API historically treats words as bytes; using Latin-1 avoids
        // UTF-8 transcoding issues and matches prior tik4net behavior.
        var bytes = new byte[row.Length];
        for (var i = 0; i < row.Length; i++)
        {
            var ch = row[i];
            bytes[i] = ch <= 0xFF ? (byte)ch : (byte)'?';
        }

        return bytes;
    }

    public static string DecodeWord(ReadOnlySpan<byte> bytes, char[] charBuffer, Decoder utf8Decoder,
        Decoder latin1Decoder)
    {
        // RouterOS mixes encodings across subsystems. In practice, WinBox-compatible strings are Latin-1.
        // Some newer subsystems can emit UTF-8; detect UTF-8 only when it is very likely.

        if (LooksLikeUtf8(bytes))
            return DecodeUtf8(bytes, utf8Decoder, charBuffer);

        latin1Decoder.Reset();
        var latinChars = latin1Decoder.GetChars(bytes, charBuffer, true);
        return new string(charBuffer, 0, latinChars);
    }

    private static bool LooksLikeUtf8(ReadOnlySpan<byte> bytes)
    {
        // Heuristic:
        // - If there are no high-bit bytes => treat as Latin-1.
        // - If there are any bytes in the C0/C1 control ranges when interpreted as UTF-8 output (0x80-0x9F)
        //   it's usually Latin-1 text, not UTF-8.
        // - If it contains typical UTF-8 lead bytes (C2-F4) followed by valid continuation bytes, accept.

        var anyHighBit = false;
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            if ((b & 0x80) == 0)
                continue;

            anyHighBit = true;

            if (b is >= 0x80 and <= 0x9F)
                return false;
        }

        if (!anyHighBit)
            return false;

        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            if (b < 0x80)
                continue;

            var needed = b switch
            {
                >= 0xC2 and <= 0xDF => 1,
                >= 0xE0 and <= 0xEF => 2,
                >= 0xF0 and <= 0xF4 => 3,
                _ => -1
            };

            if (needed < 0)
                return false;

            if (i + needed >= bytes.Length)
                return false;

            for (var j = 1; j <= needed; j++)
            {
                var c = bytes[i + j];
                if ((c & 0xC0) != 0x80)
                    return false;
            }

            i += needed;
        }

        return true;
    }

    private static string DecodeUtf8(ReadOnlySpan<byte> bytes, Decoder utf8Decoder, char[] charBuffer)
    {
        utf8Decoder.Reset();
        var charsUsed = utf8Decoder.GetChars(bytes, charBuffer, true);
        return new string(charBuffer, 0, charsUsed);
    }
}