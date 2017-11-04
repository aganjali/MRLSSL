using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.IO;
using System.Xml;
using System.Drawing;

namespace MRL.SSL.AIConsole.Engine
{
    public static class DefenceTest
    {
        //corner1 zamani be list position add shavad ke filesave mishavad
        //stopcover dar hame ja add shavad 
        //staticrole ha ezafe shavad
        private static object ReadValue(XmlTextReader xml, string tagName)
        {
            xml.ReadStartElement(tagName);
            var ret = xml.ReadContentAsObject();
            xml.ReadEndElement();
            return ret;
        }
        private static void WriteValue(XmlTextWriter xml, string tagName, object value)
        {
            xml.WriteStartElement(tagName);
            xml.WriteValue(value.ToString());
            xml.WriteEndElement();
        }
        private static void SaveData()
        {
            Random randomNumber = new Random();
            string fileName = "DefenceTestResults.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("DefenceTestData");
                WriteValue(writer, "CountOfData", countOfData);
                for (int i = 0; i < countOfData; i++)
                {
                    WriteValue(writer, "ballX" + i, BallPoses[i].X);
                    WriteValue(writer, "bally" + i, BallPoses[i].Y);
                    WriteValue(writer, "Static1X" + i, StaticDefenderRole1Poses[i].X);
                    WriteValue(writer, "Static1Y" + i, StaticDefenderRole1Poses[i].Y);
                    WriteValue(writer, "Static2X" + i, StaticDefenderRole2Poses[i].X);
                    WriteValue(writer, "Static2Y" + i, StaticDefenderRole2Poses[i].Y);
                    WriteValue(writer, "Corner1X" + i, CornerRole1Poses[i].X);
                    WriteValue(writer, "Corner1Y" + i, CornerRole1Poses[i].Y);
                    WriteValue(writer, "Corner2X" + i, CornerRole2Poses[i].X);
                    WriteValue(writer, "Corner2Y" + i, CornerRole2Poses[i].Y);
                    WriteValue(writer, "Corner3X" + i, CornerRole3Poses[i].X);
                    WriteValue(writer, "Corner3Y" + i, CornerRole3Poses[i].Y);
                    WriteValue(writer, "Corner4X" + i, CornerRole4Poses[i].X);
                    WriteValue(writer, "Corner4Y" + i, CornerRole4Poses[i].Y);
                    WriteValue(writer, "Marker1X" + i, MarkerRole1Poses[i].X);
                    WriteValue(writer, "Marker1Y" + i, MarkerRole1Poses[i].Y);
                    WriteValue(writer, "Marker2X" + i, MarkerRole2Poses[i].X);
                    WriteValue(writer, "Marker2Y" + i, MarkerRole2Poses[i].Y);
                    WriteValue(writer, "Marker3X" + i, MarkerRole3Poses[i].X);
                    WriteValue(writer, "Marker3Y" + i, MarkerRole3Poses[i].Y);
                    WriteValue(writer, "Regional1X" + i, RegionalRole1Poses[i].X);
                    WriteValue(writer, "Regional1Y" + i, RegionalRole1Poses[i].Y);
                    WriteValue(writer, "Regional2X" + i, RegionalRole2Poses[i].X);
                    WriteValue(writer, "Regional2Y" + i, RegionalRole2Poses[i].Y);
                    WriteValue(writer, "GoalieX" + i, goaliePoses[i].X);
                    WriteValue(writer, "GoalieY" + i, goaliePoses[i].Y);
                    WriteValue(writer, "StopX" + i, StopCoverPoses[i].X);
                    WriteValue(writer, "StopY" + i, StopCoverPoses[i].Y);
                    WriteValue(writer, "OpenGoalCorner1" + i, listemptyPoints[i][(int)roles.CornerRole1]);
                    WriteValue(writer, "OpenGoalCorner2" + i, listemptyPoints[i][(int)roles.CornerRole2]);
                    WriteValue(writer, "OpenGoalCorner3" + i, listemptyPoints[i][(int)roles.CornerRole3]);
                    WriteValue(writer, "OpenGoalCorner4" + i, listemptyPoints[i][(int)roles.CornerRole4]);
                    WriteValue(writer, "OpenGoalMarker1" + i, listemptyPoints[i][(int)roles.MarkerRole1]);
                    WriteValue(writer, "OpenGoalMarker2" + i, listemptyPoints[i][(int)roles.MarkerRole2]);
                    WriteValue(writer, "OpenGoalMarker3" + i, listemptyPoints[i][(int)roles.MarkerRole3]);
                    WriteValue(writer, "OpenGoalRegional1" + i, listemptyPoints[i][(int)roles.RegionalRole1]);
                    WriteValue(writer, "OpenGoalRegional2" + i, listemptyPoints[i][(int)roles.RegionalRole2]);
                    WriteValue(writer, "OpenGoalStatic1" + i, listemptyPoints[i][(int)roles.Static1]);
                    WriteValue(writer, "OpenGoalStatic2" + i, listemptyPoints[i][(int)roles.Static2]);
                    WriteValue(writer, "Goalie" + i, listemptyPoints[i][(int)roles.goalie]);
                    WriteValue(writer, "StopCover" + i, listemptyPoints[i][(int)roles.StopCover]);
                    WriteValue(writer, "totalemptyPoints" + i, TotalEmptyPOints[i]);
                    WriteValue(writer, "distancefromgoal" + i, ballDistanceFromGoal[i]);
                    WriteValue(writer, "Nearestrobot" + i, NearestRobotDistance[i]);
                    WriteValue(writer, "GoalieDist" + i, GoalDistFromGoal[i]);
                }
                writer.WriteFullEndElement();
            }
        }
        static bool testWithOnePoint = false;
        static Position2D onePointTarget = new Position2D(2.300316237, -0.811842304);

