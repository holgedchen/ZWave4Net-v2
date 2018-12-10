﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.CommandClasses
{
    public class BasicReport : NodeReport
    {
        public byte Value { get; private set; }

        protected override void Read(PayloadReader reader)
        {
            reader.ReadByte();
        }
    }
}