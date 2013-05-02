using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /*
     *  FIXME: This has a code smell to it. Maybe we can pass in a ByteConverter<T> 
     *  to the bloom filter instead of creating duplicate classes each time we want 
     *  to support both Filter types?
    */

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

    public class CountingStringBloomFilter : CountingBloomFilter<String>
    {
        public CountingStringBloomFilter(uint size, int hashTransformCount)
            : base (size, hashTransformCount)
        {

        }

        protected override byte[] GetBytes(string value)
        {
            return UTF8Encoding.UTF8.GetBytes(value);
        }
    }
}