        static bool loadFromFile = true;
        public static bool BallTest = FreekickDefence.testDefenceState;//Test Defence if => true => ball = FakeBall
        static double timeInSecondForStatic = 5;
        static int indexforstatic = 0;
        static Position2D currentBallPos = new Position2D();
        public static SingleObjectState currentBallState = new SingleObjectState();
        static int timer = 0;
        public static Position2D DefenderCornerRole1 = new Position2D();
        public static Position2D DefenderCornerRole2 = new Position2D();
        public static Position2D DefenderCornerRole3 = new Position2D();
        public static Position2D DefenderCornerRole4 = new Position2D();
        public static Position2D DefenderMarkerRole1 = new Position2D();
        public static Position2D DefenderMarkerRole2 = new Position2D();
        public static Position2D DefenderMarkerRole3 = new Position2D();
        public static Position2D GoalieRole = new Position2D();
        public static Position2D DefenderRegionalRole1 = new Position2D();
        public static Position2D DefenderRegionalRole2 = new Position2D();
        public static Position2D StopCover1 = new Position2D();
        public static Position2D DefenderStaticRole1 = new Position2D();
        public static Position2D DefenderStaticRole2 = new Position2D();

        public static bool WeHaveDefenderRegionalRole1 = false;
        public static bool WeHaveDefenderRegionalRole2 = false;
        public static bool WeHaveDefenderCornerRole1 = false;
        public static bool WeHaveDefenderCornerRole2 = false;
        public static bool WeHaveDefenderCornerRole3 = false;
        public static bool WeHaveDefenderCornerRole4 = false;
        public static bool WeHaveDefenderMarkerRole1 = false;
        public static bool WeHaveDefenderMarkerRole2 = false;
        public static bool WeHaveDefenderMarkerRole3 = false;
        public static bool WeHaveDefenderStaticRole1 = false;
        public static bool WeHaveDefenderStaticRole2 = false;
        public static bool WeHaveStopCover1 = false;
        public static bool WeHaveGoalie = false;

