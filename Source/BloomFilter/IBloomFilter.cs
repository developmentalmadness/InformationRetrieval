using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public interface IBloomFilter<T>
    {
        void Add(T key);
        bool Test(T key);
    }
}
