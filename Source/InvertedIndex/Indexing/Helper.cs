using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
	public static class Helper
	{
		private static HashSet<String> stopwords = new HashSet<String>(new String[]{
			"a", "about", "above", "above", "across", "after", "afterwards", "again", "against", 
			"all", "almost", "alone", "along", "already", "also","although","always","am","among", 
			"amongst", "amoungst", "amount",  "an", "and", "another", "any","anyhow","anyone",
			"anything","anyway", "anywhere", "are", "around", "as",  "at", "back","be","became", 
			"because","become","becomes", "becoming", "been", "before", "beforehand", "behind", 
			"being", "below", "beside", "besides", "between", "beyond", "bill", "both", "bottom",
			"but", "by", "call", "can", "cannot", "cant", "co", "con", "could", "couldnt", "cry", 
			"de", "describe", "detail", "do", "done", "down", "due", "during", "each", "eg", "eight", 
			"either", "eleven","else", "elsewhere", "empty", "enough", "etc", "even", "ever", 
			"every", "everyone", "everything", "everywhere", "except", "few", "fifteen", "fify", 
			"fill", "find", "fire", "first", "five", "for", "former", "formerly", "forty", "found", 
			"four", "from", "front", "full", "further", "get", "give", "go", "had", "has", "hasnt", 
			"have", "he", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", 
			"herself", "him", "himself", "his", "how", "however", "hundred", "ie", "if", "in", "inc", 
			"indeed", "interest", "into", "is", "it", "its", "itself", "keep", "last", "latter", 
			"latterly", "least", "less", "ltd", "made", "many", "may", "me", "meanwhile", "might", 
			"mill", "mine", "more", "moreover", "most", "mostly", "move", "much", "must", "my", 
			"myself", "name", "namely", "neither", "never", "nevertheless", "next", "nine", "no", 
			"nobody", "none", "noone", "nor", "not", "nothing", "now", "nowhere", "of", "off", 
			"often", "on", "once", "one", "only", "onto", "or", "other", "others", "otherwise", "our", 
			"ours", "ourselves", "out", "over", "own","part", "per", "perhaps", "please", "put", 
			"rather", "re", "same", "see", "seem", "seemed", "seeming", "seems", "serious", "several",
			"she", "should", "show", "side", "since", "sincere", "six", "sixty", "so", "some", 
			"somehow", "someone", "something", "sometime", "sometimes", "somewhere", "still", "such",
			"system", "take", "ten", "than", "that", "the", "their", "them", "themselves", "then", 
			"thence", "there", "thereafter", "thereby", "therefore", "therein", "thereupon", "these", 
			"they", "thickv", "thin", "third", "this", "those", "though", "three", "through", 
			"throughout", "thru", "thus", "to", "together", "too", "top", "toward", "towards", 
			"twelve", "twenty", "two", "un", "under", "until", "up", "upon", "us", "very", "via",
			"was", "we", "well", "were", "what", "whatever", "when", "whence", "whenever", "where",
			"whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever", "whether",
			"which", "while", "whither", "who", "whoever", "whole", "whom", "whose", "why", "will",
			"with", "within", "without", "would", "yet", "you", "your", "yours", "yourself", 
			"yourselves", "the" 
		});

		public static Int64 MergeInt32Value(this Int32 high, Int32 low)
		{
			ulong unsignedKey = (((ulong)high) << 32 | (uint)low);
			return (long)unsignedKey;
		}

		public static Int32[] SplitInt64Value(this Int64 value)
		{
			ulong unsignedKey = (ulong)value;
			return new Int32[] { (int)(uint)(unsignedKey >> 32), (int)(uint)(unsignedKey & 0xffffffffUL) };
		}

		public static IDictionary<Int32, IEnumerable<Int32>> Tokenize(this String data)
		{
			var histogram = new Dictionary<Int32, IEnumerable<Int32>>();
			StringBuilder term = new StringBuilder();

			for (int i = 0, index = 0; i < data.Length; i++)
			{
				char c = data[i];

				switch(c)
				{
					case '<':
						i = skipHtmlTag(ref data, i);
						continue;
					case '>':
					case '\r':
					case '\n':
					case '\t':
					case '.':
					case ',':
						c = ' ';
						break;
				}

				if (Char.IsLetter(c))
				{
					if (Char.IsUpper(c))
						term.Append(Char.ToLower(c));
					else
						term.Append(c);
				}
				else if (Char.IsSeparator(c) == false)
					term.Append(c);

				if (Char.IsSeparator(c) || i == data.Length - 1)
				{
					// new term
					var t = term.ToString();
					term.Clear();

					if (String.IsNullOrEmpty(t))
						continue;

					// filter out stop words
					if (stopwords.Contains(t))
						continue;

					// TODO: add stemming support
					var sequence = GetTermSequence(t);
					if (histogram.ContainsKey(sequence) == false)
						histogram.Add(sequence, new List<Int32>());

					((IList<Int32>)histogram[sequence]).Add(index++);
				}

			}

			return histogram;
		}

		private static IDictionary<String, Int32> termIDX = new Dictionary<String, Int32>();
		private static Int32 termSequence = 0;
		private static object syncLock = new object();
		private static Int32 GetTermSequence(string term)
		{
			int sequence = 0;
			if (!termIDX.TryGetValue(term, out sequence))
			{
				lock (syncLock)
				{
					if (!termIDX.TryGetValue(term, out sequence))
					{
						sequence = ++termSequence;
						termIDX.Add(term, sequence);
					}
				}
			}

			return sequence;
		}

		public static Int32 GetTermId(String term)
		{
			return termIDX[term.ToLower()];
		}

		private static int skipHtmlTag(ref String data, int startAt)
		{
			for (int i = startAt; i < data.Length; i++)
			{
				switch (data[i])
				{
					case '<':
					case '/':
						continue;
					case '>':
						return i;
				}

				if (Char.IsLetter(data[i]) == false)
					return startAt;
			}

			return startAt;
		}
	}
}
