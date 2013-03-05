using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
	/// <summary>
	/// http://www.ir-facility.org/scoring-and-ranking-techniques-tf-idf-term-weighting-and-cosine-similarity
	/// </summary>
	public class Index : ISearchIndex
	{
		IDictionary<Int32, TermBucket> searchIDX = new Dictionary<Int32, TermBucket>();
		IDictionary<Int32, Document> documentIDX = new Dictionary<Int32, Document>();

		// this can double as the total # of documents
		Int32 documentCount = 0;

		[StructLayout(LayoutKind.Sequential, Pack=1)]
		private struct TermBucket
		{
			private IList<Int64> _locations;
			private Int32 _termid;
			private Int32 _maxTermFrequency;

			public TermBucket(Int32 term)
			{
				_locations = new List<Int64>();
				_termid = term;
				_maxTermFrequency = 0;
			}

			public Double GetTfIdf(int totalDocuments, int termFrequency)
			{
				var normalizedTermFrequency = (double)termFrequency / (double)_maxTermFrequency;
				var documentFrequency = (double)_locations.Count();
				var inverseDocumentFrequency = 1.0d + Math.Log10((double)totalDocuments / documentFrequency);
				return normalizedTermFrequency * inverseDocumentFrequency;
			}

			/// <summary>
			/// A list of term frequencies (tf) where the high
			/// is the document id and the low is the term frequency
			/// </summary>
			/// <remarks>
			/// Term Frequency (tf) is the number of times a term
			/// occurs in an individual document
			/// </remarks>
			public IEnumerable<Int64> Locations { get { return _locations; } }

			public void AddLocation(Int32 documentId, Int32 termFrequency)
			{
				_maxTermFrequency = Math.Max(termFrequency, _maxTermFrequency);
				_locations.Add(documentId.MergeInt32Value(termFrequency));
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct Document
		{
			private IDictionary<Int32, Int32> termset;
			private Int32 documentid;

			public Document(Int32 id)
			{
				termset = new Dictionary<int, Int32>();
				documentid = id;
			}

			public Int32 DocumentId { get { return documentid; } }

			public void AddTerm(Int32 termId, Int32 termFrequency)
			{
				termset.Add(termId, termFrequency);
			}

			public IEnumerable<Int32> GetTerms()
			{
				return termset.Keys;
			}

			public Int32 GetTermFrequency(Int32 termId)
			{
				return termset[termId];
			}

			public override int GetHashCode()
			{
				return documentid.GetHashCode();
			}
		}

		public void Add(String document)
		{
			var tokens = document.Tokenize();
			if (tokens.Keys.Count == 0)
				return;

			var documentId = ++documentCount;
			var doc = new Document(documentId);

			/* this is almost O(2n), if we can update the index while 
			 * I tokenize I won't have to pass back over the results 
			 
			 The reason we can't (currently) is because we're using Count as the low value for bucket.Locations? 
			 * Is there an easy way to increment the value? on each pass?*/
			foreach (var item in tokens)
			{
				var termId = item.Key;
				var frequency = item.Value.Count();

				if (!searchIDX.ContainsKey(termId))
					searchIDX.Add(termId, new TermBucket(termId));

				var bucket = searchIDX[termId];

				// we're currently assuming each document in is unique (no updates)
				bucket.AddLocation(documentId, item.Value.Count());
				doc.AddTerm(termId, frequency);

				searchIDX[termId] = bucket;
			}

			documentIDX.Add(documentId, doc);
		}

		public IEnumerable<Tuple<Int32, Double>> Search(String query)
		{
			// find matches
			var tokens = query.Tokenize();
			var tokenCount = tokens.Count();
			if (tokenCount == 0)
				return new Tuple<Int32, Double>[0];

			var results = new HashSet<Document>();
			var queryTfIdf = new Tuple<Int32, Double>[tokenCount];

			int tokenIndex = 0;
			foreach (var item in tokens)
			{
				var termId = item.Key;
				if (!searchIDX.ContainsKey(termId))
				{
					queryTfIdf[tokenIndex] = new Tuple<Int32, Double>(termId, 0.0d);
					continue;
				}

				// merge results by document id
				var bucket = searchIDX[termId];

				queryTfIdf[tokenIndex] = new Tuple<Int32, Double>(termId, bucket.GetTfIdf(documentCount, 1));

				foreach (var match in bucket.Locations)
				{
					var key = match.SplitInt64Value();
					var docId = key[0];

					var doc = documentIDX[docId];
					if (results.Contains(doc) == false)
						results.Add(doc);
				}

				tokenIndex++;
			}

			// score documents
			var scores = new List<Tuple<Int32, Double>>();
			foreach (var document in results)
			{
				var doc = documentIDX[document.DocumentId];

				var score = CosineSimilarity(queryTfIdf, document);

				scores.Add(new Tuple<Int32, Double>(doc.DocumentId, score));
			}

			// sort by score and return
			return scores.OrderByDescending(s => s.Item2).ToArray();
		}

		private Double CosineSimilarity(Tuple<int, double>[] queryTfIdf, Document document)
		{
			var superset = document.GetTerms().Union(queryTfIdf.Select(t => t.Item1));

			// normalize documents into term vectors for comparison
			var vectorOne = CreateQueryFrequencyVector(superset, queryTfIdf);
			var vectorTwo = CreateDocumentFrequencyVector(superset, document);

			// calculate the dot product of the two vectors ((V1[0] * V2[0]) + (V1[1] * V2[1]) ... + (V1[n] * V2[n])) 
			var dotProduct = DotProduct(vectorOne, vectorTwo);
			// calculate the product of the vector magnatudes (Sqrt(Sum(V1) * Sum(V2)))
			var productOfMagnitudes = ProductOfMagnitudes(vectorOne, vectorTwo);

			// return dot product normalized by the product of magnatudes
			return dotProduct / productOfMagnitudes;
		}

		internal static double ProductOfMagnitudes(Double[] vectorOne, Double[] vectorTwo)
		{
			var sumOne = 0d;
			var sumTwo = 0d;

			for (int i = 0; i < vectorOne.Length; i++)
			{
				sumOne += System.Math.Pow(vectorOne[i], 2);
				sumTwo += System.Math.Pow(vectorTwo[i], 2);
			}

			var product = sumOne * sumTwo;

			return System.Math.Sqrt(product);
		}

		/// <summary>
		/// Returns the dot product of two vectors
		/// </summary>
		/// <example>
		/// given two vectors: [1, 3, -5], [2, -1, -6]
		///     THEN: (1 * 2) + (3 * -1) * (-5 * -6) = 29
		/// </example>
		/// <param name="vectorOne"></param>
		/// <param name="vectorTwo"></param>
		/// <returns>the dot product of two vectors</returns>
		internal static double DotProduct(Double[] vectorOne, Double[] vectorTwo)
		{
			var sum = 0d;
			for (int i = 0; i < vectorOne.Length; i++)
				sum += vectorOne[i] * vectorTwo[i];
			return sum;
		}

		/// <summary>
		/// Retuns the frequency of terms in a document in relation to a superset of terms.
		/// </summary>
		/// <param name="superset">Some set of terms which includes at least all the terms in the document "value".</param>
		/// <param name="value">The document for which the vector will be calculated.</param>
		/// <returns>The frequency of each term from superset that is contained in value</returns>
		private Double[] CreateDocumentFrequencyVector(IEnumerable<Int32> superset, Document document)
		{
			Dictionary<Int32, Double> keyset = new Dictionary<Int32, Double>();
			foreach (var key in superset)
				keyset.Add(key, 0);

			foreach (var termId in document.GetTerms())
			{
				var tf = document.GetTermFrequency(termId);
				var tfidf = searchIDX[termId].GetTfIdf(documentCount, tf);
				keyset[termId] = tfidf;
			}

			return keyset.Values.ToArray();
		}

		/// <summary>
		/// Retuns the frequency of terms in a query in relation to a superset of terms.
		/// </summary>
		/// <param name="superset">Some set of terms which includes at least all the terms in the query "value".</param>
		/// <param name="value">The query for which the vector will be calculated.</param>
		/// <returns>The frequency of each term from superset that is contained in value</returns>
		internal static Double[] CreateQueryFrequencyVector(IEnumerable<Int32> superset, IEnumerable<Tuple<Int32, Double>> value)
		{
			Dictionary<Int32, Double> keyset = new Dictionary<Int32, Double>();
			foreach (var key in superset)
				keyset.Add(key, 0);

			foreach (var key in value)
			{
				var count = keyset[key.Item1];
				keyset[key.Item1] += key.Item2;
			}

			return keyset.Values.ToArray();
		}
	}
}
