using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.Extentions;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions.Wireless_Packets;
using System.Threading;

namespace MRL.SSL.AIConsole.Engine
{
    class Program
    {
        public static bool withMatrix = false;
        public static bool Simulating = false;
        public static double MaxKickSpeed = StaticVariables.MaxKickSpeed;
        public static double OurKickSpeed = 2.5;
        static void Main(string[] args)
        {
            

            MergerParameters.Load();// mamad add
            RotateParametersNew.Load();// mamad add
            //LookupTable.Save();
            LookupTable.Load();
            ActiveParameters.Load();
            RotateParameters.Load();

            ControlParameters.Load("ControlParameters");
            StrategyInfo.Load("Aistrategy");
            //var st = new StrategyInfo("test", "defence", 2, 0.2, true, "defence in area");
            //WorldModel m = new WorldModel();
            //m.BallState = new SingleObjectState();
            //m.OurRobots = new Dictionary<int, SingleObjectState>();
            //m.OurRobots.Add(1, new SingleObjectState(ObjectType.OurRobot, new CommonClasses.MathLibrary.Position2D(1, 1), CommonClasses.MathLibrary.Vector2D.Zero, CommonClasses.MathLibrary.Vector2D.Zero, 2f, null));
            //DrawCollection c = new DrawCollection();
            //c.AddObject(new MRL.SSL.CommonClasses.MathLibrary.Position2D(2, 2));
            //st.DrawingInfo.Add(new StrategyDrawingInfo(m, c));


            //WorldModel m1 = new WorldModel();
            //m1.BallState = new SingleObjectState();
            //m1.OurRobots = new Dictionary<int, SingleObjectState>();
            //m1.OurRobots.Add(1, new SingleObjectState(ObjectType.OurRobot, new CommonClasses.MathLibrary.Position2D(0, 0), CommonClasses.MathLibrary.Vector2D.Zero, CommonClasses.MathLibrary.Vector2D.Zero, 2f, null));
            //DrawCollection c1 = new DrawCollection();
            //c1.AddObject(new MRL.SSL.CommonClasses.MathLibrary.Position2D(2, -2));
            //st.DrawingInfo.Add(new StrategyDrawingInfo(m1,c1));
            //StrategyInfo.Add(st);
            var list = System.IO.Ports.SerialPort.GetPortNames();
            //ActiveRoleSettingTemplate t= new ActiveRoleSettingTemplate();
            //t.RobotID = 1;
            //t.propeties["ali"] = 4;
            //ActiveRoleSettings.Default.Parameters.Add(1, t);
            LookUpTable.Default.Initialize();
            //TuneVariables.Default.Add("bool", true);

            if (TuneVariables.Default.Position2Ds == null)
                TuneVariables.Default.Position2Ds = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, MRL.SSL.CommonClasses.MathLibrary.Position2D>();
            if (TuneVariables.Default.Integers == null)
                TuneVariables.Default.Integers = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, int>();
            if (TuneVariables.Default.Doubles == null)
                TuneVariables.Default.Doubles = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, double>();
            TuneVariables.Default.Save();

            if (GameSettings.Default.DribleState == null)
            {
                GameSettings.Default.DribleState = new System.Collections.ArrayList();
                GameSettings.Default.DribleState.Add(DribleState.Horizontal);
                GameSettings.Default.DribleState.Add(DribleState.Target);
                GameSettings.Default.DribleState.Add(DribleState.Vertical);
            }
            if (GameSettings.Default.Engines == null)
            {
                GameSettings.Default.Engines = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<int, Engines>();
                GameSettings.Default.Engines.Add(0, new Engines(0, false, true));
                GameSettings.Default.Save();
            }

            //if (!TuneVariables.Default.Doubles.ContainsKey("RegionalReion"))
            //    TuneVariables.Default.Add("RegionalRegion", (double)2);
            //if (!TuneVariables.Default.Doubles.ContainsKey("MaxMarkDist"))
            //    TuneVariables.Default.Add("MaxMarkDist", (double)2);
            //if (!TuneVariables.Default.Doubles.ContainsKey("MarkFromDist"))
            //    TuneVariables.Default.Add("MarkFromDist", (double).5);
            Print();

            //   RecievedData.Initialize();
            EngineManager em = new EngineManager();
            //RemotingConfiguration.Configure("MRL.S;SL.AIConsole.exe.config", false);
            RefereeConnection rc = new RefereeConnection(StaticVariables.OldRefbox);
            while (true)
            {
                            string command = Console.ReadLine();
                if (command.Trim().ToLower() == "exit")
                {
                    break;
                }
                else if (command.Trim().ToLower() == "save")
                {
                    TuneVariables.Default.Save();
                    GameSettings.Default.Save();
                    AISettings.Default.Save();
                    LookUpTable.Default.Save();
                }
                else if (command.Length == 1)
                {
                    em.EnqueueCommand(command[0]);
                }
                else
                {
                    string[] parts = command.Split();
                    if (parts.Length == 0)
                        continue;
                    if (parts[0].Trim().ToLower() == "wl")
                    {
                        if (parts.Length == 1)
                            EngineManager.PortManager.Enabled = !EngineManager.PortManager.Enabled;
                        else
                        {
                            if (parts[1].Trim().ToLower() == "on")
                                EngineManager.PortManager.Enabled = true;
                            else if (parts[1].Trim().ToLower() == "off")
                                EngineManager.PortManager.Enabled = false;
                            else
                                EngineManager.PortManager.Enabled = !EngineManager.PortManager.Enabled;
                        }
                    }
                    if (parts[0].Trim().ToLower() == "ref")
                    {
                        if (parts.Length == 1)
                            rc.IgnoreRefereeBox = !rc.IgnoreRefereeBox;
                        else
                        {
                            if (parts[1].Trim().ToLower() == "on")
                                rc.IgnoreRefereeBox = true;
                            else if (parts[1].Trim().ToLower() == "off")
                                rc.IgnoreRefereeBox = false;
                            else
                                rc.IgnoreRefereeBox = !rc.IgnoreRefereeBox;
                        }
                    }
                }
            }
            rc.Dispose();
            em.Dispose();

            Environment.Exit(1);

        }

