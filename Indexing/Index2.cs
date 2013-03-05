using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
	public class Index2 : ISearchIndex
	{
		IDictionary<Int32, TermBucket> searchIDX = new Dictionary<Int32, TermBucket>();
		IDictionary<Int32, Document> documentIDX = new Dictionary<Int32, Document>();

		Int32 documentCount = 0;

		#region ISearchIndex Members

		public void Add(string document)
		{
			var tokens = document.Tokenize();
			if (tokens.Item1.Keys.Count == 0)
				return;

			var documentId = ++documentCount;
			var doc = new Document(documentId);

			foreach (var pair in tokens.Item1)
			{
				var termId = pair.Key;
				if (!searchIDX.ContainsKey(termId))
					searchIDX.Add(termId, new TermBucket(termId));

				var bucket = searchIDX[termId];

				bucket.AddLocation(documentId, pair.Value.Count());
				doc.AddTerm(termId, bucket.NormalizeTermFrequency(pair.Value.Count()));

				searchIDX[termId] = bucket;
			}

			documentIDX.Add(documentId, doc);
		}

		public IEnumerable<Tuple<int, double>> Search(string query)
		{
			var tokens = query.Tokenize().Item1;
			var tokenCount = tokens.Count();
			if (tokenCount == 0)
				return new Tuple<Int32, Double>[0];

			var results = new Dictionary<Int32, Tuple<Int32, Double>>();
			
			
			
			
			return new Tuple<Int32, Double>[0];
		}

		#endregion

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct Document
		{
			private IDictionary<Int32, Double> termset;
			private Int32 documentid;

			public Document(Int32 id)
			{
				termset = new Dictionary<int, double>();
				documentid = id;
			}

			public Int32 DocumentId { get { return documentid; } }

			public void AddTerm(Int32 termId, Double termFrequency)
			{
				termset.Add(termId, termFrequency);
			}
		}


		[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
				return 1.0d + Math.Log((double)documentCount / (double)DocumentFrequency);
			}

			public Double NormalizeTermFrequency(int termFrequency)
			{
				return (double)termFrequency / (double)_maxTermFrequency;
			}

			public Double GetTfIdf(int totalDocuments, int termFrequency)
			{
				return NormalizeTermFrequency(termFrequency) * GetInverseDocumentFrequency(totalDocuments);
			}

			/* where is this going to be used? Do I really need 
			 * it if I'm not going to actually return the document 
			 * to the user? The actual document could be returned 
			 * via some other key-value store
			 */
			public Int32 Term { get { return _term; } }

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

	}
}
