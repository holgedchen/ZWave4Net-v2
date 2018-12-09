﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ZWave4Net.Channel.Protocol
{
    /// <summary>
    /// Message from host to controller
    /// </summary>
    public class RequestMessage : Message
    {
        public RequestMessage(ByteArray payload) : base(payload)
        {
        }

        public RequestMessage() : this(ByteArray.Empty)
        {
        }

        public override string ToString()
        {
            return $"Request {base.ToString()}";
        }
    }
}
