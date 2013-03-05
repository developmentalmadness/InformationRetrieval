using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvertedIndex.Indexing;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class Index_Fixture
	{
		private ISearchIndex idx = new Index();

		[TestFixtureSetUp]
		public void Init()
		{
			idx.Add("The quick brown fox named Peter jumped over the two lazy dogs.");
			idx.Add("Peter Piper picked a peck of pickeled peppers. If Peter Piper picked a peck of pickeled peppers, how many peppers did Peter Piper pick?");
			idx.Add("If it walks like a duck and quacks like a duck, it must be a duck.");
			idx.Add("Beijing Duck is mostly prized for the thin, crispy duck skin with authentic versions of the dish serving mostly the skin.");
			idx.Add("Bugs' ascension to stardom also prompted the Warner animators to recast Daffy Duck as the rabbit's rival, intensely jealous and determined to steal back the spotlight while Bugs remained indifferent to the duck's jealousy, or used it to his advantage. This turned out to be the recipe for the success of the duo.");
			idx.Add("6:25 PM 1/7/2007 blog entry: I found this great recipe for Rabbit Braised in Wine on cookingforengineers.com.");
			idx.Add("Last week Li has shown you how to make the Sechuan duck. Today we'll be making Chinese dumplings (Jiaozi), a popular dish that I had a chance to try last summer in Beijing. There are many recipies for Jiaozi.");
		}

		[Test, Category("Index")]
		public void duck_search()
		{
			var results = idx.Search("beijing duck recipe");
			Assert.AreEqual(5, results.Count());
		}

		[Test, Category("Index")]
		public void two_peter_results()
		{
			var results = idx.Search("Peter");
			Assert.AreEqual(2, results.Count());
		}

		[Test, Category("Index")]
		public void document_two_is_first()
		{
			// only registers a single instance of peter in the document - this means it looses since the document is longer
			var results = idx.Search("Peter");
			Assert.AreEqual(2, results.First().Item1);
		}

		[Test, Category("Index")]
		public void one_lazy_dog_result()
		{
			var results = idx.Search("lazy dogs");
			Assert.AreEqual(1, results.Count());
		}

		[Test, Category("Index")]
		public void one_pepper_result()
		{
			var results = idx.Search("peppers");
			Assert.AreEqual(1, results.Count());
		}

		[Test, Category("Index")]
		public void correct_lazy_dog_result()
		{
			var results = idx.Search("lazy dogs");
			Assert.AreEqual(1, results.First().Item1);
		}

		[Test, Category("Index")]
		public void correct_pepper_result()
		{
			var results = idx.Search("peppers");
			Assert.AreEqual(2, results.First().Item1);
		}
	}
}
