using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.IO;
using System.Xml;

namespace MRL.SSL.AIConsole.Engine
{
    public class PenaltyGoalKeeperLearningUnit
    {
        private const double centerOrCornerCounterThreshold = .7;
        private const double leftOrRightCounterThreshold = .7;
        private const double openCloseCounterThreshold = .8;
        private const double shootOrTurnThreshhold = 4;
        private const double nearOrFardistance = 0.14
            ;
        private const int staticDifferentialTime = 3;
        private const int immediateOrWaitedTime = 2;
        private const double BallThreshold = .14;//for Go
        private const double openThreshold = 25;
        private const double closeThreshold = 3;
        private const double cornerDiff = 9;
        private const double centerDiff = 5;
        private const double diffSize = .1;

        List<SingleObjectState> penaltyShooterStatesInWaitingNextStepList = new List<SingleObjectState>();
        static List<PenaltyOutputLearning> PenaltyOutPutLearningList = new List<PenaltyOutputLearning>();
        List<SingleObjectState> penaltyShooterStatesInGoNextStepList = new List<SingleObjectState>();
        List<SingleObjectState> penaltyShooterStatesInWaitingList = new List<SingleObjectState>();
        public static PenaltyOutputLearning DecisionMakingOutPut = new PenaltyOutputLearning();
        List<SingleObjectState> penaltyShooterStatesInGoList = new List<SingleObjectState>();
        static PenaltyOutputLearning penaltyOutputLearning = new PenaltyOutputLearning();
        List<Position2D> GoaliePositionsInWaitingNextStepList = new List<Position2D>();
        List<Position2D> ballPositionsInWaitingNextStepList = new List<Position2D>();
        List<Position2D> GoaliePositionsInGoNextStepList = new List<Position2D>();
        List<Position2D> ballPositionsInGoNextStepList = new List<Position2D>();
        List<Position2D> GoaliePositionsInWaitingList = new List<Position2D>();
        List<Position2D> ballPositionsInWaitingList = new List<Position2D>();
        static List<CloseOROpen> closeOrOpenList = new List<CloseOROpen>();
        static List<LeftOrRight> leftOrRightList = new List<LeftOrRight>();
        List<Position2D> GoaliePositionsInGoList = new List<Position2D>();
        public static PatternType patternTypeGlobal = PatternType.Random;
        List<Position2D> ballPositionsInGoList = new List<Position2D>();
        public static penaltyType penaltyLogic = penaltyType.bigAxis;
        private static bool lastRetryFrequencyMotion = false;
        public static List<int> timesList = new List<int>();
        private static bool firstPenaltyArrived = false;
        public static bool inFrequencyMotion = true;
        public static bool inExecutionMotion = true;
        Position2D lastBallstate = new Position2D();
        List<int> KickFramesList = new List<int>();
        public static bool inLearnMotion = true;
        public static bool OnceExecute2 = true;
        public static bool OnceExecute = true;
        public static int penaltyCount = 0;
        public static bool Succes = false;
        public static bool kicked = false;
        public static int Confidence = 0;
        public static int? ShooterID = 0;
        public static int kickFrame = 0;
        public static int plusTime = 0;
        private bool firsttime = true;
        Random random = new Random();
        static int lastOppScore = 0;
        public static bool firstTime = true;
        int currentState = 0;

        private ShootOrTurn ShootORTurn(List<SingleObjectState> penaltyShooterStatesInGo)
        {
            if (Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(penaltyShooterStatesInGo[0].Angle.Value, 1), Vector2D.FromAngleSize(penaltyShooterStatesInGo[kickFrame].Angle.Value, 1)) < shootOrTurnThreshhold)
                return ShootOrTurn.ShootAtLook;
            else
                return ShootOrTurn.TurnAndShoot;
        }

        private immediateORWaited immediateOrWaitedFunc()
        {
            if (kickFrame < immediateOrWaitedTime)
            {
                return immediateORWaited.immediate;
            }
            else
            {
                return immediateORWaited.Waited;
            }
        }

