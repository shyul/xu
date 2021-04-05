﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xu.EE
{
    public abstract class OscilloscopeDigitalChannel : IChannel
    {
        public string ChannelName { get; }

        public void WriteSetting() { }

        public IOscilloscope Oscilloscope { get; }

        public double SampleRate { get; set; }
    }
}
