using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
	class Index
	{
		IDictionary<String, IList<Int64>> index = new Dictionary<String, IList<Int64>>();
		Int32 lastDocumentId = 0;

		public void Add(String document)
		{
			/*
			 If I store the idf with the term and the tf with each document
			 * in the list of matching docs would I then be able to use
			 * that to score the documents using the search terms?
			 * 
			 * I could omit terms with a high idf from the search
			 * terms and maybe the index (of course how would I know
			 * unless I kept the terms as part of the index?) - Most
			 * likely I'd have to at least come up with a partial list
			 * of stopwords to prevent having terms like 'The' waste
			 * valuable memory.
			 */
			var documentId = ++lastDocumentId;

			var tokens = document.Tokenize();

			/* this is almost O(2n), if we can update the index while 
			 * I tokenize I won't have to pass back over the results */
			foreach (var pair in tokens)
			{
				if (index.ContainsKey(pair.Key) == false)
					index.Add(pair.Key, new List<Int64>());

				var list = index[pair.Key];

				// merge documentId with document index for each term instance
				foreach (var item in pair.Value)
					list.Add(lastDocumentId.MergeInt32Value(item));
			}
		}

		public IEnumerable<Tuple<Int32, Double>> Search(String query)
		{
			// find matches
			var tokens = query.Tokenize();
			var documents = new Dictionary<Int32, IList<Tuple<String, Int32>>>();

			foreach (var item in tokens)
			{
				if (!index.ContainsKey(item.Key))
					continue;

				// merge results by document id
				foreach (var match in index[item.Key])
				{
					var key = match.SplitInt64Value();
					var docId = key[0];
					var docIdx = key[1];

					if (documents.ContainsKey(docId) == false)
						documents.Add(docId, new List<Tuple<String, Int32>>());

					var doc = documents[docId];

					doc.Add(new Tuple<string, int>(item.Key, docIdx));
				}
			}

			

			// score documents
			

			// sort by score and return


			return new Tuple<Int32, Double> [] { 
				new Tuple<Int32, Double>( 0, 0 ) 
			};
		}
	}
}
