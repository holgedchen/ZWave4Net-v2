﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Message from controller to host
    /// </summary>
    public abstract class ControllerMessage : Message
    {
        public ControllerMessage(Function function, byte[] payload) : base(function, payload)
        {
        }
    }
}