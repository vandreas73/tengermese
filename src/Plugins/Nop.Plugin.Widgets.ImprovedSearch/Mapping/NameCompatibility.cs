﻿using System;
using System.Collections.Generic;
using Nop.Data.Mapping;

namespace Nop.Plugin.Widgets.ImprovedSearch.Mapping
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>();

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}