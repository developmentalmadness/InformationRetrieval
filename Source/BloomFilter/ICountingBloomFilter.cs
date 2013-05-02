using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public interface ICountingBloomFilter<T> : IBloomFilter<T>
    {
        void Delete(T item);
    }
}