        public static bool weHaveDefenderRegionalRole1 = false;
        public static bool weHaveDefenderRegionalRole2 = false;
        public static bool weHaveDefenderCornerRole1 = false;
        public static bool weHaveDefenderCornerRole2 = false;
        public static bool weHaveDefenderCornerRole3 = false;
        public static bool weHaveDefenderCornerRole4 = false;
        public static bool weHaveDefenderMarkerRole1 = false;
        public static bool weHaveDefenderMarkerRole2 = false;
        public static bool weHaveDefenderMarkerRole3 = false;
        public static bool weHaveDefenderStaticRole1 = false;
        public static bool weHaveDefenderStaticRole2 = false;
        public static bool weHaveStopCover1 = false;
        public static bool weHaveGoalie = false;

        static bool EnableDefenderRegionalRole1 = true;
        static bool EnableDefenderRegionalRole2 = true;
        static bool EnableDefenderCornerRole1 = true;
        static bool EnableDefenderCornerRole2 = true;
        static bool EnableDefenderCornerRole3 = true;
        static bool EnableDefenderCornerRole4 = true;
        static bool EnableDefenderMarkerRole1 = true;
        static bool EnableDefenderMarkerRole2 = true;
        static bool EnableDefenderMarkerRole3 = true;
        static bool EnableDefenderStaticRole1 = true;
        static bool EnableDefenderStaticRole2 = true;
        static bool EnableStopCover1 = true;
        static bool EnableGoalie = true;

        public static List<Position2D> BallPoses = new List<Position2D>();
        public static List<Position2D> CornerRole1Poses = new List<Position2D>();
        public static List<Position2D> CornerRole2Poses = new List<Position2D>();
        public static List<Position2D> CornerRole3Poses = new List<Position2D>();
        public static List<Position2D> CornerRole4Poses = new List<Position2D>();
        public static List<Position2D> MarkerRole1Poses = new List<Position2D>();
        public static List<Position2D> MarkerRole2Poses = new List<Position2D>();
        public static List<Position2D> MarkerRole3Poses = new List<Position2D>();
        public static List<Position2D> RegionalRole1Poses = new List<Position2D>();
        public static List<Position2D> RegionalRole2Poses = new List<Position2D>();
        public static List<Position2D> StaticDefenderRole1Poses = new List<Position2D>();
        public static List<Position2D> StaticDefenderRole2Poses = new List<Position2D>();
        public static List<Position2D> goaliePoses = new List<Position2D>();
        public static List<Position2D> StopCoverPoses = new List<Position2D>();
        private static bool filesaved = false;

        static Dictionary<int, string> emptyPoints = new Dictionary<int, string>();
        static List<Dictionary<int, string>> listemptyPoints = new List<Dictionary<int, string>>();
        static List<string> TotalEmptyPOints = new List<string>();
        static List<double> ballDistanceFromGoal = new List<double>();
        static List<double> NearestRobotDistance = new List<double>();
        static List<double> GoalDistFromGoal = new List<double>();
        static List<double> posesx = new List<double>();
        static List<double> posesy = new List<double>();
        public static List<Position2D> defenceTestPoints = new List<Position2D>();

        static List<Position2D> inputPositions = new List<Position2D>();

