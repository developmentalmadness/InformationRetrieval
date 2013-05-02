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
        
        // we could use any size here, we just need to match the number of bits for each element in the vector (byte == 8; short = 16; int = 32; long = 64;)
        private const byte bucketSize = 8;

        public BloomFilter(uint size, int hashTransformCount)
        {
            // TODO: this can be calculated, provide an overload that omits this parameter and calculates it based on "size"
            this.hashTransformCount = hashTransformCount;
            this.size = size;

            // vectorSize should be the specified size divided by the size of an element in the array, plus 1 for rounding/overflow
            uint vectorSize = (size / bucketSize) + 1u;
            vector = new byte[vectorSize];
        }

        public void Add(T key)
        {
            ulong[] hash = hashProvider.GetHashCodes(GetBytes(key), hashTransformCount, size);

            for (int i = 0; i < hash.Length; i++)
            {
                // use integer division to determine which "bucket" of 8 bits the hash falls into
                ulong bucket = hash[i] / bucketSize;

                // get the remainder to find which individual bit to flip, then shift to create a mask
                byte slot = (byte) (1 << (int)(hash[i] % bucketSize));

                // apply the bit mask to the bucket
                vector[bucket] |= slot;
            }
        }

        public bool Test(T key)
        {
            ulong[] hash = hashProvider.GetHashCodes(GetBytes(key), hashTransformCount, size);

            for (int i = 0; i < hash.Length; i++)
            {
                // use integer division to determine which "bucket" of 8 bits the hash falls into
                ulong bucket = hash[i] / bucketSize;

                // get the remainder to find which individual bit to flip, then shift to create a mask
                byte slot = (byte)(1 << (int)(hash[i] % bucketSize));

                // use the mask to check the filter for existance
                if ((vector[bucket] & slot) == 0)
                    return false;
            }

            return true;
        }

        protected abstract byte[] GetBytes(T value);
    }
}
