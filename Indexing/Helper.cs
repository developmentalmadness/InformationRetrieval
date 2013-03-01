using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex.Indexing
{
	public static class Helper
	{
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

		public static IDictionary<String, IEnumerable<Int32>> Tokenize(this String data)
		{
			var histogram = new Dictionary<String, IEnumerable<Int32>>();
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

					if (histogram.ContainsKey(t) == false)
						histogram.Add(t, new List<Int32>());

					((IList<Int32>)histogram[t]).Add(index++);
				}

			}

			return histogram;
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
