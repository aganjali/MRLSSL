using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;

using ActionInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.ActionInfo;
using ActiveRoleState = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveRoleState;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Roles
{
    class RotateAndKick : ActionDescriptionBase
    {

        public override NormalSharedState.ActiveActionMode ActionCategory()
        {
            return NormalSharedState.ActiveActionMode.Drible;
        }

        public override void DetermineActionState(GameStrategyEngine engine, WorldModel Model, int RobotID, ActiveRoleState activeRoleState, ref ActionInfo actInfo)
        {
            base.DetermineActionState(engine, Model, RobotID, activeRoleState, ref actInfo);
            actInfo.dKind = NormalSharedState.ActiveDribleKind.RotateAndKick;
        }

        public override void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, ActiveRoleState activeRoleState, ref ActionInfo actInfo)
        {

        }

        public override double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, ActiveRoleState activeRoleState, ref ActionInfo actInfo)
        {
            return double.MaxValue;
            if ((activeRoleState == ActiveRoleState.Clear || activeRoleState == ActiveRoleState.LittleSpace) && ActiveParameters.NewActiveParameters.playMode == ActiveParameters.NewActiveParameters.PlayMode.PassAndDribble)
            {
                double radius = 0.5;
                double margin = 0.2;
                if (Model.BallState.Location.X < GameParameters.OppGoalCenter.X / 2)
                {
                    int sgn = 1;
                    int counter = 0;
                    int count = 4;
                    double[] scrs = new double[count];
                    
                    Position2D[] ps = new Position2D[count];
                    Circle[] cs = new Circle[count];
                    Position2D[] ends = new Position2D[count];
                    Position2D[] starts = new Position2D[count];
                    Vector2D[] rights = new Vector2D[count];
                    Vector2D[] lefts = new Vector2D[count];
                    Vector2D[] dirs = new Vector2D[count];
                    double[] diffAngs = new double[count];
                    do
                    {
                        int back = (counter >1)?1:0;

                        Vector2D translate = Vector2D.FromAngleSize((45.0).ToRadian() * sgn + back * Math.PI, radius);
                        Position2D p = Model.BallState.Location + translate;
                        Circle c = new Circle(p, radius + margin) { DrawPen = new Pen(Color.Purple, 0.01f) };
                        Circle cc = new Circle(p, Math.Max(0.1,radius)) { DrawPen = new Pen(Color.Purple, 0.01f) };

                        Vector2D right = (sgn == 1) ? -translate : (-translate).GetRotate((-translate).AngleInRadians + Math.PI / 2);
                        Vector2D left = (sgn == -1) ? -translate : (-translate).GetRotate((-translate).AngleInRadians - Math.PI / 2);

                        Vector2D dir = Vector2D.FromAngleSize(translate.AngleInRadians - sgn * Math.PI/ 2, radius);

                        Position2D end = p + Vector2D.FromAngleSize(sgn * (90.0).ToRadian(), radius);
                        Circle c2 = new Circle(p, radius);
                        bool nOpp = true;
                        Vector2D boundr = (counter == 0 || counter == 2) ? end - p : left, boundl = (counter == 0 || counter == 2) ? right : end - p;
                        foreach (var item in Model.Opponents.Keys)
                        {
                            Vector2D centerOppVec = Model.Opponents[item].Location - p;
                            Circle co = new Circle(Model.Opponents[item].Location, RobotParameters.OpponentParams.Diameter / 2 + 0.02);
                            if (c.IsInCircle(Model.Opponents[item].Location) && !cc.IsInCircle(Model.Opponents[item].Location) && (Vector2D.IsBetweenWithDirection(boundr, boundl, centerOppVec) || co.Intersect(new Line(p, end)).Count > 0 || co.Intersect(new Line(p, Model.BallState.Location)).Count > 0))
                            {
                                nOpp = false;
                                break;
                            }
                        }
                     
                        
                        double scr = -1;
                        double diffAng = 180;
                        if (nOpp)
                        {
                          //  double dist, DistFromBorder;
                            nOpp = (!(GameParameters.GetFieldLines(-.1).Any(a => c2.Intersect(a).Count > 0)))/* && !GameParameters.IsInDangerousZone(end, true, 0, out dist, out DistFromBorder)*/;
                            if (nOpp)
                            {
                                double oppDist = 0;
                                foreach (var item in Model.Opponents.Keys)
                                {
                                    oppDist += Model.Opponents[item].Location.DistanceFrom(end);
                                }
                                oppDist /= ((Model.Opponents.Keys.Count > 0) ? Model.Opponents.Keys.Count : 1);
                                var ivrs = engine.GameInfo.GetVisibleIntervals(Model, end, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                                double visInterval = 0;
                                if (ivrs.Count > 0)
                                {
                                    visInterval = ivrs.OrderBy(o => o.ViasibleWidth).FirstOrDefault().ViasibleWidth;
                                }
                                visInterval = Math.Max(visInterval, 0.01);
                                oppDist = 1 - Math.Min(oppDist, 2) / 2;
                                double robotAng = Model.OurRobots[RobotID].Angle.Value;
                                if (robotAng > 180)
                                    robotAng -= 360;
                                if (robotAng < -180)
                                    robotAng += 360;

                                diffAng = dir.AngleInDegrees - robotAng;
                                if (diffAng > 180)
                                    diffAng -= 360;
                                if (diffAng < -180)
                                    diffAng += 360;
                                double angCoef = 1 -  Math.Abs(diffAng) / 180;
                                angCoef = Math.Max(angCoef, 0.01);
                                scr = visInterval * oppDist * angCoef;
                            }
                        }
                        
                        scrs[counter] = scr;
                        ps[counter] = p;
                        cs[counter] = c2;
                        ends[counter] = end;
                        starts[counter] = Model.BallState.Location;
                        rights[counter] = right;
                        lefts[counter] = left;
                        dirs[counter] = dir;
                        diffAngs[counter] = diffAng;

                        sgn *= -1;
                        counter++;
                    } while (counter < count);

                    int idx = -1;
                    double maxScr = double.MinValue;
                    for (int i = 0; i < scrs.Length; i++)
                    {
                        if (scrs[i] >= 0 && scrs[i] > maxScr)
                        {
                            maxScr = scrs[i];
                            idx = i;
                        }
                    }
                    if (idx > -1)
                    {

                        DrawingObjects.AddObject(new Line(ps[idx], ps[idx] + rights[idx], new Pen(Color.Blue, 0.01f)), "rightBoundDriblevec" );
                        DrawingObjects.AddObject(new Line(ps[idx], ps[idx] + lefts[idx], new Pen(Color.Red, 0.01f)), "leftBoundDriblevec");
                        DrawingObjects.AddObject(cs[idx], "circleBoundDriblevec");

                    }

                }
            }
            return double.MaxValue;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }

}