        static int countOfData = 0;
        static void ParseXMLFile()
        {
            string fileName = "DefenceTest.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            if (!fileName.Contains(".xml"))
                fileName += ".xml";
            if (!File.Exists(fileName))
                return;
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                reader.ReadStartElement();
                countOfData = int.Parse(ReadValue(reader, "CountOfDataGo").ToString());
                for (int i = 0; i < countOfData; i++)
                {
                    posesx.Add(double.Parse(ReadValue(reader, "PosX" + i).ToString()));
                    posesy.Add(double.Parse(ReadValue(reader, "PosY" + i).ToString()));
                }
                reader.ReadEndElement();
            }
            for (int i = 0; i < countOfData; i++)
            {
                inputPositions.Add(new Position2D(posesx[i], posesy[i]));
            }
        }
        static int counter1 = 0;
        public static void GenerateBallPos()
        {
            if (BallTest)
            {
                Reset();
                if (counter1 == 0)
                {
                    ParseXMLFile();
                    currentBallPos = inputPositions[0];
                }
                counter1++;
            }

        }

        public static void MakeOutPut()
        {

            if (BallTest)
            {
                if (!testWithOnePoint)
                {
                    timer++;
                    if (loadFromFile)
                    {
                        if (indexforstatic < countOfData)
                        {
                            if (timer * 0.016 > timeInSecondForStatic)
                            {

                                FillFunction();
                                CalculateFunction();

                                timer = 1;
                                currentBallPos = inputPositions[indexforstatic];
                                indexforstatic++;

                            }
                        }
                        else if (!filesaved)
                        {
                            DistanceNearRobot();
                            CombineForTotalCalculate();
                            SaveData();
                            filesaved = true;
                        }
                    }
                    else
                    {


                    }
                }
                else
                {
                    currentBallPos = onePointTarget;
                }
            }
            DrawingObjects.AddObject(new Circle(currentBallPos, .04, new Pen(Brushes.White, .03f)), "1321656546546454");
            currentBallState = new SingleObjectState(currentBallPos, Vector2D.Zero, 0);
        }

        private static int[] StringToIntArray(string source)
        {
            int[] convertedarray = new int[100];
            for (int i = 0; i < source.ToList().Count; i++)
            {
                int.TryParse(source[i].ToString(), out convertedarray[i]);
            }
            return convertedarray;
        }
        //strategish moonde
        private static void CombineForTotalCalculate()
        {
            bool strategy = false;
            int[] corner1 = new int[100];
            int[] corner2 = new int[100];
            int[] corner3 = new int[100];
            int[] corner4 = new int[100];
            int[] marker1 = new int[100];
            int[] marker2 = new int[100];
            int[] marker3 = new int[100];
            int[] Regional1 = new int[100];
            int[] Regional2 = new int[100];
            int[] stopcover = new int[100];

            int[] static1 = new int[100];
            int[] static2 = new int[100];
            int[] goalie = new int[100];
            string total = "";
            if (strategy)
            {
                for (int i = 0; i < countOfData; i++)
                {
                    corner1 = StringToIntArray(listemptyPoints[i][(int)roles.CornerRole1]);
                    corner2 = StringToIntArray(listemptyPoints[i][(int)roles.CornerRole2]);
                    corner3 = StringToIntArray(listemptyPoints[i][(int)roles.CornerRole3]);
                    corner4 = StringToIntArray(listemptyPoints[i][(int)roles.CornerRole4]);
                    marker1 = StringToIntArray(listemptyPoints[i][(int)roles.MarkerRole1]);
                    marker2 = StringToIntArray(listemptyPoints[i][(int)roles.MarkerRole2]);
                    marker3 = StringToIntArray(listemptyPoints[i][(int)roles.MarkerRole3]);
                    Regional1 = StringToIntArray(listemptyPoints[i][(int)roles.RegionalRole1]);
                    Regional2 = StringToIntArray(listemptyPoints[i][(int)roles.RegionalRole2]);
                    stopcover = StringToIntArray(listemptyPoints[i][(int)roles.StopCover]);
                    goalie = StringToIntArray(listemptyPoints[i][(int)roles.goalie]);
                    total = "";
                    for (int j = 0; j < 100; j++)
                    {
                        bool corner1true = (corner1[j] == 1) ? true : false;
                        bool corner2true = (corner2[j] == 1) ? true : false;
                        bool corner3true = (corner3[j] == 1) ? true : false;
                        bool corner4true = (corner4[j] == 1) ? true : false;
                        bool marker1true = (marker1[j] == 1) ? true : false;
                        bool marker2true = (marker2[j] == 1) ? true : false;
                        bool marker3true = (marker3[j] == 1) ? true : false;
                        bool Regional1true = (Regional1[j] == 1) ? true : false;
                        bool Regional2true = (Regional2[j] == 1) ? true : false;
                        bool goalietrue = (goalie[j] == 1) ? true : false;
                        total += (corner1true || corner2true || corner3true || corner4true || marker1true || marker2true || marker3true || Regional1true || Regional2true || goalietrue) ? "1" : "0";
                    }
                    TotalEmptyPOints.Add(total);
                }
            }
            else
            {
                for (int i = 0; i < countOfData; i++)
                {
                    static1 = StringToIntArray(listemptyPoints[i][(int)roles.Static1]);
                    static2 = StringToIntArray(listemptyPoints[i][(int)roles.Static2]);
                    goalie = StringToIntArray(listemptyPoints[i][(int)roles.goalie]);
                    total = "";
                    for (int j = 0; j < 100; j++)
                    {
                        bool static1true = (static1[j] == 1) ? true : false;
                        bool static2true = (static2[j] == 1) ? true : false;
                        bool goalietrue = (goalie[j] == 1) ? true : false;
                        total += (static1true || static2true || goalietrue) ? "1" : "0";
                    }
                    TotalEmptyPOints.Add(total);
                    ballDistanceFromGoal.Add(BallPoses[i].DistanceFrom(GameParameters.OurGoalCenter));
                }
                
            }
        }

        private static void CalculateFunction()
        {
            emptyPoints = new Dictionary<int, string>();
            string emptypointss = "";

            for (int i = 0; i < 100; i++)
            {
                emptypointss += "0";
            }


            if (WeHaveDefenderStaticRole1)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderStaticRole1, currentBallPos, out WehaveTwotangent);
                string staticRole1 = goalfree(roles.Static1, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.Static1, staticRole1);
            }
            else
                emptyPoints.Add((int)roles.Static1, emptypointss);

            if (WeHaveDefenderStaticRole2)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderStaticRole2, currentBallPos, out WehaveTwotangent);
                string staticRole2 = goalfree(roles.Static2, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.Static2, staticRole2);
            }
            else
                emptyPoints.Add((int)roles.Static2, emptypointss);


            if (WeHaveDefenderCornerRole1)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderCornerRole1, currentBallPos, out WehaveTwotangent);
                string cornerRole1 = goalfree(roles.CornerRole1, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.CornerRole1, cornerRole1);
            }
            else
                emptyPoints.Add((int)roles.CornerRole1, emptypointss);

            if (WeHaveDefenderCornerRole2)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderCornerRole2, currentBallPos, out WehaveTwotangent);
                string cornerRole2 = goalfree(roles.CornerRole2, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.CornerRole2, cornerRole2);
            }
            else
                emptyPoints.Add((int)roles.CornerRole2, emptypointss);

            if (WeHaveDefenderCornerRole3)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderCornerRole3, currentBallPos, out WehaveTwotangent);
                string cornerRole3 = goalfree(roles.CornerRole3, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.CornerRole3, cornerRole3);
            }
            else
                emptyPoints.Add((int)roles.CornerRole3, emptypointss);

            if (WeHaveDefenderCornerRole4)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderCornerRole4, currentBallPos, out WehaveTwotangent);
                string cornerRole4 = goalfree(roles.CornerRole4, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.CornerRole4, cornerRole4);
            }
            else
                emptyPoints.Add((int)roles.CornerRole4, emptypointss);

            if (WeHaveDefenderMarkerRole1)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderMarkerRole1, currentBallPos, out WehaveTwotangent);
                string marker1 = goalfree(roles.MarkerRole1, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.MarkerRole1, marker1);
            }
            else
                emptyPoints.Add((int)roles.MarkerRole1, emptypointss);

            if (WeHaveDefenderMarkerRole2)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderMarkerRole2, currentBallPos, out WehaveTwotangent);
                string marker2 = goalfree(roles.MarkerRole2, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.MarkerRole2, marker2);
            }
            else
                emptyPoints.Add((int)roles.MarkerRole2, emptypointss);

            if (WeHaveDefenderMarkerRole3)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderMarkerRole3, currentBallPos, out WehaveTwotangent);
                string marker3 = goalfree(roles.MarkerRole3, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.MarkerRole3, marker3);
            }
            else
                emptyPoints.Add((int)roles.MarkerRole3, emptypointss);

            if (WeHaveDefenderRegionalRole1)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderRegionalRole1, currentBallPos, out WehaveTwotangent);
                string RegionalRole1 = goalfree(roles.RegionalRole1, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.RegionalRole1, RegionalRole1);
            }
            else
                emptyPoints.Add((int)roles.RegionalRole1, emptypointss);

            if (WeHaveDefenderRegionalRole2)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(DefenderRegionalRole2, currentBallPos, out WehaveTwotangent);
                string RegionalRole2 = goalfree(roles.RegionalRole2, tangentsPoint, currentBallPos);
                emptyPoints.Add((int)roles.RegionalRole2, RegionalRole2);
            }
            else
                emptyPoints.Add((int)roles.RegionalRole2, emptypointss);

            if (WeHaveGoalie)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(GoalieRole, currentBallPos, out WehaveTwotangent);
                string goalieRole = goalfree(roles.goalie, tangentsPoint, currentBallPos);
                emptyPoints.Add(10, goalieRole);
            }
            else
                emptyPoints.Add(10, emptypointss);

            if (WeHaveStopCover1)
            {
                bool WehaveTwotangent = false;
                List<Position2D> tangentsPoint = TangentLines(StopCover1, currentBallPos, out WehaveTwotangent);
                string stopCover1 = goalfree(roles.StopCover, tangentsPoint, currentBallPos);
                emptyPoints.Add(11, stopCover1);
            }
            else
                emptyPoints.Add(11, emptypointss);


            listemptyPoints.Add(emptyPoints);

        }

        private static void DistanceNearRobot()
        {
           
            for (int i = 0; i < countOfData; i++)
            {
                List<double> distances = new List<double>();
                if (weHaveDefenderCornerRole1)
                {
                    distances.Add(new Line( BallPoses[i],GameParameters.OurGoalCenter).Distance(CornerRole1Poses[i]));
                }
                if (weHaveDefenderCornerRole2)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(CornerRole2Poses[i]));
                }
                if (weHaveDefenderCornerRole3)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(CornerRole3Poses[i]));
                }
                if (weHaveDefenderCornerRole4)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(CornerRole4Poses[i]));
                }
                if (weHaveDefenderMarkerRole1)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(MarkerRole1Poses[i]));
                }                 
                if (weHaveDefenderMarkerRole2)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(MarkerRole2Poses[i]));
                }                 
                if (weHaveDefenderMarkerRole3)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(MarkerRole3Poses[i]));
                }
                if (weHaveDefenderRegionalRole1)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(RegionalRole1Poses[i]));
                }                 
                if (weHaveDefenderRegionalRole2)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(RegionalRole2Poses[i]));
                }
                if (weHaveDefenderStaticRole1)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(StaticDefenderRole1Poses[i]));
                }
                if (weHaveDefenderStaticRole2)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(StaticDefenderRole2Poses[i]));
                }
                if (weHaveStopCover1)
                {
                    distances.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(StopCoverPoses[i]));
                }
                GoalDistFromGoal.Add(new Line(BallPoses[i], GameParameters.OurGoalCenter).Distance(goaliePoses[i]));
                NearestRobotDistance.Add(distances.Min());
            }
             
        }

        public static void Reset()
        {
            WeHaveDefenderCornerRole1 = false;
            WeHaveDefenderCornerRole2 = false;
            WeHaveDefenderCornerRole3 = false;
            WeHaveDefenderCornerRole4 = false;
            WeHaveDefenderMarkerRole1 = false;
            WeHaveDefenderMarkerRole2 = false;
            WeHaveDefenderMarkerRole3 = false;
            WeHaveDefenderRegionalRole1 = false;
            WeHaveDefenderRegionalRole2 = false;
            WeHaveGoalie = false;
            WeHaveStopCover1 = false;
        }

        public static void FillFunction()
        {
            weHaveDefenderStaticRole1 = WeHaveDefenderStaticRole1 && EnableDefenderStaticRole1;
            weHaveDefenderStaticRole2 = WeHaveDefenderStaticRole2 && EnableDefenderStaticRole2;
            weHaveDefenderCornerRole1 = WeHaveDefenderCornerRole1 && EnableDefenderCornerRole1;
            weHaveDefenderCornerRole2 = WeHaveDefenderCornerRole2 && EnableDefenderCornerRole2;
            weHaveDefenderCornerRole3 = WeHaveDefenderCornerRole3 && EnableDefenderCornerRole3;
            weHaveDefenderCornerRole4 = WeHaveDefenderCornerRole4 && EnableDefenderCornerRole4;
            weHaveDefenderMarkerRole1 = WeHaveDefenderMarkerRole1 && EnableDefenderMarkerRole1;
            weHaveDefenderMarkerRole2 = WeHaveDefenderMarkerRole2 && EnableDefenderMarkerRole2;
            weHaveDefenderMarkerRole3 = WeHaveDefenderMarkerRole3 && EnableDefenderMarkerRole3;
            weHaveDefenderRegionalRole1 = WeHaveDefenderRegionalRole1 && EnableDefenderRegionalRole1;
            weHaveDefenderRegionalRole2 = WeHaveDefenderRegionalRole2 && EnableDefenderRegionalRole2;
            weHaveGoalie = WeHaveGoalie && EnableGoalie;
            weHaveStopCover1 = WeHaveStopCover1 && EnableStopCover1;
            BallPoses.Add(currentBallPos);
            if (weHaveDefenderStaticRole1)
            {
                StaticDefenderRole1Poses.Add(DefenderStaticRole1);

            }
            else
            {
                StaticDefenderRole1Poses.Add(new Position2D());
            }
            if (weHaveDefenderStaticRole2)
            {
                StaticDefenderRole2Poses.Add(DefenderStaticRole2);

            }
            else
            {
                StaticDefenderRole2Poses.Add(new Position2D());
            }
            if (weHaveDefenderCornerRole1)
            {
                CornerRole1Poses.Add(DefenderCornerRole1);

            }
            else
            {
                CornerRole1Poses.Add(new Position2D());
            }
            if (weHaveDefenderCornerRole2)
            {
                CornerRole2Poses.Add(DefenderCornerRole2);
            }
            else
            {
                CornerRole2Poses.Add(new Position2D());
            }
            if (weHaveDefenderCornerRole3)
            {
                CornerRole3Poses.Add(DefenderCornerRole3);
            }
            else
            {
                CornerRole3Poses.Add(new Position2D());
            }
            if (weHaveDefenderCornerRole4)
            {
                CornerRole4Poses.Add(DefenderCornerRole4);
            }
            else
            {
                CornerRole4Poses.Add(new Position2D());
            }
            if (weHaveDefenderMarkerRole1)
            {
                MarkerRole1Poses.Add(DefenderMarkerRole1);
            }
            else
            {
                MarkerRole1Poses.Add(new Position2D());
            }
            if (weHaveDefenderMarkerRole2)
            {
                MarkerRole2Poses.Add(DefenderMarkerRole2);
            }
            else
            {
                MarkerRole2Poses.Add(new Position2D());
            }
            if (weHaveDefenderMarkerRole3)
            {
                MarkerRole3Poses.Add(DefenderMarkerRole3);
            }
            else
            {
                MarkerRole3Poses.Add(new Position2D());
            }
            if (weHaveDefenderRegionalRole1)
            {
                RegionalRole1Poses.Add(DefenderRegionalRole1);
            }
            else
            {
                RegionalRole1Poses.Add(new Position2D());
            }
            if (weHaveDefenderRegionalRole2)
            {
                RegionalRole2Poses.Add(DefenderRegionalRole2);
            }
            else
            {
                RegionalRole2Poses.Add(new Position2D());
            }
            if (weHaveGoalie)
            {
                goaliePoses.Add(GoalieRole);
            }
            else
            {
                goaliePoses.Add(new Position2D());
            }
            if (weHaveStopCover1)
            {
                StopCoverPoses.Add(StopCover1);
            }
            else
            {
                StopCoverPoses.Add(new Position2D());
            }
        }

        /// <summary>
        /// calculate tangent lines of robot circle from ball point
        /// </summary>
        /// <param name="CircleCenter">Ball Position</param>
        /// <param name="BallPosition">Robot Position</param>
        /// <returns>tangent lines</returns>
        static List<Position2D> TangentLines(Position2D CircleCenter, Position2D BallPosition, out bool WeHaveTwoTangent)
        {
            WeHaveTwoTangent = false;

            Position2D firsttangent = CircleCenter + Vector2D.FromAngleSize((BallPosition - CircleCenter).AngleInRadians + Math.PI / 2, RobotParameters.OurRobotParams.Diameter / 2);
            Position2D Secondtangent = CircleCenter + Vector2D.FromAngleSize((BallPosition - CircleCenter).AngleInRadians - Math.PI / 2, RobotParameters.OurRobotParams.Diameter / 2);
            List<Position2D> intersects = new List<Position2D> { firsttangent, Secondtangent };
            //Position2D between = Position2D.Average(CircleCenter, BallPosition);

            //Circle intermediateCircle = new Circle(between, between.DistanceFrom(BallPosition));
            //DrawingObjects.AddObject(intermediateCircle, "35564656465487654");
            //List<Position2D> intersects = intermediateCircle.Intersect(new Circle(CircleCenter, RobotParameters.OurRobotParams.Diameter / 2));
            //if (intersects.Count > 1)
            //{
            //    WeHaveTwoTangent = true;
            //    return intersects;
            //}
            //else if (intersects.Count == 1)
            //{
            //    intersects.Add(Position2D.Zero);
            //}
            //else
            //{
            //    intersects.Add(Position2D.Zero);
            //    intersects.Add(Position2D.Zero);
            //}
            return intersects;
        }

        static string goalfree(roles roleType, List<Position2D> IntersectPoints, Position2D ballstate)
        {
            byte[] goalpoints = new byte[100];

            Line firstLine = new Line(IntersectPoints[0], ballstate);
            Line secondLine = new Line(IntersectPoints[1], ballstate);
            Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
            IntersectPoints[0] = firstLine.IntersectWithLine(goalLine).Value;
            IntersectPoints[1] = secondLine.IntersectWithLine(goalLine).Value;
            Position2D leftPoint = new Position2D();
            Position2D rightPoint = new Position2D();

            if (IntersectPoints[0].Y > IntersectPoints[1].Y)
            {
                leftPoint = IntersectPoints[0];
                rightPoint = IntersectPoints[1];
            }
            else
            {
                leftPoint = IntersectPoints[1];
                rightPoint = IntersectPoints[0];
            }

            leftPoint = new Position2D(GameParameters.OurGoalCenter.X, Math.Min(GameParameters.OurGoalLeft.Y, Math.Max(leftPoint.Y, GameParameters.OurGoalRight.Y)));
            rightPoint = new Position2D(GameParameters.OurGoalCenter.X, Math.Min(GameParameters.OurGoalLeft.Y, Math.Max(rightPoint.Y, GameParameters.OurGoalRight.Y)));
            int leftDistance = (int)(leftPoint.DistanceFrom(GameParameters.OurGoalLeft) * 100);
            int rightDistance = (int)(rightPoint.DistanceFrom(GameParameters.OurGoalLeft) * 100);
            string role = "";
            int s = (int)roleType;
            if (roleType == roles.goalie)
                role = "1";
            else if (roleType == roles.StopCover)
                role = "1";
            else if (roleType == roles.Static1)
                role = "1";
            else if (roleType == roles.Static2)
                role = "1";
            else
                role = "1";// s.ToString();
            string goalstring = "";
            for (int i = 0; i < 100; i++)
            {
                if (i > leftDistance && i < rightDistance)
                    goalstring += role;
                else
                    goalstring += "0";
            }
            return goalstring;
        }

        enum roles
        {
            CornerRole1 = 1,
            CornerRole2 = 2,
            CornerRole3 = 3,
            CornerRole4 = 4,
            MarkerRole1 = 5,
            MarkerRole2 = 6,
            MarkerRole3 = 7,
            RegionalRole1 = 8,
            RegionalRole2 = 9,
            goalie = 10,
            StopCover = 11,
            Static1 = 12,
            Static2 = 13
        }
    }
}
