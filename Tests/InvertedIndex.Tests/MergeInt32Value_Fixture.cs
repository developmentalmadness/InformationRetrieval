using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using InvertedIndex.Indexing;

namespace Tests
{
	[TestFixture]
	public class MergeInt32Value_Fixture
	{
		private Int64 target = 18129486541528423;
		private Int32[] splitTarget = new Int32[] { 4221100, 88382823 };

		[Test, Category("MergeInt32")]
		public void matches_split()
		{
			var actual = target.SplitInt64Value();
			Assert.AreEqual(splitTarget, actual);
		}

		[Test, Category("MergeInt32")]
		public void matches_merge()
		{
			var actual = splitTarget[0].MergeInt32Value(splitTarget[1]);
			Assert.AreEqual(target, actual);
		}
	}
}
