// 利用byte第一個bit判定是否結束
// 7 -> 0000 0111 -> 1111
// 79 -> 0100 1111 -> 0010 0010 1111
// 以達壓縮int的目的

using System.Collections.Generic;

public class SlimNumberUtility 
{
    public static void Compress(int value, int byteCount, ref List<byte> bytes)
    {
        var isWriting = false;
        for (int i = byteCount; i >= 0; i--)
        {
            var compressed = value >> 7 * i;
            if (!isWriting && compressed == 0)
                continue;
            isWriting = true;
            compressed <<= 1;
            if (i == 0)
                compressed |= 0x01;
            bytes.Add((byte)compressed);
        }
    }

    public static int Decompress(byte[] bytes, int offset, out int result)
    {
        result = 0;
        if (offset >= bytes.Length)
            return -1;

        var endIndex = offset;
        var startIndex = 0;
        for (int i = offset; i < bytes.Length; i++)
        {
            var oneByte = bytes[i];
            if ((oneByte & 0x01) != 1)
                continue;
            startIndex = i;
            break;
        }

        var digit = 0;
        for (int i = startIndex; i >= endIndex; i--)
        {
            int num = bytes[i];
            num >>= 1;
            num <<= 7 * digit;
            result |= num;
            digit++;
        }

        return offset + digit;
    }
}
