using System;

namespace Progression
{
    internal sealed class FixedXoshiro256StarStar
    {
        private ulong s0;
        private ulong s1;
        private ulong s2;
        private ulong s3;

        public FixedXoshiro256StarStar(byte[] seed)
        {
            if (seed == null || seed.Length != 32)
            {
                throw new ArgumentException("A 256-bit seed is required.", nameof(seed));
            }

            s0 = ReadUInt64(seed, 0);
            s1 = ReadUInt64(seed, 8);
            s2 = ReadUInt64(seed, 16);
            s3 = ReadUInt64(seed, 24);
            if ((s0 | s1 | s2 | s3) == 0)
            {
                s0 = 0x9e3779b97f4a7c15UL;
            }
        }

        public int NextIndex(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            ulong bound = (ulong)exclusiveMax;
            ulong threshold = unchecked(0UL - bound) % bound;
            ulong value;
            do
            {
                value = NextUInt64();
            }
            while (value < threshold);

            return (int)(value % bound);
        }

        private ulong NextUInt64()
        {
            ulong result = RotateLeft(s1 * 5, 7) * 9;
            ulong temporary = s1 << 17;
            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;
            s2 ^= temporary;
            s3 = RotateLeft(s3, 45);
            return result;
        }

        private static ulong ReadUInt64(byte[] source, int offset)
        {
            ulong result = 0;
            for (int index = 0; index < 8; index++)
            {
                result |= (ulong)source[offset + index] << (index * 8);
            }

            return result;
        }

        private static ulong RotateLeft(ulong value, int count) =>
            (value << count) | (value >> (64 - count));
    }
}
