using System.Security.Cryptography;
using System.Text;

namespace KC.Tik4Net.Api;

internal static class ApiConnectionHelper
{
    internal static string EncodePassword(string password, string hash)
    {
        var hashBytes = Convert.FromHexString(hash);

        var passwordBytes = Encoding.ASCII.GetBytes(password);
        var payload = new byte[1 + passwordBytes.Length + hashBytes.Length];

        payload[0] = 0;
        passwordBytes.CopyTo(payload, 1);
        hashBytes.CopyTo(payload, 1 + passwordBytes.Length);

        var hashedPass = MD5.HashData(payload);

        // Needs lower-case hex to match previous behavior.
        return "00" + Convert.ToHexString(hashedPass).ToLowerInvariant();
    }

    internal static byte[] EncodeLength(int length)
    {
        if (length < 0x80)
        {
            var tmp = BitConverter.GetBytes(length);
            return [tmp[0]];
        }

        if (length < 0x4000)
        {
            var tmp = BitConverter.GetBytes(length | 0x8000);
            return [tmp[1], tmp[0]];
        }

        if (length < 0x200000)
        {
            var tmp = BitConverter.GetBytes(length | 0xC00000);
            return [tmp[2], tmp[1], tmp[0]];
        }

        if (length < 0x10000000)
        {
            var tmp = BitConverter.GetBytes((uint)length | 0xE0000000);
            return [tmp[3], tmp[2], tmp[1], tmp[0]];
        }
        else
        {
            var tmp = BitConverter.GetBytes(length);
            return [0xF0, tmp[3], tmp[2], tmp[1], tmp[0]];
        }
    }
}