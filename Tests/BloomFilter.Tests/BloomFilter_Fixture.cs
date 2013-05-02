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
        public void SameKey_ReturnsTrue()
        {
            var target = new StringBloomFilter(100000, 3);

            target.Add("Orange");

            Assert.IsTrue(target.Test("Orange"));
        }

        [Test, Category("BloomFilter")]
        public void DifferentKey_ReturnsFalse()
        {
            var target = new StringBloomFilter(100000, 3);

            target.Add("Orange");

            Assert.False(target.Test("Green"));
        }

        [Test, Category("BloomFilter")]
        public void NotInSet_ReturnsFalse()
        {
            var target = new StringBloomFilter(100000, 3);

            target.Add("black");
            target.Add("white");
            target.Add("green");
            target.Add("yellow");
            target.Add("orange");

            Assert.False(target.Test("violet"));
        }

        [Test, Category("BloomFilter")]
        public void InSet_ReturnsTrue()
        {
            var target = new StringBloomFilter(100000, 3);

            target.Add("black");
            target.Add("white");
            target.Add("green");
            target.Add("yellow");
            target.Add("orange");

            Assert.True(target.Test("orange"));
        }
    }
}
