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
    public class Tokenize_Fixture
    {
		IDictionary<Int32, IEnumerable<Int32>> target;

		[TestFixtureSetUp]
		public void Init()
		{
			target = "The\tquick brown fox jumped over the < three but > one <b>lazy</b>, Lazy dogs\r\npour avoir du thé.".Tokenize();
		}

		[Test, Category("Helper")]
		[ExpectedException(typeof(KeyNotFoundException))]
		public void excludes_stopwords()
		{
			Helper.GetTermId("the");
		}
		
		[Test, Category("Helper")]
		public void returns_unique_case_insensitive_keys()
		{
			Assert.AreEqual(2, target[Helper.GetTermId("lazy")].Count(), "Incorrect key count.");	
		}

		[Test, Category("Helper")]
		public void counts_term_frequency()
		{
			Assert.AreEqual(2, target[Helper.GetTermId("lazy")].Count(), "Incorrect frequency count.");
		}

		[Test, Category("Helper")]
		public void includes_term_index()
		{
			Assert.AreEqual(3, target[Helper.GetTermId("jumped")].FirstOrDefault(), "Incorrect term index.");
		}

		[Test, Category("Helper")]
		public void strips_html_formatting()
		{
			Assert.IsTrue(target.ContainsKey(Helper.GetTermId("lazy")), "Html was not stripped");
		}

		[Test, Category("Helper")]
		public void handles_paragraph_formatting()
		{
			Assert.IsTrue(target.ContainsKey(Helper.GetTermId("quick")), "Did not properly handle tabs.");
			Assert.IsTrue(target.ContainsKey(Helper.GetTermId("dogs")), "Did not properly handle crlf.");
		}
    }
}
