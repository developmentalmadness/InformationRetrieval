using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public class BloomFilterFactory
    {
        private Dictionary<Type, Object> hashProviders = new Dictionary<Type, Object>();

        public void RegisterHashProvider<T>(Func<T, ulong> provider)
        {
            hashProviders.Add(typeof(T), provider);
        }

        public IBloomFilter<T> Create<T>(int size)
        {
            var type = typeof(T);

            IHashFunctionProvider<T> provider = null;
            if(hashProviders.ContainsKey(type))
                provider = hashProviders[type] as IHashFunctionProvider<T>;
            
            if (provider == null)
                throw new InvalidOperationException(
                    String.Format("Cannot construct a filter for the type '{0}'"
                    + " because there is no registered IHashFunctionProvider<T> for the specified type.", type));

            return (IBloomFilter<T>) Activator.CreateInstance(type, provider, size);
        }
    }
}
