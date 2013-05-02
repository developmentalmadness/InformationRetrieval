using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public class BloomFilter<T> : IBloomFilter<T>
    {
        private IHashFunctionProvider<T> hashProvider;
        byte[] vector;

        public BloomFilter(IHashFunctionProvider<T> hashProvider, int size)
        {
            this.hashProvider = hashProvider;
            vector = new byte[size / 8];
        }

        public void Add(T key)
        {
            int hash = hashProvider.GetHashCode(key);

            int bucket = hash / 8;
            byte slot = (byte) (hash * 8);

            vector[bucket] = (byte) (vector[bucket] | slot);
        }

        public bool Test(T key)
        {
            int hash = hashProvider.GetHashCode(key);
            
            int bucket = hash / 8;
            byte slot = (byte) (hash * 8);
            
            return (vector[bucket] & slot) != 0;
        }
    }
}
