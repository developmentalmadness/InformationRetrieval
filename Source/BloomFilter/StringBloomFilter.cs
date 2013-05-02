using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public class StringBloomFilter : BloomFilter<String>
    {
        public StringBloomFilter(uint size, int hashTransformCount)
            : base (size, hashTransformCount)
        {

        }

        protected override byte[] GetBytes(string value)
        {
            return UTF8Encoding.UTF8.GetBytes(value);
        }
    }
}
