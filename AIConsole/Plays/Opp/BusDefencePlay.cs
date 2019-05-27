using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.AIConsole.Plays
{
    class BusDefencePlay : PlayBase
    {

        Position2D lastballstate;
        Position2D firstballstate;
        bool flag = false;
        bool ballismoved = false, oppcathball = false;
        public static int RobotCounterInSpecialStrategyofImmortal = 0;
        public static bool DontShitPlease = true;
        private static bool wehaveActive = false;
        public double RegionalRegion = 2.5;
        bool firstSetBallPos = true;
        bool NewTagets = false;
        Position2D firstBallPos = new Position2D();
        public static GoaliPositioningMode CurrentGoalieMode = GoaliPositioningMode.InRightSide;

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            return false;
            double Region = RegionalRegion;

            if (!(engine.EngineID == 0 && (Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent))
                && !(engine.EngineID == 1 && (Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam)))
            {
                flag = false;
                return false;
            }
            else
            {
                if (Model.BallState.Location.X > Region && (
                    Status == GameDefinitions.GameStatus.DirectFreeKick_Opponent || Status == GameStatus.IndirectFreeKick_Opponent || Status == GameDefinitions.GameStatus.DirectFreeKick_OurTeam || Status == GameStatus.IndirectFreeKick_OurTeam) || flag)
                {
                    if (!flag)
                        lastballstate = Model.BallState.Location;
                    flag = true;

                    double d1, d2;
                    int? ourOwner = engine.GameInfo.OurTeam.BallOwner;
                    if (ourOwner.HasValue && !GameParameters.IsInDangerousZone(Model.OurRobots[ourOwner.Value].Location, false, 0.2, out d1, out d2)/* && Model.BallState.Location.X < 1.5*/ && ballismoved)
                    {
                        flag = false;
                        //Status = GameStatus.Normal;
                    }
                    firstBallPos = Model.BallState.Location;
                    return true;
                }
                else
                    return false;
            }
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            
            List<int> oppAttackerIds = new List<int>();
            List<int> attids = new List<int>();
            if (goalieID.HasValue)
            {
                oppAttackerIds = Model.Opponents.Where(w => w.Value.Location.X > -0.5 && w.Key != goalieID).Select(s => s.Key).ToList();
            }
            else
            {
                oppAttackerIds = Model.Opponents.Where(w => w.Value.Location.X > -0.5 && w.Key != goalieID).Select(s => s.Key).ToList();
            }

            oppAttackerIds.AddRange(attids);
            if (!oppcathball && engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < 0.2)
                oppcathball = true;
            if (Model.BallState.Speed.Size < 10 && lastballstate.DistanceFrom(Model.BallState.Location) > 0.07 && oppcathball)
            {
                ballismoved = true;
            }

            int? oppBallOwnerId, goalie = null, busStopCoverID = null, busID1 = null, busID2 = null, busID3 = null, busID4 = null, OppToMarkID1 = null, OppToMarkID2 = null, OppToMarkID3 = null, OppToMarkID4 = null;
            if (!ballismoved)
            {
                oppBallOwnerId = engine.GameInfo.OppTeam.BallOwner;
                if (oppBallOwnerId.HasValue)
                {
                    oppAttackerIds = oppAttackerIds.Where(w => w != oppBallOwnerId.Value).ToList();
                }
            }
            oppAttackerIds = oppAttackerIds.Where(w => Model.Opponents.ContainsKey(w)).OrderByDescending(o => Model.Opponents[o].Location.Y).ToList();
            Dictionary<int, List<BusRegion>> oppAttackers = new Dictionary<int, List<BusRegion>>();
            oppAttackers.Add(0, new List<BusRegion>());
            oppAttackers.Add(1, new List<BusRegion>());
            oppAttackers.Add(2, new List<BusRegion>());
            oppAttackers.Add(3, new List<BusRegion>());
            List<Vector2D> region = new List<Vector2D>();
            //0
            Vector2D vec0 = (GameParameters.OurGoalCenter - Position2D.Zero).GetNormalizeToCopy(5);
            Line l0 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec0, new Pen(Brushes.Pink, 0.02f));
            region.Add(vec0);
            //1
            Vector2D vec1 = (new Position2D(1.5, 3) - GameParameters.OurGoalCenter).GetNormalizeToCopy(5);
            Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec1, new Pen(Brushes.Blue, 0.02f));
            region.Add(vec1);
            //2
            Vector2D vec2 = (Position2D.Zero - GameParameters.OurGoalCenter).GetNormalizeToCopy(5);
            Line l2 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec2, new Pen(Brushes.Red, 0.02f));
            region.Add(vec2);
            //3
            Vector2D vec3 = (new Position2D(1.5, -3) - GameParameters.OurGoalCenter).GetNormalizeToCopy(5);
            Line l3 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec3, new Pen(Brushes.Black, 0.02f));
            region.Add(vec3);
            //4
            Vector2D vec4 = (GameParameters.OurGoalCenter - Position2D.Zero).GetNormalizeToCopy(5);
            Line l4 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec4, new Pen(Brushes.Yellow, 0.02f));
            region.Add(vec4);
            //
            List<int> reg = new List<int>();
            reg.Add(0);
            reg.Add(0);
            reg.Add(0);
            reg.Add(0);
            foreach (var item in oppAttackerIds)
            {
                Circle ballcircle = new Circle(Model.BallState.Location, 0.3);
                if (!ballcircle.IsInCircle(Model.Opponents[item].Location) || ballismoved)
                {
                    Vector2D oppVec = (Model.Opponents[item].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(5);
                    if (oppVec.AngleInDegrees >= vec0.AngleInDegrees && oppVec.AngleInDegrees <= vec1.AngleInDegrees)
                    {
                        reg[0]++;
                        oppAttackers[0].Add(new BusRegion().getBusRegion(1, item, oppVec.AngleInDegrees, Model.Opponents[item]));
                        DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.Blue, 0.05f)));
                    }
                    else if (oppVec.AngleInDegrees > vec1.AngleInDegrees && oppVec.AngleInDegrees <= vec2.AngleInDegrees)
                    {
                        reg[1]++;
                        oppAttackers[1].Add(new BusRegion().getBusRegion(2, item, oppVec.AngleInDegrees, Model.Opponents[item]));
                        DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.Red, 0.05f)));
                    }
                    else if (oppVec.AngleInDegrees < vec2.AngleInDegrees && oppVec.AngleInDegrees <= vec3.AngleInDegrees)
                    {
                        reg[2]++;
                        oppAttackers[2].Add(new BusRegion().getBusRegion(3, item, oppVec.AngleInDegrees, Model.Opponents[item]));
                        DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.Black, 0.05f)));
                    }
                    else if (oppVec.AngleInDegrees > vec3.AngleInDegrees && oppVec.AngleInDegrees <= vec4.AngleInDegrees)
                    {
                        reg[3]++;
                        oppAttackers[3].Add(new BusRegion().getBusRegion(4, item, oppVec.AngleInDegrees, Model.Opponents[item]));
                        DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.09, new Pen(Brushes.White, 0.05f)));
                    }
                }
            }

            Dictionary<int, List<BusRegion>> tempoppAtt = new Dictionary<int, List<BusRegion>>();
            foreach (var oppAtt in oppAttackers)
            {
                if (oppAtt.Value.Count > 2)
                {
                    //oppAttackerIds = engine.GameInfo.OppTeam.Scores.Where(w => !attids.Contains(w.Key)).Where(w => w.Value > 0.6).Select(s => s.Key).ToList();
                    BusRegion b = new BusRegion();
                    List<int> tempList = new List<int>();
                    foreach (var item in oppAttackers[oppAtt.Key])
                    {
                        tempList.Add(item.key);
                    }
                    tempList = engine.GameInfo.OppTeam.Scores.OrderByDescending(w => tempList.Contains(w.Key)).Select(s => s.Key).ToList();
                    tempList = new List<int>() { tempList[0], tempList[1] };
                    List<BusRegion> tempBR = new List<BusRegion>();
                    tempBR = oppAtt.Value.Where(w => tempList.Contains(w.key)).Select(s => s).ToList();
                    tempoppAtt.Add(oppAtt.Key, oppAtt.Value);
                }
                else
                {
                    tempoppAtt.Add(oppAtt.Key,oppAtt.Value);
                }
            }
            oppAttackers = tempoppAtt;

            if (firstSetBallPos)
            {
                firstballstate = Model.BallState.Location;
            }
            //List<int> opp = new List<int>();
            //List<int> real = new List<int>();
            List<int> opp = new List<int>();
            List<int> real = new List<int>();
            for (int i = 0; i < 4; i++)
                real.Add(reg[i]);

            bool flagIndex = false;
            int idsIndex = 0;
            for (int i = 0; i < real.Count; i++)
            {
                if (real[i] > 1)
                {
                    flagIndex = true;
                    idsIndex = i;
                }
            }

            if (flagIndex)
                reg[idsIndex] += 4 - oppAttackerIds.Count;
            else
            {
                for (int i = 0; i < reg.Count; i++)
                {
                    if (reg[i] < 1)
                    {
                        reg[i]++;
                    }
                }
            }
            bool flag1 = true;
            int counter10 = 0;
            while (flag1)
            {
                int val = reg[counter10];
                switch (counter10)
                {
                    case 0:
                        if (val > 3)
                        {
                            reg[1]++;
                            reg[2]++;
                            reg[0] -= 2;
                        }
                        else if (val > 2)
                        {
                            reg[1]++;
                            reg[0]--;
                        }
                        break;
                    case 1:
                        if (val > 3)
                        {
                            reg[0]++;
                            reg[2]++;
                            reg[1] -= 2;
                        }
                        else if (val > 2)
                        {
                            reg[1]--;
                            if (reg[0] <= reg[2])
                                reg[0]++;
                            else
                                reg[2]++;
                        }
                        else if (val > 1 && val > real[1])
                        {
                            reg[1]--;
                            if (reg[0] <= reg[2])
                                reg[0]++;
                            else
                                reg[2]++;
                        }
                        break;
                    case 2:
                        if (val > 3)
                        {
                            reg[1] += 1;
                            reg[3] += 1;
                            reg[2] -= 2;
                        }
                        else if (val > 2)
                        {
                            reg[2]--;
                            if (reg[1] <= reg[3])
                                reg[1]++;
                            else
                                reg[3]++;
                        }
                        else if (val > 1 && val > real[2])
                        {
                            reg[2]--;
                            if (reg[1] <= reg[3])
                                reg[1]++;
                            else
                                reg[3]++;
                        }
                        break;
                    case 3:
                        if (val > 3)
                        {
                            reg[1]++;
                            reg[2]++;
                            reg[3] -= 2;
                        }
                        else if (val > 2)
                        {
                            reg[2]++;
                            reg[3]--;
                        }
                        break;
                }

                flag1 = false;
                for (int i = 0; i < 4; i++)
                {
                    if (reg[i] > 2 || (reg[i] > real[i] && reg[i] > 1))
                    {
                        flag1 = true;
                        break;
                    }
                }

                counter10++;
                if (counter10 > 3)
                    counter10 = 0;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < reg[i]; j++)
                {
                    opp.Add(i);
                }
            }




            #region debug

            #region test
            //#region mamad
            //if (NewTagets && false)
            //{
            //    for (int i = 0; i < 4; i++)
            //        real.Add(reg[i]);

            //    bool flagIndex = false;
            //    int idsIndex = 0;
            //    for (int i = 0; i < real.Count; i++)
            //    {
            //        if (real[i] > 1)
            //        {
            //            flagIndex = true;
            //            idsIndex = i;
            //        }
            //    }

            //    if (flagIndex)
            //        reg[idsIndex] += 4 - oppAttackerIds.Count;
            //    else
            //    {
            //        for (int i = 0; i < reg.Count; i++)
            //        {
            //            if (reg[i] < 1)
            //            {
            //                reg[i]++;
            //            }
            //        }
            //    }
            //    bool flag1 = true;
            //    int counter10 = 0;
            //    while (flag1)
            //    {
            //        int val = reg[counter10];
            //        switch (counter10)
            //        {
            //            case 0:
            //                if (val > 3)
            //                {
            //                    reg[1]++;
            //                    reg[2]++;
            //                    reg[0] -= 2;
            //                }
            //                else if (val > 2)
            //                {
            //                    reg[1]++;
            //                    reg[0]--;
            //                }
            //                break;
            //            case 1:
            //                if (val > 3)
            //                {
            //                    reg[0]++;
            //                    reg[2]++;
            //                    reg[1] -= 2;
            //                }
            //                else if (val > 2)
            //                {
            //                    reg[1]--;
            //                    if (reg[0] <= reg[2])
            //                        reg[0]++;
            //                    else
            //                        reg[2]++;
            //                }
            //                else if (val > 1 && val > real[1])
            //                {
            //                    reg[1]--;
            //                    if (reg[0] <= reg[2])
            //                        reg[0]++;
            //                    else
            //                        reg[2]++;
            //                }
            //                break;
            //            case 2:
            //                if (val > 3)
            //                {
            //                    reg[1] += 1;
            //                    reg[3] += 1;
            //                    reg[2] -= 2;
            //                }
            //                else if (val > 2)
            //                {
            //                    reg[2]--;
            //                    if (reg[1] <= reg[3])
            //                        reg[1]++;
            //                    else
            //                        reg[3]++;
            //                }
            //                else if (val > 1 && val > real[2])
            //                {
            //                    reg[2]--;
            //                    if (reg[1] <= reg[3])
            //                        reg[1]++;
            //                    else
            //                        reg[3]++;
            //                }
            //                break;
            //            case 3:
            //                if (val > 3)
            //                {
            //                    reg[1]++;
            //                    reg[2]++;
            //                    reg[3] -= 2;
            //                }
            //                else if (val > 2)
            //                {
            //                    reg[2]++;
            //                    reg[3]--;
            //                }
            //                break;
            //        }

            //        flag1 = false;
            //        for (int i = 0; i < 4; i++)
            //        {
            //            if (reg[i] > 2 || (reg[i] > real[i] && reg[i] > 1))
            //            {
            //                flag1 = true;
            //                break;
            //            }
            //        }

            //        counter10++;
            //        if (counter10 > 3)
            //            counter10 = 0;
            //    }

            //    for (int i = 0; i < 4; i++)
            //    {
            //        for (int j = 0; j < reg[i]; j++)
            //        {
            //            opp.Add(i);
            //        }
            //    }

            //    for (int i = 0; i < opp.Count; i++)
            //    {
            //        DrawingObjects.AddObject(new StringDraw("opp" + i.ToString() + ": " + opp[i].ToString(), GameParameters.OurLeftCorner.Extend(-.1 * i, .2)));
            //    }
            //    for (int i = 0; i < reg.Count; i++)
            //    {
            //        DrawingObjects.AddObject(new StringDraw("reg" + i.ToString() + ": " + reg[i].ToString(), GameParameters.OurRightCorner.Extend(-.1 * i, 0)));
            //    }
            //}
            //#endregion

            //#region !NewTagets
            //else if (!NewTagets && false)
            //{
            //    reg.Add(0);
            //    reg.Add(0);
            //    reg.Add(0);
            //    reg.Add(0);
            //    for (int i = 0; i < 4; i++)
            //    {
            //        reg[i] = oppAttackers[i].Count;
            //    }

            //    for (int i = 0; i < 4; i++)
            //    {

            //    }

            //    for (int i = 0; i < opp.Count; i++)
            //    {
            //        DrawingObjects.AddObject(new StringDraw("opp" + i.ToString() + ": " + opp[i].ToString(), GameParameters.OurLeftCorner.Extend(-.1 * i, .2)));
            //    }
            //    for (int i = 0; i < reg.Count; i++)
            //    {
            //        DrawingObjects.AddObject(new StringDraw("reg" + i.ToString() + ": " + reg[i].ToString(), GameParameters.OurRightCorner.Extend(-.1 * i, 0)));
            //    }
            //}
            //#endregion

            //List<int> opp = new List<int>();
            //List<int> real = new List<int>();
            //for (int i = 0; i < 4; i++)
            //    real.Add(reg[i]);

            //bool flagIndex = false;
            //int idsIndex = 0;
            //for (int i = 0; i < real.Count; i++)
            //{
            //    if (real[i] > 1)
            //    {
            //        flagIndex = true;
            //        idsIndex = i;
            //    }
            //}

            //if (flagIndex)
            //    reg[idsIndex] += 4 - oppAttackerIds.Count;
            //else
            //{
            //    for (int i = 0; i < reg.Count; i++)
            //    {
            //        if (reg[i] < 1)
            //        {
            //            reg[i]++;
            //        }
            //    }
            //}
            //bool flag1 = true;
            //int counter10 = 0;
            //while (flag1)
            //{
            //    int val = reg[counter10];
            //    switch (counter10)
            //    {
            //        case 0:
            //            if (val > 3)
            //            {
            //                reg[1]++;
            //                reg[2]++;
            //                reg[0] -= 2;
            //            }
            //            else if (val > 2)
            //            {
            //                reg[1]++;
            //                reg[0]--;
            //            }
            //            break;
            //        case 1:
            //            if (val > 3)
            //            {
            //                reg[0]++;
            //                reg[2]++;
            //                reg[1] -= 2;
            //            }
            //            else if (val > 2)
            //            {
            //                reg[1]--;
            //                if (reg[0] <= reg[2])
            //                    reg[0]++;
            //                else
            //                    reg[2]++;
            //            }
            //            else if (val > 1 && val > real[1])
            //            {
            //                reg[1]--;
            //                if (reg[0] <= reg[2])
            //                    reg[0]++;
            //                else
            //                    reg[2]++;
            //            }
            //            break;
            //        case 2:
            //            if (val > 3)
            //            {
            //                reg[1] += 1;
            //                reg[3] += 1;
            //                reg[2] -= 2;
            //            }
            //            else if (val > 2)
            //            {
            //                reg[2]--;
            //                if (reg[1] <= reg[3])
            //                    reg[1]++;
            //                else
            //                    reg[3]++;
            //            }
            //            else if (val > 1 && val > real[2])
            //            {
            //                reg[2]--;
            //                if (reg[1] <= reg[3])
            //                    reg[1]++;
            //                else
            //                    reg[3]++;
            //            }
            //            break;
            //        case 3:
            //            if (val > 3)
            //            {
            //                reg[1]++;
            //                reg[2]++;
            //                reg[3] -= 2;
            //            }
            //            else if (val > 2)
            //            {
            //                reg[2]++;
            //                reg[3]--;
            //            }
            //            break;
            //    }


            //    counter10++;
            //    if (counter10 > 3)
            //    {
            //        counter10 = 0;
            //        flag1 = false;
            //        for (int i = 0; i < 4; i++)
            //        {
            //            if (reg[i] > 2 || (reg[i] > real[i] && reg[i] > 1))
            //            {
            //                flag1 = true;
            //                break;
            //            }
            //        }
            //    }
            //}

            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < reg[i]; j++)
            //    {
            //        opp.Add(i);
            //    }
            //}

            //if (real[0] > 2)
            //{
            //    if (real[1] == 0 && opp[2] == 1)
            //    {
            //        opp[2]--;
            //    }
            //    else if (real[1] == 0 && real[2] == 0 && opp[2] == 1)
            //    {
            //        opp[2]--;
            //    }
            //    if (real[2] == 0 && opp.Count > 3 && opp[3] == 2)
            //    {
            //        opp[3]--;
            //        if (real[0] > 3 && real[1] == 0)
            //        {
            //            opp[3]--;
            //        }
            //    }
            //}
            //else if (real[1] > 2)
            //{
            //    if (real[0] == 0)
            //    {
            //        if (opp[0] < 1)
            //            opp[0]++;
            //    }
            //    if (real[2] == 0)
            //    {
            //        if (opp[2] > 1)
            //            opp[2]--;
            //        if (real[1] > 3 && real[3] == 0)
            //            opp[3]--;
            //        if (real[1] > 2 && real[2] == 0 && real[3] == 0)
            //            opp[3]--;
            //    }
            //}
            //else if (real[2] > 2)
            //{
            //    if (real[1] == 0)
            //    {
            //        if (opp[0] < 2)
            //            opp[0]++;
            //    }
            //    if (real[3] == 0)
            //    {
            //        if (opp[3] > 2 && real[3] == 0)
            //            opp[3]--;
            //        //else if (real[3] == 0)
            //        //    opp[3]--;
            //    }
            //}
            //for (int i = 0; i < opp.Count; i++)
            //{
            //    DrawingObjects.AddObject(new StringDraw("opp" + i.ToString() + ": " + opp[i].ToString(), GameParameters.OurLeftCorner.Extend(-.1 * i, .2)));
            //}
            //for (int i = 0; i < reg.Count; i++)
            //{
            //    DrawingObjects.AddObject(new StringDraw("reg" + i.ToString() + ": " + reg[i].ToString(), GameParameters.OurRightCorner.Extend(-.1 * i, .2)));
            //}

            #endregion

            if (true)
            {
                //for (int i = 0; i < opp.Count; i++)
                //{
                //    DrawingObjects.AddObject(new StringDraw("opp" + i.ToString() + ": " + opp[i].ToString(), GameParameters.OurLeftCorner.Extend(-.1 * i, .2)));
                //}
                //for (int i = 0; i < reg.Count; i++)
                //{
                //    DrawingObjects.AddObject(new StringDraw("reg" + i.ToString() + ": " + reg[i].ToString(), GameParameters.OurRightCorner.Extend(-.1 * i, .2)));
                //}
                DrawingObjects.AddObject(l0);
                //DrawingObjects.AddObject(new StringDraw(vec0.AngleInDegrees.ToString(), l0.Tail));
                DrawingObjects.AddObject(l1);
                //DrawingObjects.AddObject(new StringDraw(vec1.AngleInDegrees.ToString(), l1.Tail));
                DrawingObjects.AddObject(l2);
                //DrawingObjects.AddObject(new StringDraw(vec2.AngleInDegrees.ToString(), l2.Tail));
                DrawingObjects.AddObject(l3);
                //DrawingObjects.AddObject(new StringDraw(vec3.AngleInDegrees.ToString(), l3.Tail));
                DrawingObjects.AddObject(l4);
                //DrawingObjects.AddObject(new StringDraw(vec4.AngleInDegrees.ToString(), l4.Tail));
            }
            #endregion

            #region opps


            #region matcher

            RoleBase r;
            roles = new List<RoleInfo>();

            r = typeof(BusRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(BusRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(BusRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(BusRole4).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));

            r = typeof(BusStopRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 10, 0.04));


            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            goalie = Model.GoalieID;

            if (matched.Any(w => w.Value.GetType() == typeof(BusStopRole1)))
                busStopCoverID = matched.Where(w => w.Value.GetType() == typeof(BusStopRole1)).First().Key;

            if (matched.Any(w => w.Value.GetType() == typeof(BusRole1)))
                busID1 = matched.Where(w => w.Value.GetType() == typeof(BusRole1)).First().Key;

            if (matched.Any(w => w.Value.GetType() == typeof(BusRole2)))
                busID2 = matched.Where(w => w.Value.GetType() == typeof(BusRole2)).First().Key;

            if (matched.Any(w => w.Value.GetType() == typeof(BusRole3)))
                busID3 = matched.Where(w => w.Value.GetType() == typeof(BusRole3)).First().Key;

            if (matched.Any(w => w.Value.GetType() == typeof(BusRole4)))
                busID4 = matched.Where(w => w.Value.GetType() == typeof(BusRole4)).First().Key;



            #endregion

            #region assigner

            int? _null = null;
            List<DefenderCommand> defendcommands = new List<DefenderCommand>();
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(GoalieCornerRole)
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole1),
                OppID = engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ToList().Count > 0 ? engine.GameInfo.OppTeam.Scores.Where(h => h.Key != goalieID).ElementAt(0).Key : _null
            });
            var infos = FreekickDefence.Match(engine, Model, defendcommands, false);
            DefenceInfo gol = infos.Single(s => s.RoleType == typeof(GoalieCornerRole));
            if (gol.TargetState.Type == ObjectType.Opponent)
            {

            }
            //gol.DefenderPosition = GameParameters.OurGoalCenter;
            //gol.OppID = null;
            //gol.RoleType = typeof(GoalieCornerRole);
            //gol.TargetState = Model.BallState;
            //gol.Teta = 180;



            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(BusGoalieCornerRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<BusGoalieCornerRole>(goalie.Value).Run(eng, wmd, goalie.Value, gol.DefenderPosition.Value, gol.Teta, gol, true, ballismoved, CurrentlyAssignedRoles);
                                                                                                 //Run(engine, wmd, golie.Value, gol.DefenderPosition.Value, gol.Teta, gol, normal3.DefenderPosition.Value, n3.Value, true);
            if (!ballismoved)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, busStopCoverID, typeof(BusStopRole1)))
                    Functions[busStopCoverID.Value] = (eng, wmd) => GetRole<BusStopRole1>(busStopCoverID.Value).RotateRun(eng, wmd, busStopCoverID.Value);
            }
            else
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, busStopCoverID, typeof(ActiveRole)))
                    Functions[busStopCoverID.Value] = (eng, wmd) => GetRole<ActiveRole>(busStopCoverID.Value).Perform(eng, wmd, busStopCoverID.Value, null);
            }
            //if (opp.Count > 0)
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, busID1, typeof(BusRole1)))
                Functions[busID1.Value] = (eng, wmd) => GetRole<BusRole1>(busID1.Value).perform(eng, wmd, busID1.Value, (opp.Count > 0 ? opp[0] : 0), oppAttackers, region, OppToMarkID1, ballismoved, firstBallPos, CurrentlyAssignedRoles);


            //if (opp.Count > 1)
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, busID2, typeof(BusRole2)))
                Functions[busID2.Value] = (eng, wmd) => GetRole<BusRole2>(busID2.Value).perform(eng, wmd, busID2.Value, (opp.Count > 1 ? opp[1] : 1), oppAttackers, region, OppToMarkID2, ballismoved, firstBallPos, CurrentlyAssignedRoles);


            //if (opp.Count > 2)
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, busID3, typeof(BusRole3)))
                Functions[busID3.Value] = (eng, wmd) => GetRole<BusRole3>(busID3.Value).perform(eng, wmd, busID3.Value, (opp.Count > 2 ? opp[2] : 2), oppAttackers, region, OppToMarkID3, ballismoved, firstBallPos, CurrentlyAssignedRoles);


            //if (opp.Count > 3)
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, busID4, typeof(BusRole4)))
                Functions[busID4.Value] = (eng, wmd) => GetRole<BusRole4>(busID4.Value).perform(eng, wmd, busID4.Value, (opp.Count > 3 ? opp[3] : 3), oppAttackers, region, OppToMarkID4, ballismoved, firstBallPos, CurrentlyAssignedRoles);


            #endregion

            //}


            #endregion

            //ControlParameters.BallIsMoved = ballismoved;
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;

            //FreekickDefence.MakeOutPut();

            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return null;
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(GameDefinitions.WorldModel Model, GameStrategyEngine engine)
        {
            flag = false;
            ballismoved = false;
            oppcathball = false;
            RobotCounterInSpecialStrategyofImmortal = 0;
            DontShitPlease = true;
            //ballState = new SingleObjectState();
            //ballStateFast = new SingleObjectState();
            wehaveActive = false;
            RegionalRegion = 2;
            firstSetBallPos = true;
        }


        private void AddRoleInfo(List<RoleInfo> roles, Type role, double weight, double margin)
        {
            RoleBase r = role.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, weight, margin));
        }

        int? getID(Dictionary<int, RoleBase> current, Type roletype)
        {
            if (current.Any(a => a.Value.GetType() == roletype))
                return current.Single(a => a.Value.GetType() == roletype).Key;
            return null;
        }
    }
}
