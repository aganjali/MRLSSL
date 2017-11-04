using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.Planning.GamePlanner
{
    public class DistanceAndAngle
    {

        public DistanceAndAngle()
        {
        }

        public void Calculate(List<WorldModel> model, GVector2D GlobalBallSpeed, ref GamePlannerInfo gpInfo)
        {
            WorldModel Model = model.Last();
            Vector2D vec, vec2;
            gpInfo.OppTeam.Distance = new Distances();
            gpInfo.OurTeam.Distance = new Distances();
            gpInfo.OppTeam.InDangerousZone = new Dictionary<int, double>();
            gpInfo.OurTeam.InDangerousZone = new Dictionary<int, double>();
            double dis = 0,dis2;
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), (gpInfo.OppTeam.GoaliID.HasValue) ? new List<int>() { gpInfo.OppTeam.GoaliID.Value } : null);
            Vector2D RobotBall ;
            Vector2D RobotTarget ;
            Position2D target = GameParameters.OppGoalCenter;
            double margin = 0.08, shootMargin = 0.03;
            foreach (var our in Model.OurRobots)
            {

                GVector2D dist = GameParameters.OurGoalCenter - our.Value.Location;
                gpInfo.OurTeam.Distance[our.Key, dist.X] = dist.X;
                if (GameParameters.IsInDangerousZone(our.Value.Location, true, 0.1, out dis, out dis2))
                    gpInfo.OurTeam.InDangerousZone[our.Key] = dis;
                gpInfo.OurTeam.DistanceFromBall[our.Key] = Model.BallState.Location - our.Value.Location;
                vec = GameParameters.OppGoalLeft - our.Value.Location;
                vec2 = GameParameters.OppGoalRight - our.Value.Location;
                gpInfo.OurTeam.GoalViewAngle[our.Key] = (float)Math.Abs((float)Vector2D.AngleBetweenInRadians(vec, vec2));
                RobotTarget = GameParameters.OppGoalCenter - our.Value.Location;
                RobotBall = Model.BallState.Location - our.Value.Location;
                int passState = 1, shootState = 1, passNear=0;

                double dballObs = 0, dourObs = 0;
                foreach (var ob in obs.ObstaclesList)
                {
                    if (passState != 4)
                    {
                        dballObs=ob.Value.State.Location.DistanceFrom(Model.BallState.Location);
                        dourObs=ob.Value.State.Location.DistanceFrom(our.Value.Location);
                        if (dballObs > 0.5 && dourObs> 1.2)
                        {
                            if (passState == 1 && ob.Value.Meet(Model.BallState, our.Value, margin + MotionPlannerParameters.BallRadi))
                                passState = 2;
                        }
                        else if (ob.Value.Meet(Model.BallState, our.Value, margin + MotionPlannerParameters.BallRadi))
                            passState = 4;
                        if (dourObs < 0.5)
                            passNear = 8;

                    }
                    if (shootState != 4)
                    {

                        if (ob.Value.State.Location.DistanceFrom(our.Value.Location) > 0.5)
                        {
                            if (shootState == 1 && ob.Value.Meet(new SingleObjectState(target, Vector2D.Zero, 0), our.Value, shootMargin + MotionPlannerParameters.BallRadi))
                                shootState = 2;
                        }
                        else if (ob.Value.Meet(new SingleObjectState(target, Vector2D.Zero, 0), our.Value, shootMargin + MotionPlannerParameters.BallRadi))
                            shootState = 4;
                        else
                            passNear = 8;
                    }
                }
                gpInfo.OurTeam.MarkingStatesToBall[our.Key] = (MarkingType)(passState);
                gpInfo.OurTeam.MarkingStatesToBall[our.Key] |= (MarkingType)((passState != 4) ? passNear : 0);
                
                gpInfo.OurTeam.MarkingStatesToTarget[our.Key] = (MarkingType)(shootState);
                gpInfo.OurTeam.MarkingStatesToTarget[our.Key] |= (MarkingType)((shootState != 4) ? passNear : 0);

                //DrawingObjects.AddObject(new StringDraw("state: " + gpInfo.OurTeam.MarkingStates[our.Key], our.Value.Location + new Vector2D(0.3, 0.3)));
                //if (obs.Meet(our.Value, Model.BallState, MotionPlannerParameters.BallRadi + margin))
                //{
                //    gpInfo.OurTeam.MarkingStates[our.Key] = MarkingType.ToBall;
                //    metBall = true;
                //}
                //bool metTarget = false;
                //foreach (var item2 in Model.Opponents)
                //{
                //    Circle C = new Circle(item2.Value.Location, MotionPlannerParameters.RobotRadi + MotionPlannerParameters.BallRadi + margin);
                //    if (our.Value.Location.DistanceFrom(item2.Value.Location) <= 1)
                //    {
                //        if ((item2.Value.Location - our.Value.Location).InnerProduct(GameParameters.OppGoalCenter - our.Value.Location) > 0 &&
                //            (item2.Value.Location - GameParameters.OppGoalCenter).InnerProduct(our.Value.Location - GameParameters.OppGoalCenter) > 0)
                //        {
                //            List<Position2D> intersects = C.Intersect(new Line(our.Value.Location, GameParameters.OppGoalCenter));
                //            if (intersects.Count > 0)
                //            {
                //                if (gpInfo.OurTeam.MarkingStates[our.Key] != MarkingType.None)
                //                    gpInfo.OurTeam.MarkingStates[our.Key] |= MarkingType.ToTarget;
                //                else
                //                    gpInfo.OurTeam.MarkingStates[our.Key] = MarkingType.ToTarget;
                //                metTarget = true;
                //                break;
                //            }
                //        }
                //    }
                //}

                //if (!metTarget && !metBall && obs.Meet(our.Value, 0.5 - MotionPlannerParameters.RobotRadi))
                //{
                //    if (gpInfo.OurTeam.MarkingStates[our.Key] != MarkingType.None)
                //        gpInfo.OurTeam.MarkingStates[our.Key] |= MarkingType.Near;
                //    else
                //        gpInfo.OurTeam.MarkingStates[our.Key] = MarkingType.Near;
                //}

            }
            foreach (var item in Model.Opponents)
            {
                GVector2D dist = item.Value.Location - GameParameters.OppGoalCenter;
                gpInfo.OppTeam.Distance[item.Key, dist.X] = dist.X;
                if (GameParameters.IsInDangerousZone(item.Value.Location, false, 0.1, out dis, out dis2))
                    gpInfo.OppTeam.InDangerousZone[item.Key] = dis;
                gpInfo.OppTeam.DistanceFromBall[item.Key] = Model.BallState.Location - item.Value.Location;
                vec = GameParameters.OurGoalLeft - item.Value.Location;
                vec2 = GameParameters.OurGoalRight - item.Value.Location;
                gpInfo.OppTeam.GoalViewAngle[item.Key] = (float)Math.Abs((float)Vector2D.AngleBetweenInRadians(vec, vec2));
                vec = GameParameters.OurGoalCenter - item.Value.Location;
                vec2 = Model.BallState.Location - item.Value.Location;
                gpInfo.OppTeam.ReflectAngle[item.Key] = (float)Math.Abs((float)Vector2D.AngleBetweenInRadians(vec, vec2));
            }
        }
     
    }
}

