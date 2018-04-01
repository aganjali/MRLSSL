using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Parameters of Field and Game that will be change by ruls like size,have to set from ssl-geometry in SSL Vision
    /// </summary>
    public class GameParameters
    {
        public static double BorderWidth = 0.01, GoalMouth = 1.00, GoalDepth = 0.18, BallDiameter = 0.043, FieldMarkerDiameter = 0.05, FieldCenterCircleDiameter = 1, DefenceAreaFrontWidth = 0.5, DefenceareaRadii = 1.00f;
        public static double RobotFrontLineAngle = Convert.ToSingle(Math.PI * 180 / (Math.PI * 3)), RobotCenterMarkerRadii = 0.025f;
        public static Color FieldColor = Color.Green, BallColor = Color.Orange, TeamMatesCenterColor = Color.Blue, OpponentCenterColor = Color.Yellow;

        public static Position2D OurGoalLeft = new Position2D(4.5, 0.500);
        public static Position2D OurGoalRight = new Position2D(4.5, -0.500);
        public static Position2D OurGoalCenter = Position2D.Interpolate(GameParameters.OurGoalRight, GameParameters.OurGoalLeft, 0.5);

        public static Position2D OppGoalLeft = new Position2D(-4.5, -0.500);
        public static Position2D OppGoalRight = new Position2D(-4.5, 0.500);
        public static Position2D OppGoalCenter = Position2D.Interpolate(GameParameters.OppGoalRight, GameParameters.OppGoalLeft, 0.5);
        public static Position2D OurLeftCorner = new Position2D(4.5, 3.000), OurRightCorner = new Position2D(4.5, -3.000);
        public static Position2D OppLeftCorner = new Position2D(-4.5, -3.000), OppRightCorner = new Position2D(-4.5, 3.000);
        public static Vector2D FieldMargins = new Vector2D(2.25, 1.25);

        private static List<Line> FieldLines = new List<Line>();
        public static List<Line> GetFieldLines()
        {
            if (FieldLines.Count < 4)
            {
                FieldLines = new List<Line>();
                FieldLines.Add(new Line(GameParameters.OurLeftCorner, GameParameters.OurLeftCorner + new Vector2D(-1, 0)));
                FieldLines.Add(new Line(GameParameters.OurLeftCorner, GameParameters.OurLeftCorner + new Vector2D(0, -1)));
                FieldLines.Add(new Line(GameParameters.OppLeftCorner, GameParameters.OppLeftCorner + new Vector2D(1, 0)));
                FieldLines.Add(new Line(GameParameters.OppLeftCorner, GameParameters.OppLeftCorner + new Vector2D(0, 1)));
            }
            return FieldLines;
        }
        public static List<Line> GetFieldLines(double margin)
        {
            List<Line> fieldLines = new List<Line>();

            Position2D p1 = new Position2D(GameParameters.OurLeftCorner.X + margin, GameParameters.OurLeftCorner.Y + margin);
            fieldLines.Add(new Line(p1, p1 + new Vector2D(-1, 0)));
            fieldLines.Add(new Line(p1, p1 + new Vector2D(0, -1)));
            p1 = new Position2D(GameParameters.OppLeftCorner.X - margin, GameParameters.OppLeftCorner.Y - margin);
            fieldLines.Add(new Line(p1, p1 + new Vector2D(1, 0)));
            fieldLines.Add(new Line(p1, p1 + new Vector2D(0, 1)));

            return fieldLines;
        }
        // public static Vector2D FieldMargins = new Vector2D(0.3, 0.3);
        /// <summary>
        /// Check if the position is in Field,without Margins
        /// </summary>
        /// <param name="pos">Target position</param>
        /// <returns>Avalablity of Input Position if it is in fieldS</returns>
        public static bool IsInField(Position2D pos, double margin)
        {
            if (pos.X < OppGoalCenter.X - margin || pos.X > OurGoalCenter.X + margin)
                return false;
            if (pos.Y < OurRightCorner.Y - margin || pos.Y > OurLeftCorner.Y + margin)
                return false; 
            return true;
        }
        public static bool IsInDangerousZone(Position2D Location, bool oppTeam, double margin, out double dist, out double DistFromBorder)
        {
            Position2D P;
            Vector2D vec;
            if (oppTeam)
            {
                P = new Position2D(GameParameters.OppGoalCenter.X, -GameParameters.DefenceAreaFrontWidth / 2);
                vec = Location - GameParameters.OppGoalCenter;
                Position2D pp = IntersectWithDangerZone(Location, false);
                DistFromBorder = Math.Abs((pp - GameParameters.OppGoalCenter).Size);
            }
            else
            {
                P = new Position2D(GameParameters.OurGoalCenter.X, GameParameters.DefenceAreaFrontWidth / 2);
                vec = GameParameters.OurGoalCenter - Location;
                Position2D pp = IntersectWithDangerZone(Location, true);
                DistFromBorder = Math.Abs((pp - GameParameters.OurGoalCenter).Size);
            }
            dist = vec.Size;

            if (Math.Abs(vec.Y) < (GameParameters.DefenceAreaFrontWidth / 2))
            {
                if (vec.X < GameParameters.DefenceareaRadii + margin)
                    return true;
            }
            else
            {
                Circle C;
                if (vec.Y < 0)
                    C = new Circle(P, GameParameters.DefenceareaRadii + margin);
                else
                    C = new Circle(new Position2D(P.X, -P.Y), GameParameters.DefenceareaRadii + margin);
                if (C.IsInCircle(Location))
                    return true;
            }
            //if (vec.X < 0 && vec.X >= -GameParameters.GoalDepth)
            //{
            //    if (Math.Abs(vec.Y) < GameParameters.GoalMouth / 2)
            //        return true;
            //}
            return false;
        }


        public static Position2D InFieldSize(Position2D pos)
        {
            if (!GameParameters.IsInField(pos, 0))
            {
                Position2D t = pos;
                double x = t.X, y = t.Y;
                if (Math.Abs(t.X) > Math.Abs(OurLeftCorner.X))
                    x = t.X - Math.Sign(t.X) * (Math.Abs(t.X) - Math.Abs(OurLeftCorner.X - 0.1));
                if (Math.Abs(t.Y) > Math.Abs(OurLeftCorner.Y))
                    y = t.Y - Math.Sign(t.Y) * (Math.Abs(t.Y) - Math.Abs(OurLeftCorner.Y - 0.1));
                return new Position2D(x, y);
            }
            return pos;
        }

        //public static double SafeRadi(SingleObjectState target, double margin)
        //{
        //    SingleObjectState Target = new SingleObjectState(target);
            
        //    if (!GameParameters.IsInField(Target.Location, 0))
        //    {
        //        Position2D t = Target.Location;
        //        double x = t.X, y = t.Y;
        //        if (Math.Abs(t.X) > Math.Abs(OurLeftCorner.X))
        //            x = t.X - Math.Sign(t.X) * (Math.Abs(t.X) - Math.Abs(OurLeftCorner.X - 0.1));
        //        if (Math.Abs(t.Y) > Math.Abs(OurLeftCorner.Y))
        //            y = t.Y - Math.Sign(t.Y) * (Math.Abs(t.Y) - Math.Abs(OurLeftCorner.Y - 0.1));
        //        Target.Location = new Position2D(x, y);
        //    }

        //    margin += 0.9;
        //    Position2D PosTarget = Target.Location;
        //    Position2D pright = new Position2D(GameParameters.OurGoalCenter.X - 0.8, GameParameters.OurGoalCenter.Y - 0.175);
        //    Position2D pleft = new Position2D(GameParameters.OurGoalCenter.X - 0.8, GameParameters.OurGoalCenter.Y + 0.175);
        //    Vector2D robotcent = PosTarget - GameParameters.OurGoalCenter;

        //    Position2D posalfa = new Position2D(GameParameters.OurGoalCenter.X, GameParameters.OurGoalCenter.Y - 0.175);
        //    Position2D posteta = new Position2D(GameParameters.OurGoalCenter.X, GameParameters.OurGoalCenter.Y + 0.175);
        //    double alfa = (pright - GameParameters.OurGoalCenter).AngleInDegrees;
        //    double teta = (pleft - GameParameters.OurGoalCenter).AngleInDegrees;
        //    double rr = 0;
        //    Position2D targetpos = Position2D.Zero;
        //    if (robotcent.AngleInDegrees > alfa && robotcent.AngleInDegrees <= -90)
        //        targetpos = posalfa + Vector2D.FromAngleSize(robotcent.AngleInRadians, margin);
        //    else if (robotcent.AngleInDegrees < teta && robotcent.AngleInDegrees >= 90)
        //        targetpos = posteta + Vector2D.FromAngleSize(robotcent.AngleInRadians, margin);
        //    else
        //    {

        //        double x2 = margin * margin;
        //        double y2 = PosTarget.Y * PosTarget.Y;
        //        double r = Math.Sqrt(x2 + y2);
        //        double x = GameParameters.OurGoalCenter.Extend(-margin, 0).X;
        //        targetpos = GameParameters.OurGoalCenter + Vector2D.FromAngleSize(robotcent.AngleInRadians, 1.5);
        //        targetpos = new Position2D(x, targetpos.Y);
        //    }

        //    return GameParameters.OurGoalCenter.DistanceFrom(targetpos);
        //}
        public static double SafeRadi(SingleObjectState target, double margin)
        {
            margin += 1.1;
            Position2D PosTarget = target.Location;
            Vector2D v = PosTarget - GameParameters.OurGoalCenter;

            Position2D pright = new Position2D(GameParameters.OurGoalCenter.X - 1, GameParameters.OurGoalCenter.Y - 0.25);
            Position2D pleft = new Position2D(GameParameters.OurGoalCenter.X - 1, GameParameters.OurGoalCenter.Y + 0.25);

            Vector2D leftBoundVec = pleft - GameParameters.OurGoalCenter;
            Vector2D rightBoundVec = GameParameters.OurGoalCenter - pright;
            Vector3D n = leftBoundVec * rightBoundVec;
            double innerL = n.InnerProduct(leftBoundVec * v) , innerR = n.InnerProduct(v * rightBoundVec);

            if (innerL >= 0 && innerR >= 0)
            {
                Vector2D leftCenter = new Vector2D(0, 1);
                double cos_theta = v.GetNormnalizedCopy().InnerProduct(leftCenter);
                double d1 = cos_theta * 0.25;
                double h2 = (1 - cos_theta * cos_theta) * 0.030625;
                double d2 = Math.Sqrt(Math.Max(margin * margin - h2, 0));
                return d1 + d2;
            }
            else if (innerL < 0 && innerR < 0)  
            {
                Vector2D rightCenter = new Vector2D(0, -1);
                double cos_theta = v.GetNormnalizedCopy().InnerProduct(rightCenter);
                double d1 = cos_theta * 0.25;
                double h2 = (1 - cos_theta * cos_theta) * 0.030625;
                double d2 = Math.Sqrt(Math.Max(margin * margin - h2,0));
                return d1 + d2;
            }
            else
            {
                Line l1 = new Line(GameParameters.OurGoalCenter, PosTarget);
                Line l2 = new Line(pright.Extend(1.00 - margin, 0), pleft.Extend(1.00 - margin, 0));
                Position2D p = new Position2D(GameParameters.OurGoalCenter.X - margin, 0);
                l1.IntersectWithLine(l2, ref p);

                return p.DistanceFrom(GameParameters.OurGoalCenter);
            }
        }

        public static Position2D IntersectWithDangerZone(Position2D Target, bool ourZone)
        {
            Position2D pos = new Position2D();
            Circle rightC, leftC;
            Line L;
            Position2D GoalCenter;
            double sgn = 1;
            if (ourZone)
            {
                if (Target.X >= GameParameters.OurGoalCenter.X)
                    return new Position2D(GameParameters.OurGoalCenter.X, (GameParameters.DefenceAreaFrontWidth / 2 + GameParameters.DefenceareaRadii) * Math.Sign(Target.Y));
                Position2D tmpR = new Position2D(OurGoalCenter.X, -DefenceAreaFrontWidth / 2);
                Position2D tmpL = new Position2D(OurGoalCenter.X, DefenceAreaFrontWidth / 2);
                rightC = new Circle(tmpR, DefenceareaRadii);
                leftC = new Circle(tmpL, DefenceareaRadii);
                L = new Line(tmpL + new Vector2D(-DefenceareaRadii, 0), tmpR + new Vector2D(-DefenceareaRadii, 0));
                GoalCenter = OurGoalCenter;
                sgn = -1;
            }
            else
            {
                if (Target.X <= GameParameters.OppGoalCenter.X)
                    return new Position2D(GameParameters.OppGoalCenter.X, (GameParameters.DefenceAreaFrontWidth / 2 + GameParameters.DefenceareaRadii) * Math.Sign(Target.Y));
                Position2D tmpR = new Position2D(OppGoalCenter.X, DefenceAreaFrontWidth / 2);
                Position2D tmpL = new Position2D(OppGoalCenter.X, -DefenceAreaFrontWidth / 2);
                rightC = new Circle(tmpR, DefenceareaRadii);
                leftC = new Circle(tmpL, DefenceareaRadii);
                L = new Line(tmpL + new Vector2D(DefenceareaRadii, 0), tmpR + new Vector2D(DefenceareaRadii, 0));
                GoalCenter = OppGoalCenter;
                sgn = 1;
            }
            Vector2D vec = Target - GoalCenter;
            Position2D posalfa = L.Tail;
            Position2D posteta = L.Head;
            double alfa = (posalfa - GoalCenter).AngleInDegrees;
            double teta = (posteta - GoalCenter).AngleInDegrees;
            Position2D pos2 = new Position2D();
            if (vec.AngleInDegrees > alfa && vec.AngleInDegrees <= sgn * 90)
            {
                Line l = new Line(Target, GoalCenter);
                List<Position2D> intersects = rightC.Intersect(l);
                if (intersects.Count == 1)
                    pos = intersects[0];
                else if (intersects.Count == 2)
                {
                    if (ourZone)
                    {
                        if (intersects[0].X == intersects[1].X)
                            pos2 = (intersects[0].Y < intersects[1].Y) ? intersects[0] : intersects[1];
                        else
                            pos2 = (intersects[0].X < intersects[1].X) ? intersects[0] : intersects[1];
                    }
                    else
                    {
                        if (intersects[0].X == intersects[1].X)
                            pos2 = (intersects[0].Y > intersects[1].Y) ? intersects[0] : intersects[1];
                        else
                            pos2 = (intersects[0].X > intersects[1].X) ? intersects[0] : intersects[1];
                    }
                }
            }
            else if (vec.AngleInDegrees < teta && vec.AngleInDegrees >= -sgn * 90)
            {
                Line l = new Line(Target, GoalCenter);
                List<Position2D> intersects = leftC.Intersect(l);
                if (intersects.Count == 1)
                    pos = intersects[0];
                else if (intersects.Count == 2)
                {
                    if (ourZone)
                    {
                        if (intersects[0].X == intersects[1].X)
                            pos2 = (intersects[0].Y > intersects[1].Y) ? intersects[0] : intersects[1];
                        else
                            pos2 = (intersects[0].X < intersects[1].X) ? intersects[0] : intersects[1];
                    }
                    else
                    {
                        if (intersects[0].X == intersects[1].X)
                            pos2 = (intersects[0].Y < intersects[1].Y) ? intersects[0] : intersects[1];
                        else
                            pos2 = (intersects[0].X > intersects[1].X) ? intersects[0] : intersects[1];
                    }
                }
            }
            else
            {
                Line l = new Line(Target, GoalCenter);
                Position2D? poss = l.IntersectWithLine(L);
                if (poss.HasValue)
                    pos2 = poss.Value;
            }

            return pos2;
        }
        public static Vector2D RotateCoordinates(Vector2D vec, double Rotation)
        {
            Vector2D v = Vector2D.Zero;
            Rotation = Rotation * Math.PI / 180;
            double cosTheta = Math.Cos(Rotation), sinTheta = Math.Sin(Rotation);
            v.X = vec.Y * cosTheta - vec.X * sinTheta;
            v.Y = vec.X * cosTheta + vec.Y * sinTheta; ;
            return v;
        }

        public static List<Position2D> LineIntersectWithDangerZone(Line TargetLine, bool ourZone)
        {
            if (TargetLine != null && TargetLine.Head != null)
            {
                Position2D rightLPos = new Position2D();
                Position2D rightCPos = new Position2D();
                Position2D leftLPos = new Position2D();
                Position2D leftCPos = new Position2D();
                List<Position2D> retpos = new List<Position2D>();
                Circle rightC, leftC;
                Line L;
                if (ourZone)
                {
                    Position2D tmpR = new Position2D(OurGoalCenter.X, -DefenceAreaFrontWidth / 2);
                    Position2D tmpL = new Position2D(OurGoalCenter.X, DefenceAreaFrontWidth / 2);
                    rightC = new Circle(tmpR, DefenceareaRadii);
                    leftC = new Circle(tmpL, DefenceareaRadii);
                    L = new Line(tmpL + new Vector2D(-DefenceareaRadii, 0), tmpR + new Vector2D(-DefenceareaRadii, 0));
                    rightLPos = tmpR + new Vector2D(-DefenceareaRadii, 0);
                    leftLPos = tmpL + new Vector2D(-DefenceareaRadii, 0);
                    leftCPos = tmpL + new Vector2D(0, DefenceareaRadii);
                    rightCPos = tmpR + new Vector2D(0, -DefenceareaRadii);
                }
                else
                {
                    Position2D tmpR = new Position2D(OppGoalCenter.X, DefenceAreaFrontWidth / 2);
                    Position2D tmpL = new Position2D(OppGoalCenter.X, -DefenceAreaFrontWidth / 2);
                    rightC = new Circle(tmpR, DefenceareaRadii);
                    leftC = new Circle(tmpL, DefenceareaRadii);
                    L = new Line(tmpL + new Vector2D(DefenceareaRadii, 0), tmpR + new Vector2D(DefenceareaRadii, 0));
                }

                List<Position2D> rightIntersects = rightC.Intersect(TargetLine);
                List<Position2D> LeftIntersects = leftC.Intersect(TargetLine);
                Position2D? linePos = TargetLine.IntersectWithLine(L);

                if ((TargetLine.Head.X > leftLPos.X || TargetLine.Tail.X > leftLPos.X))
                {
                    foreach (var item in rightIntersects)
                    {

                        if (IsInField(item, 0) && item.Y > rightCPos.Y && item.Y < rightLPos.Y)
                        {
                            retpos.Add(item);
                        }
                    }
                    foreach (var item in LeftIntersects)
                    {
                        if (IsInField(item, 0) && item.Y < leftCPos.Y && item.Y > leftLPos.Y)
                        {
                            retpos.Add(item);
                        }
                    }

                    if (linePos.HasValue && linePos.Value.Y > rightLPos.Y && linePos.Value.Y < leftLPos.Y)
                    {
                        retpos.Add(linePos.Value);
                    }
                }

                List<Position2D> temp = retpos;
                retpos = new List<Position2D>();
                foreach (var item in temp)
                {
                    Vector2D vec1 = TargetLine.Head - item;
                    Vector2D vec2 = TargetLine.Tail - item;
                    if (vec1.InnerProduct(vec2) < 0)
                        retpos.Add(item);
                }

                return retpos;
            }
            return new List<Position2D>();
        }

        public static Vector2D InRefrence(Vector2D v, Vector2D refrence)
        {
            Vector2D temVec = Vector2D.Zero;
            double angleRefrence = refrence.AngleInRadians;
            temVec.X = v.Y * Math.Cos(angleRefrence) - v.X * Math.Sin(angleRefrence);
            temVec.Y = v.X * Math.Cos(angleRefrence) + v.Y * Math.Sin(angleRefrence);
            return temVec;
        }
        public static double AngleModeD(double a)
        {
            if (a > 180)
            {
                while (a > 180)
                    a -= 360;
            }
            else if (a < -180)
            {
                while (a < -180)
                    a += 360;
            }

            return a;
        }
        public static double AngleModeR(double a)
        {
            if (a > Math.PI )
            {
                while (a > Math.PI)
                    a -= Math.PI * 2;
            }
            else if (a < -Math.PI)
            {
                while (a < -Math.PI)
                    a += Math.PI * 2;
            }

            return a;
        }
        public static Position2D? RobotHeadPosition(WorldModel Model,int RobotID,bool draw)
        {
            Position2D? p = null;
            bool RobotAngle = Model.OurRobots[RobotID].Angle.HasValue ? true : false;
            Position2D RobotFrontPos = new Position2D();
            if (RobotAngle)
            {
                Vector2D RobotFrontVec = new Vector2D();
                RobotFrontVec = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 0.09);
                RobotFrontPos = Model.OurRobots[RobotID].Location + RobotFrontVec.GetNormalizeToCopy(0.07);
                p = Model.OurRobots[RobotID].Location + (RobotFrontPos - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(0.07);
                if (draw)
                {
                    DrawingObjects.AddObject(p.Value, p.Value.X.ToString() + "536754");
                }
            }
            return p;
        }
        public static Position2D? RobotLeftCornerPosition(WorldModel Model, int RobotID, bool draw)
        {
            Position2D? p = null;
            bool RobotAngle = Model.OurRobots[RobotID].Angle.HasValue ? true : false;
            Position2D RobotFrontPos = new Position2D();
            if (RobotAngle)
            {
                Vector2D RobotFrontVec = new Vector2D();
                RobotFrontVec = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 0.09);
                RobotFrontPos = Model.OurRobots[RobotID].Location + RobotFrontVec.GetNormalizeToCopy(0.07);
                p = Model.OurRobots[RobotID].Location + ((RobotAngle ? RobotFrontPos : Model.BallState.Location) - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(0.07);
                Vector2D v1 = Vector2D.FromAngleSize((Model.OurRobots[RobotID].Location - p.Value).AngleInRadians + ((Math.PI / 180) * 90), 0.07);
                p = p.Value + v1;
                if (draw)
                {
                    DrawingObjects.AddObject(p.Value, p.Value.X.ToString() + "536+985");
                }
            }
            return p;
        }
        public static Position2D? RobotRightCornerPosition(WorldModel Model, int RobotID, bool draw)
        {
            Position2D? p = null;
            bool RobotAngle = Model.OurRobots[RobotID].Angle.HasValue ? true : false;
            Position2D RobotFrontPos = new Position2D();
            if (RobotAngle)
            {
                Vector2D RobotFrontVec = new Vector2D();
                RobotFrontVec = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 0.09);
                RobotFrontPos = Model.OurRobots[RobotID].Location + RobotFrontVec.GetNormalizeToCopy(0.07);
                p = Model.OurRobots[RobotID].Location + ((RobotAngle ? RobotFrontPos : Model.BallState.Location) - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(0.07);
                Vector2D v1 = Vector2D.FromAngleSize((Model.OurRobots[RobotID].Location - p.Value).AngleInRadians - ((Math.PI / 180) * 90), 0.07);
                p = p.Value + v1;
                if (draw)
                {
                    DrawingObjects.AddObject(p.Value, p.Value.X.ToString() + "536+985");
                }
            }
            return p;
        }
    }
}
