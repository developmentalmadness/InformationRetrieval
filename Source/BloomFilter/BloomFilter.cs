using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public abstract class BloomFilter<T> : IBloomFilter<T>
    {
        private IHashFunctionProvider hashProvider = new HashProvider();
        private int hashTransformCount;
        private byte[] vector;
        private uint size;
        private const byte bucketSize = 8;

        public BloomFilter(uint size, int hashTransformCount)
        {
            this.hashTransformCount = hashTransformCount;
            this.size = size;

            uint vectorSize = (size / bucketSize) + 1u;
            vector = new byte[vectorSize];
        }

        public void Add(T key)
        {
            ulong[] hash = hashProvider.GetHashCodes(GetBytes(key), hashTransformCount, size);

            for (int i = 0; i < hash.Length; i++)
            {
                ulong bucket = hash[i] / bucketSize;
                byte slot = (byte) (1 << (int)(hash[i] % bucketSize));

                vector[bucket] |= slot;
            }
        }

        public bool Test(T key)
        {
            ulong[] hash = hashProvider.GetHashCodes(GetBytes(key), hashTransformCount, size);

            for (int i = 0; i < hash.Length; i++)
            {
                ulong bucket = hash[i] / bucketSize;
                byte slot = (byte)(1 << (int)(hash[i] % bucketSize));

                if ((vector[bucket] & slot) == 0)
                    return false;
            }

            return true;
        }

        protected abstract byte[] GetBytes(T value);
    }
}
