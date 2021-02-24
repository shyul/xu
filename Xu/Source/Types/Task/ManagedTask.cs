﻿/// ***************************************************************************
/// Shared Libraries and Utilities
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xu
{
    
    public enum ManagedTaskStatus : int 
    {
        Created,

        Running,

        Paused,
        
        Cancelled,

        Finished,
    }

    public abstract class ManagedTask
    {
        public abstract void Start();

        public abstract void Pause();

        public abstract void Cancel();

        public ManagedTaskStatus Status { get; set; } = ManagedTaskStatus.Created;

        // Merge with Schedule???

        // Progress Report!!

        // Log List
    }
}
