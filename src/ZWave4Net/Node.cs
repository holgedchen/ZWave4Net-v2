﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZWave4Net.Channel;
using ZWave4Net.Channel.Protocol;
using System.Reactive.Linq;

namespace ZWave4Net
{
    public class Node : Endpoint, IEquatable<Node>
    {
        public byte NodeID { get; private set; }
        public BasicType BasicType { get; private set; }
        public GenericType GenericType { get; private set; }
        public SpecificType SpecificType { get; private set; }
        public Security Security { get; private set; }
        public bool IsListening { get; private set; }
        public EndpointCollection Endpoints { get; private set; }

        public Node(byte nodeID, ZWaveController controller) : base(controller)
        {
            NodeID = nodeID;
            Endpoints = new EndpointCollection(this);
        }

        public async Task Initialize()
        {
            var command = new ControllerRequest(Function.GetNodeProtocolInfo, new Payload(NodeID));
            var protocolInfo = await Channel.Send<NodeProtocolInfo>(command, CancellationToken.None);
            BasicType = protocolInfo.BasicType;
            GenericType = protocolInfo.GenericType;
            SpecificType = protocolInfo.SpecificType;
            Security = protocolInfo.Security;
            IsListening = protocolInfo.IsListening;
        }

        public bool IsController
        {
            get { return NodeID == Controller.NodeID; }
        }

        public Endpoint CreateEndpoint(byte endpointID)
        {
            return EndpointFactory.CreateEndpoint(endpointID, this);
        }

        public async Task<NodeInfo> GetNodeInfo(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Channel.SendRequestNodeInfo(NodeID, cancellationToken);
        }

        public async Task<Node[]> GetNeighbours(CancellationToken cancellationToken = default(CancellationToken))
        {
            var results = new List<Node>();

            var command = new ControllerRequest(Function.GetRoutingTableLine, new Payload(NodeID));

            // send request
            var response = await Channel.Send<Payload>(command, cancellationToken);

            var bits = new BitArray(response.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    results.Add(Controller.Nodes[(byte)(i + 1)]);
                }
            }
            return results.ToArray();
        }


        public async Task<NeighborUpdateStatus> RequestNeighborUpdate(IProgress<NeighborUpdateStatus> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Channel.SendRequestNeighborUpdate(NodeID, progress, cancellationToken);
        }

        public IObservable<NodeUpdate> Updates
        {
            get { return Channel.ReceiveNodeUpdates(NodeID); }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public bool Equals(Node other)
        {
            return other != null &&
                   NodeID == other.NodeID;
        }

        public override int GetHashCode()
        {
            return -1960697928 + NodeID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{NodeID:D3}";
        }

        public static bool operator ==(Node node1, Node node2)
        {
            return EqualityComparer<Node>.Default.Equals(node1, node2);
        }

        public static bool operator !=(Node node1, Node node2)
        {
            return !(node1 == node2);
        }
    }
}
