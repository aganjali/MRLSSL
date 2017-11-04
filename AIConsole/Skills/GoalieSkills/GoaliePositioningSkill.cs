using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Skills.GoalieSkills
{
    public class GoaliePositioningSkill:SkillBase
    {
        public GoaliePositioningSkill()
        {
           // Controller = new Controller();
        }
        
        double minGoaliDist = 0.4, maxGoaliDist = 0.75;
        double minGoalidx = 0.13;
        double mindefX = 0.9, maxdefX = 1.6;
        double GoaliDist, GoaliX, DefenderDist;
        bool isOnGoalLine;
        double margin = 0, prepAng = 0;
        Position2D FirstGoalCorner, SecondGoalCorner;
        Line FirstGoalLine, SecondGoalLine;
        Position2D TargetPos = new Position2D();
        public SingleWirelessCommand Positioning(GameStrategyEngine engine, WorldModel Model, SingleObjectState TargetState, int RobotID, GoaliPositioningMode Mode, bool defender, bool isChipKick, double kickPower)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            Line DefenderBoundLine1, DefendTargetGoalLine, SecondGoalParallelLine;
            if (GameParameters.IsInField(TargetState.Location, 0.1))
            {
              // TargetState.Location = new Position2D(Math.Min(TargetState.Location.X, GameParameters.OurGoalCenter.X - 0.12), TargetState.Location.Y);
                CalculateBounds(Model, TargetState, Mode);
                Position2D tmpGoaliPos = CalculateGoaliPos(Model, TargetState, out isOnGoalLine, out DefendTargetGoalLine);
                Position2D? tmpDefenderPos = CalculateDefenderPos(Model, TargetState, Mode, tmpGoaliPos, out DefenderBoundLine1, out SecondGoalParallelLine);
                List<Position2D> Positions = ReCalculatePositions(Model,TargetState, Mode, tmpGoaliPos, tmpDefenderPos, DefenderBoundLine1, isOnGoalLine, DefendTargetGoalLine, SecondGoalParallelLine);
                if (Positions[0].X > GameParameters.OurGoalCenter.X - minGoalidx)
                    Positions[0] = new Position2D(GameParameters.OurGoalCenter.X - minGoalidx, Positions[0].Y);
                Position2D PosToGo = (defender) ? Positions[1] : Positions[0];
                bool avoidzone;
                if (defender)
                    avoidzone = true;
                else
                {
                    avoidzone = false;
                }
                Planner.Add(RobotID, PosToGo, (float)(TargetPos - PosToGo).AngleInDegrees, avoidzone);
                //SWC = Controller.CalculateTargetSpeed(Model, RobotID, PosToGo, (TargetPos - PosToGo).AngleInDegrees, null);
            }
            Planner.AddKick(RobotID, kickPowerType.Power, kickPower, isChipKick);
            //SWC.KickPower = kickPower;
            //SWC.isChipKick = isChipKick;
            //SWC.BackSensor = false;
            return SWC;
            
        }
        private void CalculateBounds(WorldModel Model, SingleObjectState TargetState, GoaliPositioningMode Mode)
        {
            minGoaliDist = 0.4;
            maxGoaliDist = 0.75;
            minGoalidx = 0.11;
            mindefX = 1;
            maxdefX = 1.6;

            if (Mode == GoaliPositioningMode.InRightSide)
            {
                margin = -0.02;
                prepAng = -Math.PI / 2;
                FirstGoalCorner = GameParameters.OurGoalRight;
                SecondGoalCorner = GameParameters.OurGoalLeft;
            }
            else
            {
                margin = 0.02;
                prepAng = Math.PI / 2;
                FirstGoalCorner = GameParameters.OurGoalLeft;
                SecondGoalCorner = GameParameters.OurGoalRight;
            }
            TargetPos = new Position2D(Math.Min(TargetState.Location.X, GameParameters.OurGoalCenter.X - 0.12), TargetState.Location.Y);
            //if (TargetState.Location.X < -GameParameters.OurGoalCenter.X / 3)
            //    maxdefX = 2.5;
            if (TargetPos.DistanceFrom(GameParameters.OurGoalCenter) <= mindefX)
            {
                mindefX = Math.Max(TargetPos.DistanceFrom(GameParameters.OurGoalCenter) + 0.1, mindefX);
                maxdefX = Math.Max(TargetPos.DistanceFrom(GameParameters.OurGoalCenter) + 0.1, mindefX);
            }
            double db = Math.Abs(TargetPos.DistanceFrom(GameParameters.OurGoalCenter));
            double dbx = Math.Abs(TargetPos.X - GameParameters.OurGoalCenter.X);

            db = db / (GameParameters.OurGoalCenter.DistanceFrom(new Position2D(0, 0) + new Vector2D(-1, 0)));
            db = (db > 1) ? 1 : db;
            
            dbx = dbx / (GameParameters.OurGoalCenter.X + 1);
            dbx = (dbx > 1) ? 1 : dbx;

            GoaliDist = minGoaliDist + db * (maxGoaliDist - minGoaliDist);
            GoaliX = minGoalidx + dbx * (maxGoaliDist - minGoalidx);

            double ddb = Math.Abs(TargetPos.DistanceFrom(GameParameters.OurGoalCenter));
            ddb = ddb / (GameParameters.OurGoalCenter.DistanceFrom(new Position2D(0, 0) + new Vector2D(-1.5, 0)));
            ddb = (ddb > 1) ? 1 : ddb;
            DefenderDist = mindefX + ddb * (maxdefX - mindefX);
            FirstGoalLine = new Line(TargetPos, new Position2D(FirstGoalCorner.X, FirstGoalCorner.Y + margin));
            SecondGoalLine = new Line(TargetPos, new Position2D(SecondGoalCorner.X, SecondGoalCorner.Y - margin));
        }
        private Position2D  CalculateGoaliPos(WorldModel Model,SingleObjectState TargetState, out bool isOnGoalLine, out Line DefendTargetGoaliLine)
        {
            Position2D GoaliPos;
            Circle goaliBands = new Circle(GameParameters.OurGoalCenter, GoaliDist);
            Vector2D FGoalVec = FirstGoalLine.Head - FirstGoalLine.Tail;
            isOnGoalLine = false;
            Vector2D tmpVec = Vector2D.FromAngleSize(FGoalVec.AngleInRadians + prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            DefendTargetGoaliLine = new Line(FirstGoalLine.Head + tmpVec, (FirstGoalLine.Head + tmpVec) - FGoalVec);
            List<Position2D> Intersects;
            Intersects = goaliBands.Intersect(DefendTargetGoaliLine);
            if (Intersects.Count == 0)
            {
                Line l = new Line(FirstGoalCorner, goaliBands.Center);
                List<Position2D> tmpInts = goaliBands.Intersect(l);
                GoaliPos = (Math.Sign(tmpInts[0].Y) == Math.Sign(FirstGoalCorner.Y)) ? tmpInts[0] : tmpInts[1];
            }
            else if (Intersects.Count == 1)
                GoaliPos = Intersects[0];
            else
                GoaliPos = (Intersects[0].X < Intersects[1].X) ? Intersects[0] : Intersects[1];
            if (GameParameters.OurGoalCenter.X - GoaliPos.X > GoaliX)
            {
                Line tmpLine = new Line(new Position2D(GameParameters.OurGoalCenter.X - GoaliX, -1), new Position2D(GameParameters.OurGoalCenter.X - GoaliX, 1));
                Position2D? tmpp = tmpLine.IntersectWithLine(DefendTargetGoaliLine);
                GoaliPos = tmpp.Value;
                isOnGoalLine = true;
            }

            return GoaliPos;
        }
        private Position2D? CalculateDefenderPos(WorldModel Model,SingleObjectState TargetState, GoaliPositioningMode Mode, Position2D GoaliPos, out Line DefenderBoundLine1, out Line SecondGoalParallelLine)
        {
            Circle cGoali = new Circle(GoaliPos, RobotParameters.OurRobotParams.Diameter / 2);
           
            Vector2D FirstGoalvec = FirstGoalLine.Tail - FirstGoalLine.Head;
            Vector2D GoaliDefendTargetVec = cGoali.Center - TargetPos;
            double angle = Math.Abs(Vector2D.AngleBetweenInRadians(FirstGoalvec, GoaliDefendTargetVec));
        
            Vector2D tmpVec;
            if (Mode == GoaliPositioningMode.InRightSide)
                tmpVec = Vector2D.FromAngleSize(GoaliDefendTargetVec.AngleInRadians + angle, 1);
            else
                tmpVec = Vector2D.FromAngleSize(GoaliDefendTargetVec.AngleInRadians - angle, 1);

             DefenderBoundLine1 = new Line(TargetPos, TargetPos + tmpVec);
            Vector2D DefenderBoundVec1 = DefenderBoundLine1.Head - DefenderBoundLine1.Tail;
            Vector2D tmpV = Vector2D.FromAngleSize(DefenderBoundVec1.AngleInRadians + prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            Line tmpL = new Line(DefenderBoundLine1.Head + tmpV, (DefenderBoundLine1.Head + tmpV) - DefenderBoundVec1);
            Vector2D SecondGoalVec = SecondGoalLine.Head - SecondGoalLine.Tail;
            Vector2D tmpV2 = Vector2D.FromAngleSize(SecondGoalVec.AngleInRadians - prepAng, RobotParameters.OurRobotParams.Diameter / 2);
            SecondGoalParallelLine = new Line(SecondGoalLine.Head + tmpV2, (SecondGoalLine.Head + tmpV2) - SecondGoalVec);
            
            Position2D? DefenderPos = null;
            Line DefenderLine = new Line();
            Circle  DefenderBound = new Circle(GameParameters.OurGoalCenter, DefenderDist);
            if ((Mode == GoaliPositioningMode.InRightSide  && (-DefenderBoundVec1).AngleInDegrees >= (-SecondGoalVec).AngleInDegrees)||(Mode == GoaliPositioningMode.InLeftSide  && (-DefenderBoundVec1).AngleInDegrees <= (-SecondGoalVec).AngleInDegrees))
            {
                List<Position2D> tmpInts = DefenderBound.Intersect(SecondGoalLine);
                if (tmpInts.Count > 1)
                {
                    if (Math.Sign(tmpInts[0].Y) == Math.Sign(TargetPos.Y) && Math.Sign(tmpInts[1].Y) == Math.Sign(TargetPos.Y))
                        DefenderPos = (tmpInts[0].X < tmpInts[1].X) ? tmpInts[0] : tmpInts[1];
                    else if (Math.Sign(tmpInts[0].Y) == Math.Sign(TargetPos.Y))
                        DefenderPos = tmpInts[0];
                    else if (Math.Sign(tmpInts[1].Y) == Math.Sign(TargetPos.Y))
                        DefenderPos = tmpInts[1];
                    else
                        DefenderPos = (tmpInts[0].X < tmpInts[1].X) ? tmpInts[0] : tmpInts[1];
                }
                else if (tmpInts.Count == 1)
                    DefenderPos = tmpInts[0];
            }
            else
                DefenderPos = tmpL.IntersectWithLine(SecondGoalParallelLine);
            return DefenderPos;
        }
        private List<Position2D> ReCalculatePositions(WorldModel Model,SingleObjectState TargetState, GoaliPositioningMode Mode, Position2D GoaliPos, Position2D? DefenderPos, Line DefenderBoundLine1, bool isOnGoalLine, Line DefendTargetGoalLine, Line SecondGoalParallelLine)
        {
            Line DefenderLine = new Line();
            Circle DefenderBound  = new Circle(GameParameters.OurGoalCenter, DefenderDist);
            Circle GoaliBound = new Circle(GameParameters.OurGoalCenter, GoaliDist);

            if (DefenderPos.HasValue)
            {
                if (DefenderPos.Value.DistanceFrom(TargetPos) > 0.05)
                {
                    DefenderLine = new Line(TargetPos, DefenderPos.Value);
                    if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) > maxdefX && DefenderPos.Value.X < GameParameters.OurGoalCenter.X)
                    {
                        List<Position2D> possd = DefenderBound.Intersect(DefenderLine);
                        Position2D tmpDefenderPos = new Position2D();
                        if (possd.Count > 1)
                        {
                            if (Math.Sign(possd[0].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[1].Y) != Math.Sign(TargetPos.Y))
                                tmpDefenderPos = possd[0];
                            else if (Math.Sign(possd[1].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[0].Y) != Math.Sign(TargetPos.Y))
                                tmpDefenderPos = possd[1];
                            else
                                tmpDefenderPos = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                        }
                        else if (possd.Count == 1)
                            tmpDefenderPos = possd[0];
                        Circle DefenderCircle = new Circle(tmpDefenderPos, RobotParameters.OurRobotParams.Diameter / 2);
                        List<Line> tngDefL;
                        List<Position2D> tngDefP;
                        int tangCount = DefenderCircle.GetTangent(TargetPos, out tngDefL, out tngDefP);
                        double tngAng;
                        double vAng;
                        if (tangCount < 2)
                            return new List<Position2D>() { GoaliPos, tmpDefenderPos };
                        tngAng = Math.Abs(Vector2D.AngleBetweenInRadians(tngDefP[0] - TargetPos, tngDefP[1] - TargetPos));
                        vAng = Math.Abs(Vector2D.AngleBetweenInRadians(SecondGoalLine.Tail - SecondGoalLine.Head, DefenderBoundLine1.Tail - DefenderBoundLine1.Head));
                        double errAng = (vAng - tngAng) / 3;
                        Vector2D vec;
                        if (Mode == GoaliPositioningMode.InRightSide)
                            vec = Vector2D.FromAngleSize((tmpDefenderPos - TargetPos).AngleInRadians + errAng / 2, 1);
                        else
                            vec = Vector2D.FromAngleSize((tmpDefenderPos - TargetPos).AngleInRadians - errAng / 2, 1);

                        Line tmpLine = new Line(TargetPos, TargetPos + vec);
                        List<Position2D> ppp = DefenderBound.Intersect(tmpLine);
                        if (ppp.Count > 1)
                        {
                            if (Math.Sign(ppp[0].Y) == Math.Sign(TargetPos.Y) && Math.Sign(ppp[1].Y) != Math.Sign(TargetPos.Y))
                                DefenderPos = ppp[0];
                            else if (Math.Sign(ppp[1].Y) == Math.Sign(TargetPos.Y) && Math.Sign(ppp[0].Y) != Math.Sign(TargetPos.Y))
                                DefenderPos = ppp[1];
                            else
                                DefenderPos = (ppp[0].X < ppp[1].X) ? ppp[0] : ppp[1];
                        }
                        else if (ppp.Count == 1)
                            DefenderPos = ppp[0];
                        Vector2D vec2;
                        if (Mode == GoaliPositioningMode.InRightSide)
                            vec2 = Vector2D.FromAngleSize((GoaliPos - TargetPos).AngleInRadians + errAng, 1);
                        else
                            vec2 = Vector2D.FromAngleSize((GoaliPos - TargetPos).AngleInRadians - errAng, 1);
                        Line NewGoaliLine = new Line(TargetPos, TargetPos + vec2);

                        if (isOnGoalLine)
                        {
                            Line tmpLine2 = new Line(new Position2D(GameParameters.OurGoalCenter.X - GoaliX, -1), new Position2D(GameParameters.OurGoalCenter.X - GoaliX, 1));
                            Position2D? tmpp = tmpLine2.IntersectWithLine(NewGoaliLine);
                            GoaliPos = tmpp.Value;
                        }
                        else
                        {
                            List<Position2D> ppp2 = GoaliBound.Intersect(NewGoaliLine);
                            if (ppp2.Count > 1)
                                GoaliPos = (ppp2[0].X < ppp2[1].X) ? ppp2[0] : ppp2[1];
                            else if (ppp2.Count == 1)
                                GoaliPos = ppp2[0];
                        }
                    }
                    else if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) < mindefX || (DefenderPos.Value.X > GameParameters.OurGoalCenter.X))
                    {

                        List<Position2D> possd = DefenderBound.Intersect(DefenderLine);
                        Position2D pdef = new Position2D();
                        if (possd.Count > 1)
                        {
                            //if (Math.Sign(possd[0].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[1].Y) != Math.Sign(TargetPos.Y))
                            //    pdef = possd[0];
                            //else if (Math.Sign(possd[1].Y) == Math.Sign(TargetPos.Y) && Math.Sign(possd[0].Y) != Math.Sign(TargetPos.Y))
                            //    pdef = possd[1];
                            //else
                            //if (DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter) < mindefX)
                            //    pdef = DefenderPos.Value;
                            //if(DefenderPos.Value.X > GameParameters.OurGoalCenter.X)
                                pdef = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                            
                        }
                        //pdef = (possd[0].X < possd[1].X) ? possd[0] : possd[1];
                        else if (possd.Count == 1)
                            pdef = possd[0];
                        DefenderPos = pdef;
                    }
                }
                List<Position2D> tangposes;
                List<Line> tanglines;
                new Circle(DefenderPos.Value, RobotParameters.OurRobotParams.Diameter / 2).GetTangent(TargetPos, out tanglines, out tangposes);
                if (tangposes.Count > 1)
                {
                    Position2D selectedtng = (tangposes[0].Y >= tangposes[0].Y) ? tangposes[0] : tangposes[1];
                    Position2D Otng = (tangposes[0].Y < tangposes[0].Y) ? tangposes[0] : tangposes[1];
                    if (((selectedtng - TargetPos).AngleInDegrees >= (GameParameters.OurGoalLeft - TargetPos).AngleInDegrees) && ((Otng - TargetPos).AngleInDegrees <= (GameParameters.OurGoalRight - TargetPos).AngleInDegrees))
                    {
                        Line ll = new Line(TargetPos, GoaliPos);
                        Line goal = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                        Position2D? intersect = ll.IntersectWithLine(goal);
                        if (intersect.HasValue)
                        {
                            GoaliPos = GoaliPos + (GoaliPos - TargetPos).GetNormalizeToCopy(0.4);
                            if (GoaliPos.X > GameParameters.OurGoalCenter.X - 0.2)
                            {
                                GoaliPos = intersect.Value + (TargetPos - intersect.Value).GetNormalizeToCopy(0.2);
                            }
                        }
                        if (Math.Abs(GoaliPos.Y) > 0.25)
                            GoaliPos = (GameParameters.OurGoalCenter + new Vector2D(0, -0.2)) + (TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.15);
                    }
                }
                if (TargetPos.DistanceFrom(GameParameters.OurGoalCenter) < DefenderPos.Value.DistanceFrom(GameParameters.OurGoalCenter))
                {
                    Position2D? tp = DefendTargetGoalLine.IntersectWithLine(SecondGoalParallelLine);
                    if (tp.HasValue)
                    {
                        if (tp.Value.X > GameParameters.OurGoalCenter.X - minGoalidx)
                        {
                            Vector2D tmpGv = GameParameters.OurGoalCenter - TargetPos;
                            tp = TargetPos + tmpGv.GetNormalizeToCopy(0.25);
                        }
                        GoaliPos = tp.Value;
                    }
                }
                return new List<Position2D>() { GoaliPos, DefenderPos.Value };
            }
            return new List<Position2D>() { GoaliPos, new Position2D() };
        }
          
        public enum GoaliPositioningMode
        {
            InLeftSide,
            InRightSide
        }
    }
}

