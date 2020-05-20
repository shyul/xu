﻿/// ***************************************************************************
/// Shared Libraries and Utilities
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;

namespace Xu
{
    public interface IStackable : IItem, ICoordinatable
    {
        Point DropMenuOriginPoint { get; }

        bool IsSectionEnd { get; set; }

        bool IsLineEnd { get; set; }

        int StackedY { get; set; }

        int SectionIndex { get; set; }
    }
}
