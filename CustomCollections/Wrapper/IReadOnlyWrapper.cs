﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCollections
{
    public interface IReadOnlyWrapper<out T>
    {
        T Item { get; }
    }
}
