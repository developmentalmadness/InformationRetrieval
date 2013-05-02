﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    public interface IHashFunctionProvider
    {
        ulong[] GetHashCodes(byte[] value, int count, ulong max);
    }
}
