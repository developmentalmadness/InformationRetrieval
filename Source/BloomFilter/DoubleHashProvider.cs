using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Provides hashing capabilities required by BloomFilter
    /// </summary>
    /// <see cref="http://www.eecs.harvard.edu/~kirsch/pubs/bbbf/rsa.pdf"/>
    public class DoubleHashProvider
    {
        // FNV constants obtained from: http://isthe.com/chongo/tech/comp/fnv/
        const uint x32Offset = 2166136261;
        const uint x32Prime = 16777619;
        const ulong x64Offset = 14695981039346656037;
        const ulong x64Prime = 1099511628211;

        /// <summary>
        /// FNV-1 (32-bit) non-cryptographic hash function.
        /// </summary>
        public uint Hashx32FNV1(byte[] bytes)
        {
            uint hash = x32Offset;

            for (var i = 0; i < bytes.Length; i++)
            {
                hash = hash * x32Prime;
                hash = hash ^ bytes[i];
            }

            return hash;
        }

        /// <summary>
        /// FNV-1 (64-bit) non-cryptographic hash function.
        /// </summary>
        public ulong Hashx64FNV1(byte[] bytes)
        {
            ulong hash = x64Offset;

            for (var i = 0; i < bytes.Length; i++)
            {
                hash = hash * x64Prime;
                hash = hash ^ bytes[i];
            }

            return hash;
        }

        /// <summary>
        /// FNV-1a (32-bit) non-cryptographic hash function.
        /// </summary>
        public uint Hashx32FNV1a(byte[] bytes)
        {
            uint hash = x32Offset;

            for (var i = 0; i < bytes.Length; i++)
            {
                hash = hash ^ bytes[i];
                hash = hash * x32Prime;
            }

            return hash;
        }

        /// <summary>
        /// FNV-1a (64-bit) non-cryptographic hash function.
        /// </summary>
        public ulong Hashx64FNV1a(byte[] bytes)
        {
            ulong hash = x64Offset;

            for (var i = 0; i < bytes.Length; i++)
            {
                hash = hash ^ bytes[i];
                hash = hash * x64Prime;
            }

            return hash;
        }

        /// <summary>
        /// Applies <paramref name="count"/> hash transformations on <paramref name="value"/> and 
        /// returns each transformation with a limit of <paramref name="size"/>.
        /// </summary>
        /// <see cref="http://www.eecs.harvard.edu/~kirsch/pubs/bbbf/rsa.pdf"/>
        /// <param name="value">The value to be hashed.</param>
        /// <param name="count">The number of transformations to be done.</param>
        /// <param name="size">The maximum hash code value.</param>
        /// <returns><paramref name="count"/> hash values.</returns>
        public int[] GetHashCodes(String value, int count, int size)
        {
            int[] result = new int[count];
            
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(value);
            ulong hash1 = Hashx64FNV1(bytes);
            ulong hash2 = Hashx64FNV1a(bytes);

            for (uint i = 1; i <= count; i++)
                result[i- 1] = (int) (hash1 + (i * hash2)) % size;

            return result;
        }
    }
}
