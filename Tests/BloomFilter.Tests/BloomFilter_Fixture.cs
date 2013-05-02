using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    [TestFixture]
    public class BloomFilter_Fixture
    {
        [Test, Category("BloomFilter")]
        public void Test_ReturnsTrueForAddedKey()
        {
            var factory = new BloomFilterFactory();

            var target = factory.Create<String>(100000);

            target.Add("Orange");

            Assert.IsTrue(target.Test("Orange"));
        }
    }
}
