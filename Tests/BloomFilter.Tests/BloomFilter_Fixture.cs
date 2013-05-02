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
            var target = new StringBloomFilter(100000, 3);

            target.Add("Orange");

            Assert.IsTrue(target.Test("Orange"));
        }
    }
}
