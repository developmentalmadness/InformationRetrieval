using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Tests
{
    [TestFixture]
    public class CountingBloomFilter_Fixture
    {
        //TODO: need more tests around edge cases (values that hash to first, last for slot or whole vector, collisions, etc)

        [Test, Category("CountingBloomFilter")]
        public void SameKey_ReturnsTrue()
        {
            var target = new CountingStringBloomFilter(100000, 3);

            target.Add("Orange");

            Assert.IsTrue(target.Test("Orange"));
        }

        [Test, Category("CountingBloomFilter")]
        public void DifferentKey_ReturnsFalse()
        {
            var target = new CountingStringBloomFilter(100000, 3);

            target.Add("Orange");

            Assert.False(target.Test("Green"));
        }

        [Test, Category("CountingBloomFilter")]
        public void NotInSet_ReturnsFalse()
        {
            var target = new CountingStringBloomFilter(100000, 3);

            target.Add("black");
            target.Add("white");
            target.Add("green");
            target.Add("yellow");
            target.Add("orange");

            Assert.False(target.Test("violet"));
        }

        [Test, Category("CountingBloomFilter")]
        public void InSet_ReturnsTrue()
        {
            var target = new CountingStringBloomFilter(100000, 3);

            target.Add("black");
            target.Add("white");
            target.Add("green");
            target.Add("yellow");
            target.Add("orange");

            Assert.True(target.Test("orange"));
        }

        [Test, Category("CountingBloomFilter")]
        public void DeleteFromSet_ReturnsFalse()
        {
            var target = new CountingStringBloomFilter(100000, 3);

            target.Add("black");
            target.Add("white");
            target.Add("green");
            target.Add("yellow");
            target.Add("orange");

            target.Delete("black");

            Assert.False(target.Test("black"));
        }

        [Test, Category("CountingBloomFilter")]
        public void DeleteFromSet_OtherKeysStillReturnTrue()
        {
            var target = new CountingStringBloomFilter(100000, 3);

            target.Add("black");
            target.Add("white");
            target.Add("green");
            target.Add("yellow");
            target.Add("orange");

            target.Delete("black");

            Assert.True(target.Test("yellow"));
        }
    }
}
