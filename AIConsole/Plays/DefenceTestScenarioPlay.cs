using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Plays
{
    class DefenceTestScenarioPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            return Status == GameStatus.TestOffend;
            return false;
        }
        int mainCounter = 0;

        int Robot1ID = 0;
        int Robot2ID = 1;
        int Robot3ID = 2;
        int Robot4ID = 3;
        int Robot5ID = 4;
        int Robot6ID = 5;

        List<bool> weHaveRoboti = new List<bool>() { false, false, false, false, false, false };

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            mainCounter++;
            int robotNumbers = 6;
            for (int i = 0; i < robotNumbers; i++)
            {
                weHaveRoboti[i] = true;
            }
            //Robot Positions

            List<Position2D> robot1Positions = new List<Position2D>() { new Position2D(0, 0), new Position2D(0, 1) };

            List<Position2D> robot2Positions = new List<Position2D>() { new Position2D(1, 0), new Position2D(1, 1), new Position2D(3, 3) };

            List<Position2D> robot3Positions = new List<Position2D>() { new Position2D(2, 0), new Position2D(2, 1) };

            List<Position2D> robot4Positions = new List<Position2D>() { new Position2D(3, 0), new Position2D(3, 1),  new Position2D(-1, -2)};

            List<Position2D> robot5Positions = new List<Position2D>() { new Position2D(-1, 0), new Position2D(-1, 1) };

            List<Position2D> robot6Positions = new List<Position2D>() { new Position2D(-2, 0), new Position2D(-2, 1) };


            //Robot Delay Before Each State ( Count of Frames)

            //First Item is for go to initial state

            List<int> robot1Time = new List<int>() { 100, 200 };

            List<int> robot2Time = new List<int>() { 100, 200, 400 };

            List<int> robot3Time = new List<int>() { 100, 200 };

            List<int> robot4Time = new List<int>() { 100, 200  , 400};

            List<int> robot5Time = new List<int>() { 100, 200 };

            List<int> robot6Time = new List<int>() { 100, 200 };


            //Robot Target Angles (Degree)

            List<double> robot1Angle = new List<double>() { 20, 90 };

            List<double> robot2Angle = new List<double>() { 20, 90  , -90};

            List<double> robot3Angle = new List<double>() { 20, 90 };

            List<double> robot4Angle = new List<double>() { 20, 90 ,-90};

            List<double> robot5Angle = new List<double>() { 20, 90 };

            List<double> robot6Angle = new List<double>() { 20, 90 };

            //Robot 1 Calculations
            #region robot 1
            if (weHaveRoboti[0])
            {
                Position2D target = new Position2D();
                double angle = 0;

                List<int> robot1IntegeratedTimes = new List<int>();
                for (int i = 0; i < robot1Time.Count; i++)
                {
                    int time = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        if (i == 0)
                        {
                            time = robot1Time[i];
                        }
                        else
                        {
                            time += robot1Time[j];
                        }
                    }
                    robot1IntegeratedTimes.Add(time);
                }
                for (int i = 0; i < robot1IntegeratedTimes.Count - 1; i++)
                {
                    if (robot1IntegeratedTimes.Count > 1)
                    {
                        if (mainCounter > 0 && mainCounter <= robot1IntegeratedTimes[0])
                        {
                            target = robot1Positions[0];
                            angle = robot1Angle[0];
                        }
                        else if (mainCounter > robot1IntegeratedTimes[i] && mainCounter <= robot1IntegeratedTimes[i + 1])
                        {
                            target = robot1Positions[i + 1];
                            angle = robot1Angle[i + 1];
                        }

                    }
                    else
                    {
                        target = robot1Positions[0];
                        angle = robot1Angle[0];
                    }

                }
                Planner.Add(Robot1ID, target, angle);
            }
            #endregion
            //Robot 2 Calculations
            #region robot2
            if (weHaveRoboti[1])
            {
                Position2D target = new Position2D();
                double angle = 0;

                List<int> robot2IntegeratedTimes = new List<int>();
                for (int i = 0; i < robot2Time.Count; i++)
                {
                    int time = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        if (i == 0)
                        {
                            time = robot2Time[i];
                        }
                        else
                        {
                            time += robot2Time[j];
                        }
                    }
                    robot2IntegeratedTimes.Add(time);
                }
                for (int i = 0; i < robot2IntegeratedTimes.Count - 1; i++)
                {
                    if (robot2IntegeratedTimes.Count > 1)
                    {
                        if (mainCounter > 0 && mainCounter <= robot2IntegeratedTimes[0])
                        {
                            target = robot2Positions[0];
                            angle = robot2Angle[0];
                        }
                        else if (mainCounter > robot2IntegeratedTimes[i] && mainCounter <= robot2IntegeratedTimes[i + 1])
                        {
                            target = robot2Positions[i + 1];
                            angle = robot2Angle[i + 1];
                        }

                    }
                    else
                    {
                        target = robot2Positions[0];
                        angle = robot2Angle[0];
                    }

                }
                Planner.Add(Robot2ID, target, angle);
            }
            #endregion 
            //Robot 3 Calculations
            #region Robot 3

            if (weHaveRoboti[2])
            {
                Position2D target = new Position2D();
                double angle = 0;

                List<int> robot3IntegeratedTimes = new List<int>();
                for (int i = 0; i < robot3Time.Count; i++)
                {
                    int time = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        if (i == 0)
                        {
                            time = robot3Time[i];
                        }
                        else
                        {
                            time += robot3Time[j];
                        }
                    }
                    robot3IntegeratedTimes.Add(time);
                }
                for (int i = 0; i < robot3IntegeratedTimes.Count - 1; i++)
                {
                    if (robot3IntegeratedTimes.Count > 1)
                    {
                        if (mainCounter > 0 && mainCounter <= robot3IntegeratedTimes[0])
                        {
                            target = robot3Positions[0];
                            angle = robot3Angle[0];
                        }
                        else if (mainCounter > robot3IntegeratedTimes[i] && mainCounter <= robot3IntegeratedTimes[i + 1])
                        {
                            target = robot3Positions[i + 1];
                            angle = robot3Angle[i + 1];
                        }

                    }
                    else
                    {
                        target = robot3Positions[0];
                        angle = robot3Angle[0];
                    }

                }

                Planner.Add(Robot3ID, target, angle);
            }
            #endregion
            //Robot 4 Calculations
            #region Robot4
            if (weHaveRoboti[3])
            {
                Position2D target = new Position2D();
                double angle = 0;

                List<int> robot4IntegeratedTimes = new List<int>();
                for (int i = 0; i < robot4Time.Count; i++)
                {
                    int time = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        if (i == 0)
                        {
                            time = robot4Time[i];
                        }
                        else
                        {
                            time += robot4Time[j];
                        }
                    }
                    robot4IntegeratedTimes.Add(time);
                }
                for (int i = 0; i < robot4IntegeratedTimes.Count - 1; i++)
                {
                    if (robot4IntegeratedTimes.Count > 1)
                    {
                        if (mainCounter > 0 && mainCounter <= robot4IntegeratedTimes[0])
                        {
                            target = robot4Positions[0];
                            angle = robot4Angle[0];
                        }
                        else if (mainCounter > robot4IntegeratedTimes[i] && mainCounter <= robot4IntegeratedTimes[i + 1])
                        {
                            target = robot4Positions[i + 1];
                            angle = robot4Angle[i + 1];
                        }

                    }
                    else
                    {
                        target = robot4Positions[0];
                        angle = robot4Angle[0];
                    }

                }
                Planner.Add(Robot4ID, target, angle);
            }
            #endregion
            //Robot 5 Calculations
            #region Robot 5
            if (weHaveRoboti[4])
            {
                Position2D target = new Position2D();
                double angle = 0;

                List<int> robot5IntegeratedTimes = new List<int>();
                for (int i = 0; i < robot5Time.Count; i++)
                {
                    int time = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        if (i == 0)
                        {
                            time = robot5Time[i];
                        }
                        else
                        {
                            time += robot5Time[j];
                        }
                    }
                    robot5IntegeratedTimes.Add(time);
                }
                for (int i = 0; i < robot5IntegeratedTimes.Count - 1; i++)
                {
                    if (robot5IntegeratedTimes.Count > 1)
                    {
                        if (mainCounter > 0 && mainCounter <= robot5IntegeratedTimes[0])
                        {
                            target = robot5Positions[0];
                            angle = robot5Angle[0];
                        }
                        else if (mainCounter > robot5IntegeratedTimes[i] && mainCounter <= robot5IntegeratedTimes[i + 1])
                        {
                            target = robot5Positions[i + 1];
                            angle = robot5Angle[i + 1];
                        }

                    }
                    else
                    {
                        target = robot5Positions[0];
                        angle = robot5Angle[0];
                    }

                }
                Planner.Add(Robot5ID, target, angle);
            }
            #endregion
            //Robot 6 Calculations
            #region Robot6
            if (weHaveRoboti[5])
            {
                Position2D target = new Position2D();
                double angle = 0;

                List<int> robot6IntegeratedTimes = new List<int>();
                for (int i = 0; i < robot6Time.Count; i++)
                {
                    int time = 0;
                    for (int j = i; j >= 0; j--)
                    {
                        if (i == 0)
                        {
                            time = robot6Time[i];
                        }
                        else
                        {
                            time += robot6Time[j];
                        }
                    }
                    robot6IntegeratedTimes.Add(time);
                }
                for (int i = 0; i < robot6IntegeratedTimes.Count - 1; i++)
                {
                    if (robot6IntegeratedTimes.Count > 1)
                    {
                        if (mainCounter > 0 && mainCounter <= robot6IntegeratedTimes[0])
                        {
                            target = robot6Positions[0];
                            angle = robot6Angle[0];
                        }
                        else if (mainCounter > robot6IntegeratedTimes[i] && mainCounter <= robot6IntegeratedTimes[i + 1])
                        {
                            target = robot6Positions[i + 1];
                            angle = robot6Angle[i + 1];
                        }

                    }
                    else
                    {
                        target = robot6Positions[0];
                        angle = robot6Angle[0];
                    }

                }
                Planner.Add(Robot6ID, target, angle);
            }
            #endregion
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(GameDefinitions.WorldModel Model, GameStrategyEngine engine)
        {

        }
    }
}
