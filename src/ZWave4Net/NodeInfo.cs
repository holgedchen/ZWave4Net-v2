﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ZWave4Net.CommandClasses;

namespace ZWave4Net
{
    public class NodeInfo : IPayloadSerializable
    {
        public BasicType BasicType { get; private set; }
        public GenericType GenericType { get; private set; }
        public SpecificType SpecificType { get; private set; }
        public CommandClass[] SupportedCommandClasses { get; private set; } = new CommandClass[0];

        void IPayloadSerializable.Read(PayloadReader reader)
        {
            var length = reader.ReadByte();
            BasicType = (BasicType)reader.ReadByte();
            GenericType = (GenericType)reader.ReadByte();

            var specificType = reader.ReadByte();
            if (specificType == 0)
            {
                SpecificType = SpecificType.NotUsed;
            }
            else
            {
                SpecificType = (SpecificType)((int)GenericType << 16 | specificType);
            }

            SupportedCommandClasses = reader
                .ReadBytes(reader.Length - reader.Position)
                .TakeWhile(x => x != 0xEF)
                .Select(x => (CommandClass)x)
                .ToArray();
        }

        void IPayloadSerializable.Write(PayloadWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
