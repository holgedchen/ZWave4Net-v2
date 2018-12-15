﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using System.Reactive.Linq;

namespace ZWave4Net.CommandClasses
{
    public class CommandClassBase
    {
        public readonly CommandClass CommandClass;
        public readonly ZWaveController Controller;
        public readonly byte NodeID;
        public readonly byte EndpointID;

        public CommandClassBase(CommandClass commandClass, ZWaveController controller, byte nodeID, byte endpointID)
        {
            CommandClass = commandClass;
            Controller = controller;
            NodeID = nodeID;
            EndpointID = endpointID;
        }

        protected Task Send(ICommand command)
        {
            if (EndpointID != 0)
            {
                command = new EncapsulatedCommand(0, EndpointID, command);
            }

            return Controller.Channel.Send(NodeID, command);
        }

        protected async Task<T> Send<T>(ICommand command, Enum responseCommand) where T : NodeReport, new()
        {
            if (EndpointID != 0)
            {
                command = new EncapsulatedCommand(0, EndpointID, command);
            }

            var payload = await Controller.Channel.Send<Payload>(NodeID, command, Convert.ToByte(responseCommand));

            // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
            return new Payload(new[] { NodeID, EndpointID }.Concat(payload))
                .Deserialize<T>();
        }

        protected IObservable<T> Reports<T>(Enum command) where T : NodeReport, new()
        {
            var reportCommand = (ICommand)new Command(CommandClass, command);
            if (EndpointID != 0)
            {
                reportCommand = new EncapsulatedCommand(EndpointID, 0, reportCommand);
            }

            return Controller.Channel.ReceiveNodeEvents<Payload>(NodeID, reportCommand)
                // push NodeID and EndpointID in the payload so T has access to the node and the endpoint
                .Select(element => new Payload(new[] { NodeID, EndpointID }.Concat(element)))
                .Select(element => element.Deserialize<T>()); 
        }
    }
}
