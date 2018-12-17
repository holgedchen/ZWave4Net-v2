﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZWave4Net;
using ZWave4Net.Channel;
using ZWave4Net.CommandClasses;
using ZWave4Net.Diagnostics;

namespace ZWaveDumper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Logging.Factory.Subscribe((message) =>
            {
                if (message.Level > LogLevel.Info)
                {
                    WriteLogRecord(message);
                }
            });

            var port = new SerialPort(SerialPort.GetPortNames().Where(element => element != "COM1").First());
            var controller = new ZWaveController(port);

            try
            {
                await controller.Open();

                await Dump(controller);
                WriteLine();

                foreach (var node in controller.Nodes)
                {
                    await Dump(node);
                    WriteLine();
                }
            }
            catch(Exception ex)
            {
                WriteError(ex);
            }
            finally
            {
                await controller.Close();
            }
            Console.ReadLine();
        }

        private static Task Dump(ZWaveController controller)
        {
            WriteInfo($"Controller");
            WriteSeparator();
            WriteInfo($"Version: {controller.Version}");
            WriteInfo($"ChipType: {controller.ChipType}");
            WriteInfo($"HomeID: {controller.HomeID:X}");
            WriteInfo($"NodeID: {controller.NodeID}");

            return Task.CompletedTask;
        }

        private static async Task Dump(Node node)
        {
            WriteInfo($"node {node}");
            WriteSeparator();

            WriteInfo($"ProtocolInfo: Specific = {node.SpecificType}, Generic = {node.GenericType}, Basic = {node.BasicType}, Listening = {node.IsListening} ");

            var neighbours = await node.GetNeighbours();
            WriteInfo($"Neighbours: {string.Join(", ", neighbours.Cast<object>().ToArray())}");

            if (!node.IsController && node.IsListening)
            {
                try
                {
                    var nodeInfo = await node.GetNodeInfo();
                    var commandClasses = nodeInfo.SupportedCommandClasses;
                    WriteInfo($"Supported CommandClasses: {string.Join(", ", commandClasses)}");

                    if (commandClasses.Contains(CommandClass.Basic))
                    {
                        await Dump(node as IBasic);
                    }

                    if (commandClasses.Contains(CommandClass.Association))
                    {
                        await Dump(node as IAssociation);
                    }

                    if (commandClasses.Contains(CommandClass.BinarySwitch))
                    {
                        await Dump(node as IBinarySwitch);
                    }
                }
                catch (Exception ex)
                {
                    WriteError(ex);
                }
            }
        }

        private static async Task Dump(IBasic basic)
        {
            try
            {
                var report = await basic.Get();
                WriteInfo($"Basic: {report.Value}");
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private static async Task Dump(IAssociation association)
        {
            try
            {
                var groups = await association.GroupingsGet();
                WriteInfo($"Association: Groups = {groups.SupportedGroupings}");

                for (byte group = 1; group <= groups.SupportedGroupings; group++)
                {
                    var nodes = await association.Get(group);
                    WriteInfo($"Association: {string.Join(", ", nodes)}");
                }
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private static async Task Dump(IBinarySwitch binarySwitch)
        {
            try
            {
                var report = await binarySwitch.Get();
                WriteInfo($"BinarySwitch: {report.Value}");
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private static void WriteLogRecord(LogRecord record)
        {
            switch (record.Level)
            {
                case LogLevel.Debug:
                    WriteDebug(record);
                    break;
                case LogLevel.Info:
                    WriteInfo(record);
                    break;
                case LogLevel.Warning:
                    WriteWarning(record);
                    break;
                case LogLevel.Error:
                    WriteError(record);
                    break;
            }
        }

        private static void WriteLine()
        {
            Console.WriteLine();
        }

        private static void WriteSeparator()
        {
            Console.WriteLine(new string('-', 80));
        }

        private static void WriteDebug(object state)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(state);
        }

        private static void WriteInfo(object state)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(state);
        }

        private static void WriteWarning(object state)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(state);
        }

        private static void WriteError(object state)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(state);
        }
    }
}