﻿/// ***************************************************************************
/// Shared Libraries and Utilities
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

namespace Xu
{
    public class StringColumn : Column
    {
        public StringColumn(string name) => Name = Label = name;

        public StringColumn(string name, string label)
        {
            Name = name;
            Label = label;
        }
    }
}