        public static void Print()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\t\t AI Settings From Configuration file\n");
            foreach (int item in GameSettings.Default.Engines.Keys)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\nReverse Side: " + GameSettings.Default.Engines[item].ReverseSide.ToString());
                Console.Write("\t Our Color: ");
                if (GameSettings.Default.Engines[item].ReverseColor)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(GameSettings.Default.Engines[item].ReverseColor ? "Yellow" : "Blue");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\nIs Simulating: " + Simulating.ToString());
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n\t\t Network Settings");
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write("\nAI Name: " + AISettings.Default.AiName);
            Console.Write("\nAI Port: " + AISettings.Default.x.ToString());
            Console.Write("\nCommand & Monitor Name: " + AISettings.Default.VisName);
            Console.Write("\nCommand & Monitor Port: " + AISettings.Default.VisPort.ToString());
            Console.Write("\nReferee mnulticast IP: " + AISettings.Default.RefIP);
            Console.Write("\nReferee mnulticast Port: " + AISettings.Default.RefPort.ToString());
            Console.Write("\nSSLVision mnulticast IP: " + AISettings.Default.SSLVisionIP);
            Console.Write("\nSSLVision mnulticast Port: " + AISettings.Default.SSLVisionPort.ToString());
            Console.Write("\nSimulator Name: " + AISettings.Default.SimulatorName);
            Console.Write("\nSimulator Recive Port: " + AISettings.Default.SimulatorRecievePort.ToString());
            Console.Write("\nSimulator Send Name: " + AISettings.Default.SimulatorSendPort.ToString());
            Console.Write("\n");
        }
    }
}