        /// <summary>
        /// Save Position Data in XML File
        /// </summary>
        /// <param name="model">World Model</param>
        private void SaveData(WorldModel model)
        {
            Random randomNumber = new Random();
            string fileName = "PenaltyData.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("PenaltyData");
                writeValue(writer, "CountOfDataGo", ballPositionsInGoNextStepList.Count);
                writeValue(writer, "CountOfDataWait", ballPositionsInWaitingNextStepList.Count);
                for (int i = 0 ; i < ballPositionsInGoNextStepList.Count ; i++)
                {
                    writeValue(writer, "ballPositionsInGoX" + i, ballPositionsInGoNextStepList[i].X);
                    writeValue(writer, "ballPositionsInGoY" + i, ballPositionsInGoNextStepList[i].Y);
                    writeValue(writer, "ShooterPositionsInGoX" + i, penaltyShooterStatesInGoNextStepList[i].Location.X);
                    writeValue(writer, "ShooterPositionsInGoY" + i, penaltyShooterStatesInGoNextStepList[i].Location.Y);
                    writeValue(writer, "ShooterAngleInGo" + i, penaltyShooterStatesInGoNextStepList[i].Angle);
                    writeValue(writer, "GoaliePositionsInGoX" + i, GoaliePositionsInGoNextStepList[i].X);
                    writeValue(writer, "GoaliePositionsInGoY" + i, GoaliePositionsInGoNextStepList[i].Y);

                }
                for (int i = 0 ; i < ballPositionsInWaitingNextStepList.Count ; i++)
                {
                    writeValue(writer, "ballPositionsInWaitingX" + i, ballPositionsInWaitingNextStepList[i].X);
                    writeValue(writer, "ballPositionsInWaitingY" + i, ballPositionsInWaitingNextStepList[i].Y);
                    writeValue(writer, "ShooterPositionsInWaitingX" + i, penaltyShooterStatesInWaitingNextStepList[i].Location.X);
                    writeValue(writer, "ShooterPositionsInWaitingY" + i, penaltyShooterStatesInWaitingNextStepList[i].Location.Y);
                    writeValue(writer, "ShooterAngleInWaiting" + i, penaltyShooterStatesInWaitingNextStepList[i].Angle.Value);
                    writeValue(writer, "GoaliePositionsInWaitingX" + i, GoaliePositionsInWaitingNextStepList[i].X);
                    writeValue(writer, "GoaliePositionsInWaitingY" + i, GoaliePositionsInWaitingNextStepList[i].Y);
                }
                writeValue(writer, "OppScore", model.OpponentScore);
                writer.WriteFullEndElement();
            }
        }

        /// <summary>
        /// Save Results of Penalties in XML Files
        /// </summary>
        /// <param name="pOL">Penalty Output Learning</param>
        /// <param name="penaltyNum">Number of Previews Penalties</param>
        private void SaveResults(List<PenaltyOutputLearning> pOL, int penaltyNum)
        {
            Random randomNumber = new Random();
            string fileName = "PenaltyLearns.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("PenaltyLearningData");
                writeValue(writer, "PenaltyNum", pOL.Count);
                for (int i = 0 ; i < pOL.Count ; i++)
                {
                    writeValue(writer, "dependOrNotWaiting" + i, (int)pOL[i].dependOrNotWaiting);
                    writeValue(writer, "nearOrFarWaiting" + i, (int)pOL[i].nearOrFarWaiting);
                    writeValue(writer, "whereLookWaiting" + i, (pOL[i].whereLookWaiting.HasValue) ? (int)pOL[i].whereLookWaiting.Value : (int)WhereLook.Lookatgoalie);
                    writeValue(writer, "centerOrCornerWaiting" + i, (int)pOL[i].centerOrCornerWaiting);
                    writeValue(writer, "leftOrRightWaiting" + i, (int)pOL[i].leftOrRightGoBall);
                    writeValue(writer, "immediateOrGo" + i, (int)pOL[i].immediateOrGo);
                    writeValue(writer, "shootOrTurnGO" + i, (int)pOL[i].shootOrTurnGO);
                    writeValue(writer, "nearOrFarGO" + i, (int)pOL[i].nearOrFarGO);
                    writeValue(writer, "patternTypeGO" + i, (int)pOL[i].patternTypeGO);
                    writeValue(writer, "penaltyLogic" + i, (int)pOL[i].penaltytype);
                    writeValue(writer, "minimum" + i, (int)pOL[i].min);
                    writeValue(writer, "maximum" + i, (int)pOL[i].max);
                    writeValue(writer, "time" + i, (int)pOL[i].time);

                }
                writer.WriteFullEndElement();
            }
        }

        /// <summary>
        /// Intermediate Function for save in XML Files
        /// </summary>
        /// <param name="xml">Xml Text Writer</param>
        /// <param name="tagName">Tag Name</param>
        /// <param name="value">Value</param>
        private static void writeValue(XmlTextWriter xml, string tagName, object value)
        {
            xml.WriteStartElement(tagName);
            xml.WriteValue(value.ToString());
            xml.WriteEndElement();
        }

        /// <summary>
        /// Load Data from XML Files
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="ballPositionsInGo">ball Positions in GO State(Normal After Penalty Waiting)</param>
        /// <param name="ballPositionsInWaiting">ball Positions in Waiting State</param>
        /// <param name="penaltyShooterStatesInGo"></param>
        /// <param name="penaltyShooterStatesInWaiting"></param>
        /// <param name="GoaliePositionsInWaiting"></param>
        /// <param name="GoaliePositionsInGo"></param>
        private void GetData(string fileName, ref List<Position2D> ballPositionsInGo, ref List<Position2D> ballPositionsInWaiting, ref List<SingleObjectState> penaltyShooterStatesInGo, ref List<SingleObjectState> penaltyShooterStatesInWaiting, ref List<Position2D> GoaliePositionsInWaiting, ref List<Position2D> GoaliePositionsInGo)
        {
            List<double> ballPositionsInGoX = new List<double>();
            List<double> ballPositionsInGoY = new List<double>();
            List<double> ballPositionsInWaitingX = new List<double>();
            List<double> ballPositionsInWaitingY = new List<double>();
            List<double> ShooterPositionsInWaitingX = new List<double>();
            List<double> ShooterPositionsInWaitingY = new List<double>();
            List<float?> ShooterAngleInWaiting = new List<float?>();
            List<double> ShooterPositionsInGoX = new List<double>();
            List<double> ShooterPositionsInGoY = new List<double>();
            List<float?> ShooterAngleInGo = new List<float?>();
            List<double> GoaliePositionsInGoX = new List<double>();
            List<double> GoaliePositionsInGoY = new List<double>();
            List<double> GoaliePositionsInWaitingX = new List<double>();
            List<double> GoaliePositionsInWaitingY = new List<double>();

            fileName = "PenaltyData.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            if (!fileName.Contains(".xml"))
                fileName += ".xml";
            if (!File.Exists(fileName))
                return;

            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                reader.ReadStartElement();
                int countOfDataGo = int.Parse(readValue(reader, "CountOfDataGo").ToString());
                int countOfDataWait = int.Parse(readValue(reader, "CountOfDataWait").ToString());
                for (int i = 0 ; i < countOfDataGo ; i++)
                {
                    ballPositionsInGoX.Add(double.Parse(readValue(reader, "ballPositionsInGoX" + i).ToString()));
                    ballPositionsInGoY.Add(double.Parse(readValue(reader, "ballPositionsInGoY" + i).ToString()));
                    ShooterPositionsInGoX.Add(double.Parse(readValue(reader, "ShooterPositionsInGoX" + i).ToString()));
                    ShooterPositionsInGoY.Add(double.Parse(readValue(reader, "ShooterPositionsInGoY" + i).ToString()));
                    ShooterAngleInGo.Add(float.Parse(readValue(reader, "ShooterAngleInGo" + i).ToString()));
                    GoaliePositionsInGoX.Add(double.Parse(readValue(reader, "GoaliePositionsInGoX" + i).ToString()));
                    GoaliePositionsInGoY.Add(double.Parse(readValue(reader, "GoaliePositionsInGoY" + i).ToString()));
                }
                for (int i = 0 ; i < countOfDataWait ; i++)
                {
                    ballPositionsInWaitingX.Add(double.Parse(readValue(reader, "ballPositionsInWaitingX" + i).ToString()));
                    ballPositionsInWaitingY.Add(double.Parse(readValue(reader, "ballPositionsInWaitingY" + i).ToString()));
                    ShooterPositionsInWaitingX.Add(double.Parse(readValue(reader, "ShooterPositionsInWaitingX" + i).ToString()));
                    ShooterPositionsInWaitingY.Add(double.Parse(readValue(reader, "ShooterPositionsInWaitingY" + i).ToString()));
                    ShooterAngleInWaiting.Add(float.Parse(readValue(reader, "ShooterAngleInWaiting" + i).ToString()));
                    GoaliePositionsInWaitingX.Add(double.Parse(readValue(reader, "GoaliePositionsInWaitingX" + i).ToString()));
                    GoaliePositionsInWaitingY.Add(double.Parse(readValue(reader, "GoaliePositionsInWaitingY" + i).ToString()));
                }
                lastOppScore = int.Parse(readValue(reader, "OppScore").ToString());
                reader.ReadEndElement();
            }

            for (int i = 0 ; i < ballPositionsInGoX.Count ; i++)
            {
                ballPositionsInGo.Add(new Position2D(ballPositionsInGoX[i], ballPositionsInGoY[i]));
                penaltyShooterStatesInGo.Add(new SingleObjectState(new Position2D(ShooterPositionsInGoX[i], ShooterPositionsInGoY[i]), Vector2D.Zero, ShooterAngleInGo[i]));
                GoaliePositionsInGo.Add(new Position2D(GoaliePositionsInGoX[i], GoaliePositionsInGoY[i]));
            }
            for (int i = 0 ; i < ballPositionsInWaitingX.Count ; i++)
            {
                ballPositionsInWaiting.Add(new Position2D(ballPositionsInWaitingX[i], ballPositionsInWaitingY[i]));
                penaltyShooterStatesInWaiting.Add(new SingleObjectState(new Position2D(ShooterPositionsInWaitingX[i], ShooterPositionsInWaitingY[i]), Vector2D.Zero, ShooterAngleInWaiting[i]));
                GoaliePositionsInWaiting.Add(new Position2D(GoaliePositionsInWaitingX[i], GoaliePositionsInWaitingY[i]));
            }
        }

        private List<PenaltyOutputLearning> LoadResults(string fileName = "PenaltyLearns.xml")
        {
            if (!fileName.Contains(".xml"))
                fileName += ".xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            List<PenaltyOutputLearning> polList = new List<PenaltyOutputLearning>();
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                PenaltyOutputLearning POL = new PenaltyOutputLearning();
                reader.ReadStartElement();
                int Penaltynum = int.Parse(readValue(reader, "PenaltyNum").ToString());
                for (int i = 0 ; i < Penaltynum ; i++)
                {
                    int dependOrNotDependWaitingPrevPenalty = int.Parse((readValue(reader, "dependOrNotWaiting" + i).ToString()));
                    int nearOrFarWaitingPrevPenalty = int.Parse((readValue(reader, "nearOrFarWaiting" + i).ToString()));
                    int whereLookWaitingPrevPenalty = int.Parse((readValue(reader, "whereLookWaiting" + i).ToString()));
                    int centerOrCornerWaitingPrevPenalty = int.Parse((readValue(reader, "centerOrCornerWaiting" + i).ToString()));
                    int leftOrRightWaitingPrevPenalty = int.Parse((readValue(reader, "leftOrRightWaiting" + i).ToString()));
                    int immediateOrWaitedGoPrevPenalty = int.Parse((readValue(reader, "immediateOrGo" + i).ToString()));
                    int ShootOrTurnGoPrevPenalty = int.Parse((readValue(reader, "shootOrTurnGO" + i).ToString()));
                    int nearOrFarGoPrevPenalty = int.Parse((readValue(reader, "nearOrFarGO" + i).ToString()));
                    int patternTypeGoPrevPenalty = int.Parse((readValue(reader, "patternTypeGO" + i).ToString()));
                    int penaltyLogic = int.Parse((readValue(reader, "penaltyLogic" + i).ToString()));
                    int minPrevPenalty = int.Parse((readValue(reader, "minimum" + i).ToString()));
                    int maxPrevPenalty = int.Parse((readValue(reader, "maximum" + i).ToString()));
                    int time = int.Parse((readValue(reader, "time" + i).ToString()));

                    POL.dependOrNotWaiting = (DependORNot)dependOrNotDependWaitingPrevPenalty;
                    POL.nearOrFarWaiting = (NearOrFar)nearOrFarWaitingPrevPenalty;
                    POL.whereLookWaiting = (WhereLook)whereLookWaitingPrevPenalty;
                    POL.centerOrCornerWaiting = (CenterOrCorner)centerOrCornerWaitingPrevPenalty;
                    POL.leftOrRightGoBall = (LeftOrRight)leftOrRightWaitingPrevPenalty;
                    POL.immediateOrGo = (immediateORWaited)immediateOrWaitedGoPrevPenalty;
                    POL.shootOrTurnGO = (ShootOrTurn)ShootOrTurnGoPrevPenalty;
                    POL.nearOrFarGO = (NearOrFar)nearOrFarGoPrevPenalty;
                    POL.patternTypeGO = (PatternType)patternTypeGoPrevPenalty;
                    POL.penaltytype = (penaltyType)penaltyLogic;
                    POL.min = minPrevPenalty;
                    POL.max = maxPrevPenalty;
                    POL.time = time;
                    polList.Add(POL);
                }
                penaltyCount = Penaltynum;
                reader.ReadEndElement();
            }
            return polList;
        }

        private static object readValue(XmlTextReader xml, string tagName)
        {
            xml.ReadStartElement(tagName);
            var ret = xml.ReadContentAsObject();
            xml.ReadEndElement();
            return ret;
        }

        public void Run(WorldModel model)
        {
            if (firstTime)
            {
                lastBallstate = model.BallState.Location;
                firstTime = false;
            }
            ShooterID = CalculatePenaltyShooterRobotFunc(model);
            DetermineNextStateFunc(model, ShooterID);
            DefineKickFrameFunc(model, ShooterID.Value);
            if (!firstPenaltyArrived)
            {
                DecisionMakingOutPut.centerOrCornerWaiting = CenterOrCorner.Corner;
                DecisionMakingOutPut.dependOrNotWaiting = DependORNot.Independent;
                DecisionMakingOutPut.immediateOrGo = immediateORWaited.Waited;
                DecisionMakingOutPut.max = 200;
                DecisionMakingOutPut.min = 100;
                DecisionMakingOutPut.leftOrRightGoBall = LeftOrRight.Left;
                DecisionMakingOutPut.nearOrFarGO = NearOrFar.Near;
                DecisionMakingOutPut.nearOrFarWaiting = NearOrFar.Far;
                DecisionMakingOutPut.patternTypeGO = PatternType.Random;
                DecisionMakingOutPut.shootOrTurnGO = ShootOrTurn.TurnAndShoot;
                DecisionMakingOutPut.whereLookWaiting = WhereLook.LookOtherSide;
                DecisionMakingOutPut.time = 50;
            }
            if (firstPenaltyArrived && !kicked && OnceExecute)
            {
                OnceExecute = false;
                inLearnMotion = true;
                inExecutionMotion = true;
                inFrequencyMotion = true;
                LearnUnit(model, ShooterID.Value);
            }
            if (model.OpponentScore == lastOppScore)
            {
                Succes = true;
            }
            else if (model.OpponentScore != lastOppScore)
            {
                Succes = false;
            }
            if (currentState == (int)CurrentStateMode.Waiting)
            {
                Confidence = 0;
                OnceExecute2 = true;
                ballPositionsInWaitingNextStepList.Add(model.BallState.Location);
                penaltyShooterStatesInWaitingNextStepList.Add(model.Opponents[ShooterID.Value]);
                GoaliePositionsInWaitingNextStepList.Add(model.OurRobots[model.GoalieID.Value].Location);
            }
            if (currentState == (int)CurrentStateMode.GO)
            {
                ballPositionsInGoNextStepList.Add(model.BallState.Location);
                penaltyShooterStatesInGoNextStepList.Add(model.Opponents[ShooterID.Value]);
                GoaliePositionsInGoNextStepList.Add(model.OurRobots[model.GoalieID.Value].Location);
            }
            if (kicked)
            {
                Confidence++;
            }
            if (OnceExecute2 && Confidence > 6)
            {
                OnceExecute2 = false;
                firstPenaltyArrived = true;
                Confidence = 0;
                SaveData(model);

            }
        }

        private PenaltyOutputLearning DecisionMakingUnit(List<PenaltyOutputLearning> penaltyOutputLearning)
        {
            double averageTime = 0;
            double averageMax = 0;
            double averageMin = 0;
            PenaltyOutputLearning POL = new PenaltyOutputLearning();

            POL.penaltytype = penaltyOutputLearning.LastOrDefault().penaltytype;
            POL.centerOrCornerWaiting = penaltyOutputLearning.LastOrDefault().centerOrCornerWaiting;
            POL.dependOrNotWaiting = penaltyOutputLearning.LastOrDefault().dependOrNotWaiting;
            POL.immediateOrGo = penaltyOutputLearning.LastOrDefault().immediateOrGo;
            POL.leftOrRightGoBall = penaltyOutputLearning.Last().leftOrRightGoBall;
            POL.leftOrRightGoRobot = penaltyOutputLearning.Last().leftOrRightGoRobot;
            POL.nearOrFarGO = penaltyOutputLearning.LastOrDefault().nearOrFarGO;
            POL.nearOrFarWaiting = penaltyOutputLearning.LastOrDefault().nearOrFarWaiting;
            POL.patternTypeGO = penaltyOutputLearning.LastOrDefault().patternTypeGO;
            POL.shootOrTurnGO = penaltyOutputLearning.Last().shootOrTurnGO;
            POL.whereLookWaiting = penaltyOutputLearning.Last().whereLookWaiting;
            POL.success = Succes;
            int timecount = 0;
            for (int i = 1 ; i < penaltyOutputLearning.Count ; i++)
            {
                timecount += i;
                averageTime += (penaltyOutputLearning[i].time);
                averageMax += penaltyOutputLearning[i].max;
                averageMin += penaltyOutputLearning[i].min;
            }


            POL.max = (int)(averageMax / (penaltyOutputLearning.Count - 1));
            POL.min = (int)(averageMin / (penaltyOutputLearning.Count - 1));
            POL.time = (int)(averageTime / (penaltyOutputLearning.Count - 1));
            if (penaltyCount == 1 || POL.time < 0)
            {
                POL.time = 0;
            }
            if (penaltyCount == 2)
            {
                if ((POL.leftOrRightGoBall == LeftOrRight.Left && POL.leftOrRightGoRobot == LeftOrRight.Left) || (POL.leftOrRightGoBall == LeftOrRight.Right && POL.leftOrRightGoRobot == LeftOrRight.Right))
                {
                    POL.retryFrequencyMotion = true;
                    lastRetryFrequencyMotion = true;
                }
                else
                {
                    POL.retryFrequencyMotion = false;
                    lastRetryFrequencyMotion = false;
                }
            }
            if (penaltyCount > 2 && lastRetryFrequencyMotion)
            {
                if ((POL.leftOrRightGoBall == LeftOrRight.Left && POL.leftOrRightGoRobot == LeftOrRight.Left) || (POL.leftOrRightGoBall == LeftOrRight.Right && POL.leftOrRightGoRobot == LeftOrRight.Right))
                {
                    POL.retryFrequencyMotion = true;
                    lastRetryFrequencyMotion = true;
                }
                else
                {
                    POL.retryFrequencyMotion = false;
                    lastRetryFrequencyMotion = false;
                }
            }
            return POL;
        }

        private void LearnUnit(WorldModel model, int ShooterID)
        {
            ///
            GetData("PenaltyData", ref ballPositionsInGoList, ref ballPositionsInWaitingList, ref penaltyShooterStatesInGoList, ref penaltyShooterStatesInWaitingList, ref GoaliePositionsInWaitingList, ref GoaliePositionsInGoList);

            int? GoalieID = model.GoalieID;

            ///Initial Assign
            PatternType patternType = PatternType.Random;
            NearOrFar nearOrFar = NearOrFar.Near;
            ShootOrTurn shootOrTurn = ShootOrTurn.ShootAtLook;
            WhereLook? whereLook = null;
            LeftOrRight leftOrRight = LeftOrRight.Left;
            LeftOrRight leftOrRightRobot = LeftOrRight.Left;
            CenterOrCorner centerOrcorner = CenterOrCorner.Corner;


            firsttime = true;

            ///Define Near Or Far in Waiting State of Penalty
            NearOrFar nearORFar = NearOrFarFunc(model, ShooterID, penaltyShooterStatesInWaitingList, ballPositionsInWaitingList);

            ///Define Close or open Angle with penalty Goalkeeper in penalty
            CloseOROpen closeOrOpen = CloseOrOpenFunc(model, ref centerOrcorner, ref leftOrRightRobot, ref leftOrRight, GoalieID.Value, ShooterID, penaltyShooterStatesInWaitingList, GoaliePositionsInWaitingList, ballPositionsInGoList);
            leftOrRightList.Add(leftOrRight);

            ///Define Is Shooter motion depend On Our Goalie   
            DependORNot dependOrIndepend = DependOrNotFunc(closeOrOpenList, ref whereLook);

            ///Define Left Or Right Direction Logic
            penaltyType leftOrRightLogic = DefineLeftRightLogic(leftOrRightList);

            ///define Pattern of Time Changing
            int min = 0, max = 0;
            int time = 0;
            
            patternType = DefinePatternFunc(timesList, ref min, ref max, ref time);
            kickFrame = time;
            ///Analyze results and take decision for immediate
            immediateORWaited immediateOrWaited = immediateOrWaitedFunc();
            
            shootOrTurn = ShootORTurn(penaltyShooterStatesInGoList);
            nearOrFar = NearOrFarFunc(model, ShooterID, penaltyShooterStatesInGoList, ballPositionsInGoList);

           

            plusTime = time;
            patternTypeGlobal = patternType;

            ///Temporal Preserve of results
            penaltyOutputLearning.nearOrFarWaiting = nearORFar;
            penaltyOutputLearning.leftOrRightGoBall = leftOrRight;
            penaltyOutputLearning.centerOrCornerWaiting = centerOrcorner;
            penaltyOutputLearning.dependOrNotWaiting = dependOrIndepend;
            penaltyOutputLearning.whereLookWaiting = whereLook;
            penaltyOutputLearning.immediateOrGo = immediateOrWaited;
            penaltyOutputLearning.nearOrFarGO = nearOrFar;
            penaltyOutputLearning.patternTypeGO = patternType;
            penaltyOutputLearning.shootOrTurnGO = shootOrTurn;
            penaltyOutputLearning.min = min;
            penaltyOutputLearning.max = max;
            penaltyOutputLearning.time = time;
            penaltyOutputLearning.penaltytype = leftOrRightLogic;
            PenaltyOutPutLearningList.Add(penaltyOutputLearning);
            ///Save Results of Penalties
            SaveResults(PenaltyOutPutLearningList, penaltyCount);
            kickFrame = 0;
            ///Load Results for make decision with all Penalty data
            List<PenaltyOutputLearning> penaltyList = LoadResults();

            ///Make Decision Unit for next penalty 
            ///DecisionMakingOutPut is Global PenaltyOutputLearning struct 
            DecisionMakingOutPut = DecisionMakingUnit(penaltyList);
        }

        public penaltyType DefineLeftRightLogic(List<LeftOrRight> LftRghtPnltyLst)
        {
            penaltyType penaltyLeftOrRight = penaltyType.unPattern;
            if (penaltyCount > 0)
            {
                if (penaltyCount == 1)
                {
                    if (LftRghtPnltyLst.FirstOrDefault() == LeftOrRight.Left)
                    {
                        penaltyLeftOrRight = penaltyType.bigAxis;
                    }
                    if (LftRghtPnltyLst.FirstOrDefault() == LeftOrRight.Right)
                    {
                        penaltyLeftOrRight = penaltyType.smallAxis;
                    }
                    return penaltyLeftOrRight;
                }
                if (penaltyCount == 2)
                {
                    if (LftRghtPnltyLst.FirstOrDefault() != LftRghtPnltyLst.LastOrDefault())
                    {
                        if (LftRghtPnltyLst.FirstOrDefault() == LeftOrRight.Left)
                        {
                            penaltyLeftOrRight = penaltyType.LRLRLR;
                        }
                        if (LftRghtPnltyLst.FirstOrDefault() == LeftOrRight.Right)
                        {
                            penaltyLeftOrRight = penaltyType.RLRLRL;
                        }
                    }
                    else
                    {
                        if (LftRghtPnltyLst.FirstOrDefault() == LeftOrRight.Left)
                        {
                            penaltyLeftOrRight = penaltyType.bigAxis;
                        }
                        if (LftRghtPnltyLst.FirstOrDefault() == LeftOrRight.Right)
                        {
                            penaltyLeftOrRight = penaltyType.smallAxis;
                        }
                    }
                    return penaltyLeftOrRight;
                }
                if (penaltyCount >= 3)
                {
                    if (!Succes)
                    {
                        return penaltyType.unPattern;
                    }
                    else
                    {
                        return penaltyLogic;
                    }
                }
            }
            return penaltyType.unPattern;
        }

        /// <summary>
        /// Define Type Of Time Pattern and if times dont have any pattern 
        /// return Random and Define min and Max of Random 
        /// </summary>
        /// <param name="times">History Of Times </param>
        /// <param name="min">minimum of Random Input Method</param>
        /// <param name="max">maximum of Random Input Method</param>
        /// <returns>Type of Relation Between Times</returns>
        private PatternType DefinePatternFunc(List<int> times, ref int min, ref int max, ref int differential)
        {
            int counterStaticDifferentialTime = 0;
            int counterFixIntegralSize = 0;
            List<int> Diffs = new List<int>();
            if (times.Count > 1)
            {
                for (int i = 1 ; i < times.Count ; i++)
                {
                    Diffs.Add(Math.Abs(times[i] - times[i - 1]));
                }
                double prevSumValue = 0;
                double SumValueValue = 0;
                double nextSumValue = 0;
                int tempDefferential = 0;
                double lastSumValueGreaterThanOne = 0;

                for (int i = 1 ; i < Diffs.Count - 1 ; i++)
                {
                    if (Math.Abs(Diffs[i] - Diffs[i - 1]) < staticDifferentialTime)
                    {
                        counterStaticDifferentialTime++;// Define + OR -
                        tempDefferential = Diffs[i] - Diffs[i - 1];//if (-) --> + $ else --> - 
                    }
                    if (i > 1)
                    {
                        double lastSumValue2 = Math.Abs(Diffs[i] - Diffs[i - 1]);
                        SumValueValue = Math.Abs(lastSumValue2 - prevSumValue);
                        lastSumValueGreaterThanOne = lastSumValue2;

                        double lastSumValue = Math.Abs(Diffs[i + 1] - Diffs[i]);
                        nextSumValue = Math.Abs(lastSumValue - lastSumValueGreaterThanOne);

                        prevSumValue = Diffs[i - 1] - Diffs[i - 2];
                        if (nextSumValue > SumValueValue)
                        {
                            if (nextSumValue % SumValueValue < diffSize)
                            {
                                counterFixIntegralSize++;
                            }
                        }
                        if (nextSumValue < SumValueValue)
                        {
                            if (nextSumValue % SumValueValue < diffSize)
                            {
                                counterFixIntegralSize++;
                            }
                        }
                    }
                }


                if (counterFixIntegralSize == Diffs.Count)
                {
                    return PatternType.FixedSumDiff;
                }
                if (counterStaticDifferentialTime == Diffs.Count)
                {
                    differential = tempDefferential;
                    return PatternType.StaticDiff;
                }
            }
            if (times.Count != 0)
            {
                double Maximum = double.MinValue;
                double minimum = double.MaxValue;
                foreach (var item in times)
                {
                    if (item < minimum)
                    {
                        minimum = item;
                    }
                    if (item > Maximum)
                    {
                        Maximum = item;
                    }
                }
                min = (int)minimum;
                max = (int)Maximum;
                if (min == max)
                {
                    min = min - 20;
                    max = max + 20;
                }
                differential = min;//timesList[times.Count - 1];
            }
            return PatternType.Random;
        }

        /// <summary>
        /// Define When Ball kicked
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="penaltyShooterID">Penalty Shooter ID</param>
        private void DefineKickFrameFunc(WorldModel model, int penaltyShooterID)
        {
            if (currentState == (int)CurrentStateMode.Waiting)
            {
                kicked = false;
                firsttime = true;
                kickFrame = 0;
                penaltyShooterStatesInGoList = new List<SingleObjectState>();
            }
            if (currentState == (int)CurrentStateMode.GO)
            {
                if ((model.BallState.Location.DistanceFrom(lastBallstate) < .11 || model.BallState.Speed.Size < .05) && kicked == false)
                {
                    penaltyShooterStatesInGoList.Add(model.Opponents[penaltyShooterID]);
                    kickFrame++;
                    DrawingObjects.AddObject(new StringDraw(kickFrame.ToString(), new Position2D(1, 1)), "353132131654");
                }
                else
                {
                    kicked = true;
                    if (firsttime)
                    {
                        timesList.Add(kickFrame);
                        penaltyCount++;
                        firsttime = false;
                    }
                }
            }
        }

        /// <summary>
        /// Analyze the First Input Parameter dependent Or Independent
        /// Second Layer of this method if is dependent calculate look
        /// at goalie or look at other side  
        /// </summary>
        /// <param name="dependOrIndependList">CloseOrOpen's Output Collection</param>
        /// <param name="whereLook">Where Look</param>
        /// <returns></returns>
        private DependORNot DependOrNotFunc(List<CloseOROpen> dependOrIndependList, ref WhereLook? whereLook)
        {
            whereLook = null;
            int CloseCounter = 0;
            int OpenCounter = 0;
            int NothCounter = 0;
            foreach (var item in dependOrIndependList)
            {
                switch (item)
                {
                    case CloseOROpen.Close:
                        CloseCounter++;
                        break;
                    case CloseOROpen.Open:
                        OpenCounter++;
                        break;
                    case CloseOROpen.Noth:
                        NothCounter++;
                        break;
                    default:
                        break;
                }
            }
            if (CloseCounter > (int)(dependOrIndependList.Count * .7) || OpenCounter > (int)(dependOrIndependList.Count * .7))
            {
                if (CloseCounter > (int)(dependOrIndependList.Count * .7))
                {
                    whereLook = WhereLook.Lookatgoalie;
                }
                else
                {
                    whereLook = WhereLook.LookOtherSide;
                }
                return DependORNot.Dependent;
            }
            else
            {
                return DependORNot.Independent;
            }
        }

        /// <summary>
        /// this method in first layer calculated dependent or independent
        /// if penalty shooter look depend on goalie pos return true else
        /// return false ,method have 2 ref parameter in layer 2 method 
        /// calculate center or corner and if corner is selected define left or right
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="centerOrCorner">Center Or Corner in Independent State</param>
        /// <param name="leftOrRightBall">Left or Right in Corner State</param>
        /// <param name="goalieID">Goalie ID</param>
        /// <param name="shooterID">Penalty Shooter ID</param>
        /// <returns></returns>
        private CloseOROpen CloseOrOpenFunc(WorldModel model, ref CenterOrCorner centerOrCorner, ref LeftOrRight leftOrRightRobot, ref LeftOrRight leftOrRightBall, int goalieID, int shooterID, List<SingleObjectState> shooterStates, List<Position2D> golieStates, List<Position2D> ballStates)
        {
            int counterCenter = 0;
            int counterCorner = 0;
            int counterClose = 0;
            int counterOpen = 0;
            int counterNoth = 0;
            int counterRightRobot = 0;
            int counterLeftRobot = 0;

            for (int i = 0 ; i < shooterStates.Count ; i++)
            {
                Vector2D penaltyShooterHeadVector = Vector2D.FromAngleSize((shooterStates[i].Angle.Value * Math.PI) / 180, 1);
                Vector2D penaltyShooterGoalie = Vector2D.FromAngleSize((golieStates[i] - shooterStates[i].Location).AngleInRadians, 1);
                Vector2D penaltyShooterOurgoalLeft = GameParameters.OurGoalLeft - shooterStates[i].Location;
                Vector2D penaltyShooterOurgoalRight = GameParameters.OurGoalRight - shooterStates[i].Location;
                Vector2D penaltyShooterOurgoalCenter = GameParameters.OurGoalCenter - shooterStates[i].Location;

                double angleBetweenRobotHeadVectorAndOurGoalRight = Math.Abs(Vector2D.AngleBetweenInDegrees(penaltyShooterOurgoalRight, penaltyShooterHeadVector));
                double angleBetweenRobotHeadVectorAndOurGoalLeft = Math.Abs(Vector2D.AngleBetweenInDegrees(penaltyShooterOurgoalLeft, penaltyShooterHeadVector));
                double angleBetweenRobotHeadVectorAndOurGoalCenter = Math.Abs(Vector2D.AngleBetweenInDegrees(penaltyShooterOurgoalCenter, penaltyShooterHeadVector));

                if (angleBetweenRobotHeadVectorAndOurGoalCenter < centerDiff)
                {
                    counterCenter++;
                }
                else if (angleBetweenRobotHeadVectorAndOurGoalRight < cornerDiff)
                {
                    counterCorner++;
                    counterRightRobot++;
                }
                else if (angleBetweenRobotHeadVectorAndOurGoalLeft < cornerDiff)
                {
                    counterCorner++;
                    counterLeftRobot++;
                }
                double angleBetween = Math.Abs(Vector2D.AngleBetweenInDegrees(penaltyShooterGoalie, penaltyShooterHeadVector));
                if (angleBetween < closeThreshold)
                {
                    counterClose++;
                    closeOrOpenList.Add(CloseOROpen.Close);
                }
                else if (angleBetween > openThreshold)
                {
                    counterOpen++;
                    closeOrOpenList.Add(CloseOROpen.Open);
                }
                else
                {
                    counterNoth++;
                    closeOrOpenList.Add(CloseOROpen.Noth);
                }
            }
            CloseOROpen closeOrOpenOutput = CloseOROpen.Noth;
            if (counterOpen > shooterStates.Count * openCloseCounterThreshold)
            {
                closeOrOpenOutput = CloseOROpen.Open;
            }
            if (counterClose > shooterStates.Count * openCloseCounterThreshold)
            {
                closeOrOpenOutput = CloseOROpen.Close;
            }
            if (counterNoth > shooterStates.Count * openCloseCounterThreshold - 0.2)
            {
                closeOrOpenOutput = CloseOROpen.Noth;
            }
            if (closeOrOpenOutput == CloseOROpen.Noth)
            {
                if (counterCenter > shooterStates.Count * centerOrCornerCounterThreshold)
                {
                    centerOrCorner = CenterOrCorner.Center;
                }
                if (counterCorner > shooterStates.Count * centerOrCornerCounterThreshold)
                {
                    centerOrCorner = CenterOrCorner.Corner;
                }
                if (centerOrCorner == CenterOrCorner.Corner)
                {
                    if (counterLeftRobot > shooterStates.Count * leftOrRightCounterThreshold)
                    {
                        leftOrRightRobot = LeftOrRight.Left;
                    }
                    if (counterRightRobot > shooterStates.Count * leftOrRightCounterThreshold)
                    {
                        leftOrRightRobot = LeftOrRight.Right;
                    }
                }
            }
            int rightAngles = 0;
            int leftAngles = 0;

            for (int i = 0 ; i < ballStates.Count ; i++)
            {
                Vector2D ballLastBall = ballStates[i] - ballStates[0];
                Vector2D oppGoalRightLastBall = GameParameters.OppGoalRight - ballStates[0];
                Vector2D oppGoalLeftLastBall = GameParameters.OppGoalLeft - ballStates[0];
                double angleRightBetween = Vector2D.AngleBetweenInDegrees(ballLastBall, oppGoalRightLastBall);
                double angleLeftBetween = Vector2D.AngleBetweenInDegrees(ballLastBall, oppGoalLeftLastBall);
                if (angleLeftBetween > angleRightBetween)
                {
                    leftAngles++;
                }
                else
                {
                    rightAngles++;
                }
            }
            if (leftAngles > rightAngles)
            {
                leftOrRightBall = LeftOrRight.Right;
                //leftOrRightBall = LeftOrRight.Left;
            }
            else
            {
                //leftOrRightBall = LeftOrRight.Right;
                leftOrRightBall = LeftOrRight.Left;
            }
            return closeOrOpenOutput;
        }

        /// <summary>
        /// Check Near or Far State of Penalty Shooter
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="PenaltyShooterID">Penalty Shooter ID</param>
        /// <returns></returns>
        private NearOrFar NearOrFarFunc(WorldModel model, int PenaltyShooterID, List<SingleObjectState> shooterStates, List<Position2D> ballPositions)
        {
            int CounterNear = 0, CounterFar = 0;
            for (int i = 0 ; i < shooterStates.Count ; i++)
            {
                if (shooterStates[i].Location.DistanceFrom(ballPositions[i]) < nearOrFardistance)
                {
                    CounterNear++;

                }
                else
                {
                    CounterFar++;
                }
            }
            if (CounterNear  > CounterFar)
            {
                return NearOrFar.Near;
            }
            else if (CounterFar > CounterNear)
            {
                return NearOrFar.Far;
            }
            else
            {
                return NearOrFar.Near;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">World Model</param>
        /// <returns>Return Penalty Shooeter ID</returns>
        private int? CalculatePenaltyShooterRobotFunc(WorldModel model)
        {
            int? penatyShooter = null;
            Position2D BallLocation = model.BallState.Location;
            double min = double.MaxValue;

            if (currentState == (int)CurrentStateMode.GO || currentState == (int)CurrentStateMode.Waiting || currentState ==  (int)CurrentStateMode.Noth)
            {
                foreach (var item in model.Opponents.Keys)
                {

                    if (model.Opponents[item].Location.DistanceFrom(BallLocation) < min)
                    {
                        min = model.Opponents[item].Location.DistanceFrom(BallLocation);
                        penatyShooter = item;
                    }
                }
            }
            return penatyShooter;
        }

        /// <summary>
        /// Determine the next State of Penalty 
        /// </summary>
        /// <param name="model">World Model</param>
        public void DetermineNextStateFunc(WorldModel model, int? ShooterID)
        {
            //Game Status Calculation
            if (model.Status == GameStatus.Penalty_Opponent_Waiting)
            {
                currentState = (int)CurrentStateMode.Waiting;
            }
            if (ShooterID.HasValue)
            {
                if (model.Status == GameStatus.Penalty_Opponent_Go && model.Opponents[ShooterID.Value].Location.DistanceFrom(model.BallState.Location) < BallThreshold)
                {
                    currentState = (int)CurrentStateMode.GO;
                }
            }
            else
            {
                currentState = (int)CurrentStateMode.Noth;
            }
        }

        /// <summary>
        /// Waiting Or Go
        /// </summary>
        public enum CurrentStateMode
        {
            Waiting,
            GO,
            Noth
        }

        /// <summary>
        /// Near Or Far For Ball Back State in Waiting Or Go
        /// </summary>
        public enum NearOrFar
        {
            Near,
            Far
        }

        /// <summary>
        /// Depend to Goalie Or Independ
        /// </summary>
        public enum DependORNot
        {
            Dependent,
            Independent
        }

        /// <summary>
        /// Waited or immediate when Kick
        /// </summary>
        public enum immediateORWaited
        {
            immediate,
            Waited
        }

        /// <summary>
        /// In immediate mode 
        /// </summary>
        public enum ShootOrTurn
        {
            ShootAtLook,
            TurnAndShoot
        }

        /// <summary>
        /// in Waiting State and Dependent Mode
        /// </summary>
        public enum WhereLook
        {
            Lookatgoalie,
            LookOtherSide
        }

        /// <summary>
        /// Look At Center or Corner of Goal
        /// </summary>
        public enum CenterOrCorner
        {
            Center,
            Corner
        }

        public enum LeftOrRight
        {
            Left,
            Right
        }

        /// <summary>
        /// Type of Time Changing
        /// </summary>
        public enum Time
        {
            Random,
            Conditional,
            Pattern
        }

        public enum PatternType
        {
            StaticDiff,
            FixedSumDiff,
            Random
        }

        public enum CloseOROpen
        {
            Close,
            Open,
            Noth
        }

        public struct PenaltyOutputLearning
        {
            public NearOrFar nearOrFarWaiting;
            public DependORNot dependOrNotWaiting;
            public WhereLook? whereLookWaiting;
            public CenterOrCorner centerOrCornerWaiting;
            public LeftOrRight leftOrRightGoBall;
            public LeftOrRight leftOrRightGoRobot;
            public immediateORWaited immediateOrGo;
            public ShootOrTurn shootOrTurnGO;
            public NearOrFar nearOrFarGO;
            public PatternType patternTypeGO;
            public penaltyType penaltytype;
            public int min;
            public int max;
            public int time;
            public bool retryFrequencyMotion;
            public bool success;
        }

        public enum penaltyType
        {
            bigAxis,
            smallAxis,
            LRLRLR,
            RLRLRL,
            LRRLRRL,
            unPattern
        }
    }

}
