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

        // each slot is 4 bytes (a "nibble"), so a bucket only has 2 slots 
        private const byte BUCKET_SIZE = 2;
        private const byte SLOT_SIZE = 4;
        private const byte LOW_SLOT_MASK = 0x0F;

        public CountingBloomFilter(uint size, int hashTransformCount)
        {
            this.size = size;
            this.hashTransformCount = hashTransformCount;

            uint vectorSize = (size / BUCKET_SIZE) + (size % BUCKET_SIZE);
            vector = new byte[vectorSize];
        }

        public void Delete(T item)
        {
            var bytes = GetBytes(item);

            var hash = hashProvider.GetHashCodes(bytes, hashTransformCount, size);

            for (uint i = 1; i <= hash.Length; i++)
            {
                // use integer division to determine which "bucket" of 8 bits the hash falls into
                ulong bucket = hash[i - 1] / BUCKET_SIZE;

                // use the remainder (1 or 0) to find which individual "nibble" to set
                ulong slot = hash[i - 1] % BUCKET_SIZE;

                // split the bucket into two "nibbles"
                int[] slots = SplitBucket(vector[bucket]);
                
                // decrement the correct "nibble"
                if (--slots[slot] < 0)
                    throw new InvalidOperationException("Too many rerences were removed! The counter can't be negative.");

                // update the bucket with the new, merged values
                vector[bucket] = MergeBucket(slots);
            }
        }

        public void Add(T key)
        {
            var bytes = GetBytes(key);

            var hash = hashProvider.GetHashCodes(bytes, hashTransformCount, size);

            for (uint i = 1; i <= hash.Length; i++)
            {
                // use integer division to determine which "bucket" of 8 bits the hash falls into
                ulong bucket = hash[i - 1] / BUCKET_SIZE;

                // use the remainder (1 or 0) to find which individual "nibble" to set
                ulong slot = hash[i - 1] % BUCKET_SIZE;

                // split the bucket into two "nibbles"
                int[] slots = SplitBucket(vector[bucket]);

                // increment the correct "nibble"
                if (++slots[slot] > 15)
                    throw new OverflowException("The number of references surpased the limit supported by this algorithm! Try increasing the size of the BloomFilter.");

                // update the bucket with the new, merged values
                vector[bucket] = MergeBucket(slots);
            }
        }

        public bool Test(T key)
        {
            var bytes = GetBytes(key);

            var hash = hashProvider.GetHashCodes(bytes, hashTransformCount, size);

            for (uint i = 1; i <= hash.Length; i++)
            {
                // use integer division to determine which "bucket" of 8 bits the hash falls into
                ulong bucket = hash[i - 1] / BUCKET_SIZE;

                // use the remainder (1 or 0) to find which individual "nibble" to set
                ulong slot = hash[i - 1] % BUCKET_SIZE;

                // split the bucket into two "nibbles"
                int[] slots = SplitBucket(vector[bucket]);

                // if the value is 0, then there's no match
                if (slots[slot] == 0)
                    return false;
            }

            // all hashes were matched in the vector
            return true;
        }

        protected abstract byte[] GetBytes(T key);

        private int[] SplitBucket(byte bucketValue)
        {
            return new int[]{
                LOW_SLOT_MASK & bucketValue, // lo slot
                bucketValue >> 4      // hi slot
            };
        }

        private byte MergeBucket(int[] slotValues)
        {
            return (byte)((slotValues[1] << 4) // hi slot
                | (LOW_SLOT_MASK & slotValues[0]));     // lo slot
        }
    }
}
