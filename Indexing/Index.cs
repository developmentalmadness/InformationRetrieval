using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
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
			private Int32 _term;
			private Int32 _maxTermFrequency;

			public TermBucket(Int32 term)
			{
				_locations = new List<Int64>();
				_term = term;
				_maxTermFrequency = 0;
			}

			/// <summary>
			/// The number of documents which contain the 
			/// term at least one time.
			/// </summary>
			public Int32 DocumentFrequency { get { return _locations.Count; } }

			/// <summary>
			/// The number of documents in the index 
			/// divided by the document frequency
			/// </summary>
			public Double GetInverseDocumentFrequency(int documentCount)
			{
				return 1.0d + Math.Log10((double) documentCount / (double) DocumentFrequency);
			}

			public Double NormalizeTermFrequency(int termFrequency)
			{
				return (double)termFrequency / (double)_maxTermFrequency;
			}

			public Double GetTfIdf(int totalDocuments, int termFrequency)
			{
				return NormalizeTermFrequency(termFrequency) * GetInverseDocumentFrequency(totalDocuments);
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
		}

		public void Add(String document)
		{
			var tokens = document.Tokenize();
			if (tokens.Keys.Count == 0)
				return;

			/*
			 * I could omit terms with a high idf from the search
			 * terms and maybe the index (of course how would I know
			 * unless I kept the terms as part of the index?) - Most
			 * likely I'd have to at least come up with a partial list
			 * of stopwords to prevent having terms like 'The' waste
			 * valuable memory.
			 * 
			 * tf = # of times a term (t) occurs in a document (d)
			 * df = # of times a term (t) occurs in the document index (D)
			 * 
			 * idf = 1 + log(|D| / df)
			 * tf-idf = tf * idf
			 * 
			 * see: http://www.ir-facility.org/scoring-and-ranking-techniques-tf-idf-term-weighting-and-cosine-similarity
			 */
			// NOTE: this is also == the total # of documents :)
			var documentId = ++documentCount;
			var doc = new Document(documentId);


			/* this is almost O(2n), if we can update the index while 
			 * I tokenize I won't have to pass back over the results 
			 
			 The reason we can't (currently) is because we're using Count as the low value for bucket.Locations ? Is there an easy way to increment the value? on each pass?*/
			foreach (var pair in tokens)
			{
				var termSeq = pair.Key;
				if (!searchIDX.ContainsKey(termSeq))
					searchIDX.Add(termSeq, new TermBucket(pair.Key));

				var bucket = searchIDX[termSeq];

				// we're currently assuming each document in is unique (no updates)
				bucket.AddLocation(documentId, pair.Value.Count());
				doc.AddTerm(termSeq, pair.Value.Count());

				searchIDX[termSeq] = bucket;

				// merge documentId with term count (tf)
			}

			documentIDX.Add(documentId, doc);
		}

		public IEnumerable<Tuple<Int32, Double>> Search(String query)
		{
			// find matches
			var tokens = query.Tokenize(); // Tokenize(query).Item1;
			var tokenCount = tokens.Count();
			if (tokenCount == 0)
				return new Tuple<Int32, Double>[0];

			var results = new Dictionary<Int32, Document>();
			var queryTfIdf = new Tuple<Int32, Double>[tokenCount];

			int tokenIndex = 0;
			foreach (var item in tokens)
			{
				var termId = item.Key;
				if (!searchIDX.ContainsKey(termId))
					continue;

				// merge results by document id
				var bucket = searchIDX[termId];

				queryTfIdf[tokenIndex] = new Tuple<int, double>(item.Key, bucket.GetTfIdf(documentCount, 1));

				foreach (var match in bucket.Locations)
				{
					var key = match.SplitInt64Value();
					var docId = key[0];

					if (results.ContainsKey(docId) == false)
						results.Add(docId, documentIDX[docId]);
				}

				tokenIndex++;
			}

			// score documents
			var scores = new List<Tuple<Int32, Double>>();
			foreach (var document in results)
			{
				var doc = documentIDX[document.Key];

				var score = CosineSimilarity(queryTfIdf, document.Value);

				scores.Add(new Tuple<Int32, Double>(doc.DocumentId, score));
			}

			// sort by score and return
			return scores.OrderByDescending(s => s.Item2).ToArray();
		}

		private Double CosineSimilarity(Tuple<int, double>[] queryTfIdf, Document document)
		{
			var superset = document.GetTerms().Union(queryTfIdf.Select(t => t.Item1));

			var vectorOne = CreateQueryFrequencyVector(superset, queryTfIdf);
			var vectorTwo = CreateDocumentFrequencyVector(superset, document);

			var dotProduct = DotProduct(vectorOne, vectorTwo);
			var productOfMagnitudes = ProductOfMagnitudes(vectorOne, vectorTwo);

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

		internal static double DotProduct(Double[] vectorOne, Double[] vectorTwo)
		{
			var sum = 0d;
			for (int i = 0; i < vectorOne.Length; i++)
				sum += vectorOne[i] * vectorTwo[i];
			return sum;
		}

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
