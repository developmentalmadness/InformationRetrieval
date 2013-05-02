using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public abstract class CountingBloomFilter<T> : ICountingBloomFilter<T>
    {
        private IHashFunctionProvider hashProvider = new HashProvider();
        private byte[] vector;
        private uint size;
        private int hashTransformCount;

        // each slot is 4 bytes, so a bucket only has 2 slots 
        private const byte bucketSize = 2;
        private const byte slotSize = 4;

        public CountingBloomFilter(uint size, int hashTransformCount)
        {
            this.size = size;
            this.hashTransformCount = hashTransformCount;

            uint vectorSize = (size / bucketSize) + (size % bucketSize);
            vector = new byte[vectorSize];
        }

        public void Delete(T item)
        {
            var bytes = GetBytes(item);

            var hash = hashProvider.GetHashCodes(bytes, hashTransformCount, size);

            for (uint i = 1; i <= hash.Length; i++)
            {
                ulong bucket = hash[i - 1] / bucketSize;
                ulong slot = hash[i - 1] % bucketSize;

                int[] slots = SplitBucket(vector[bucket]);
                
                if (--slots[slot] < 0)
                    throw new InvalidOperationException("Too many rerences were removed! The counter can't be negative.");

                vector[bucket] = MergeBucket(slots);
            }
        }

        public void Add(T key)
        {
            var bytes = GetBytes(key);

            var hash = hashProvider.GetHashCodes(bytes, hashTransformCount, size);

            for (uint i = 1; i <= hash.Length; i++)
            {
                ulong bucket = hash[i - 1] / bucketSize;
                ulong slot = hash[i - 1] % bucketSize;

                int[] slots = SplitBucket(vector[bucket]);

                if (++slots[slot] > 15)
                    throw new OverflowException("The number of references surpased the limit supported by this algorithm! Try increasing the size of the BloomFilter.");

                vector[bucket] = MergeBucket(slots);
            }
        }

        public bool Test(T key)
        {
            var bytes = GetBytes(key);

            var hash = hashProvider.GetHashCodes(bytes, hashTransformCount, size);

            for (uint i = 1; i <= hash.Length; i++)
            {
                ulong bucket = hash[i - 1] / bucketSize;
                ulong slot = hash[i - 1] % bucketSize;

                int[] slots = SplitBucket(vector[bucket]);
                if (slots[slot] == 0)
                    return false;
            }

            return true;
        }

        protected abstract byte[] GetBytes(T key);

        private int[] SplitBucket(byte bucketValue)
        {
            return new int[]{
                0x0F & bucketValue, // lo slot
                bucketValue >> 4      // hi slot
            };
        }

        private byte MergeBucket(int[] slotValues)
        {
            return (byte)((slotValues[1] << 4) // hi slot
                | (0x0F & slotValues[0]));     // lo slot
        }
    }
}
