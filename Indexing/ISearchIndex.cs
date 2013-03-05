using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
	public interface ISearchIndex
	{
		void Add(String document);
		IEnumerable<Tuple<Int32, Double>> Search(String query);
	}
}
