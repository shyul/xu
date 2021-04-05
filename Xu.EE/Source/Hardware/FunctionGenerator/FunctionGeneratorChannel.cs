﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xu.EE
{
    public class FunctionGeneratorChannel : IOutputChannel
    {
        public FunctionGeneratorChannel(IFunctionGenerator fgen, string channelName)
        {
            ChannelName = channelName;
            FunctionGenerator = fgen;
        }

        public string ChannelName { get; }

        public IFunctionGenerator FunctionGenerator { get; }

        public void WriteSetting() => FunctionGenerator.FGEN_WriteSetting(ChannelName);

        public void ON() => FunctionGenerator.FGEN_ON(ChannelName);

        public void OFF() => FunctionGenerator.FGEN_OFF(ChannelName);




        public WaveFormType WaveFormType { get; set; } = WaveFormType.Triangle;

        public double Amplitude { get; set; }

        public double DcOffset { get; set; }

        public double Frequency { get; set; }

        public double DutyCycle { get; set; }





        public bool IsArbitrary { get; set; } = false;

        public double ArbitrarySampleRate { get; set; }

        public IEnumerable<double> ArbitraryWaveForm { get; set; }


    }
}
