using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public class CountingBloomFilter<T> : ICountingBloomFilter<T>
    {
        private IHashFunctionProvider<T> hashProvider;

        public CountingBloomFilter(IHashFunctionProvider<T> hashProvider)
        {
            this.hashProvider = hashProvider;
        }

        public void Delete(T item)
        {
            throw new NotImplementedException();
        }

        public void Add(T key)
        {
            throw new NotImplementedException();
        }

        public bool Test(T key)
        {
            throw new NotImplementedException();
        }
    }
}
