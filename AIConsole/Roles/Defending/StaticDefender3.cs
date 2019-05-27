using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Plays;
using System.Drawing;
namespace MRL.SSL.AIConsole.Roles
{
    class  staticDefender3 : RoleBase
    {
        public List<int> oppAttackerIds = new List<int>();
        public Position2D target = new Position2D();
        public static int? oppMarkID;
        double angle;
        double GetNormalizeBehind = 0.15;
        double distToMark = 0.5 + RobotParameters.OurRobotParams.Diameter;
        public SingleObjectState ballState = new SingleObjectState();
        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID, double markRegion, int? _oppMarkID, List<int> oppAttackerIds, List<int> oppValue1, List<int> oppValue2)
        {
            if (_oppMarkID.HasValue)
                oppMarkID = _oppMarkID;
            if (!_oppMarkID.HasValue || (_oppMarkID.HasValue && (!Model.Opponents.ContainsKey(_oppMarkID.Value) || (Model.Opponents.ContainsKey(_oppMarkID.Value) && Model.Opponents[_oppMarkID.Value].Location.X < markRegion))))
                oppMarkID = null;

            if (oppValue1.Count == 0 && oppValue2.Count == 2)
            {
                oppMarkID = oppValue2[1];
                angle = (Model.Opponents[oppMarkID.Value].Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            }
            else if (oppValue1.Count >= 1 && oppValue2.Count == 2)
            {
                oppMarkID = oppValue2[1];
                angle = (Model.Opponents[oppMarkID.Value].Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            }
            else if (oppValue1.Count == 0 && oppValue2.Count >= 3)
            {
                oppMarkID = oppValue2[2];
                angle = (Model.Opponents[oppMarkID.Value].Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            }
            else if (oppValue1.Count >= 1 && oppValue2.Count >= 3)
            {
                oppMarkID = oppValue2[1];
                angle = (Model.Opponents[oppMarkID.Value].Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            }
            else
            {
                angle = 180;
            }

            if (Model.BallState.Location.Y < 0 && oppValue2.Count <= 1)
            {
                target = new Position2D(4.80, 1.20);
            }
            else if (Model.BallState.Location.Y > 0 && oppValue2.Count <= 1)
            {
                target = new Position2D(4.80, -1.20);
            }
            else
            {
                Line lineOpp = new Line(Model.Opponents[oppMarkID.Value].Location, GameParameters.OurGoalCenter);
                List<Position2D> intersectwithdanger = GameParameters.LineIntersectWithOurDangerZone(lineOpp);
                if (intersectwithdanger.Count > 0 && (intersectwithdanger.FirstOrDefault()) != Position2D.Zero)
                    target = intersectwithdanger.OrderBy(o => o.DistanceFrom(GameParameters.OurGoalCenter)).FirstOrDefault();
                Vector2D ourGoalDangerzone = (target - GameParameters.OurGoalCenter);
                target = target + (ourGoalDangerzone).GetNormalizeToCopy(GetNormalizeBehind);
            }

            Position2D? overlapTarget = null;
            Circle c = new Circle(target, 0.22);

            var firstCondition = FreekickDefence.Static1ID.HasValue && c.IsInCircle(Model.OurRobots[FreekickDefence.Static1ID.Value].Location);
            var secondCondition = FreekickDefence.Static2ID.HasValue && c.IsInCircle(Model.OurRobots[FreekickDefence.Static2ID.Value].Location);
            if (firstCondition && secondCondition)
            {
                if (Model.OurRobots[FreekickDefence.Static1ID.Value].Location.DistanceFrom(target) < Model.OurRobots[FreekickDefence.Static2ID.Value].Location.DistanceFrom(target))
                {
                    target = target + ((target) - (Model.OurRobots[FreekickDefence.Static1ID.Value].Location)).GetNormalizeToCopy(Model.OurRobots[FreekickDefence.Static1ID.Value].Location.DistanceFrom(target) + 0.09);
                }
                else
                {
                    target = target + ((target) - (Model.OurRobots[FreekickDefence.Static2ID.Value].Location)).GetNormalizeToCopy(Model.OurRobots[FreekickDefence.Static2ID.Value].Location.DistanceFrom(target) + 0.09);
                }
            }
            else
            {
                foreach (var item in Model.OurRobots.Where(w => w.Key != RobotID))
                {
                    double DY = (item.Value.Location.Y - target.Y);
                    
                    if (item.Value.Location.DistanceFrom(target) < 0.22)//Math.Abs( DY) < .22)
                    {
                        //int sign = Math.Sign(target.Y);
                        Vector2D vec = (target - item.Value.Location );
                          target = target.Extend(0, (0.22 - Math.Abs(DY)) * Math.Sign(vec.Y));

                    }
                    
                }
            }
            DrawingObjects.AddObject(new Circle(target, 0.10, new Pen(Brushes.Red, 0.01f))); 
            //DrawingObjects.AddObject(new StringDraw(target.toString(), Position2D.Zero.Extend(0.1, 0)));
            // return (overlapTarget.HasValue ? overlapTarget.Value : target);
            if (overlapTarget.HasValue)
            {
                target = overlapTarget.Value;
            }

            //Planner.ChangeDefaulteParams(RobotID, false);
            //Planner.SetParameter(RobotID, 10, 10);
            NormalSharedState.CommonInfo.ST3Cost = target.DistanceFrom(Model.OurRobots[RobotID].Location);
            Planner.Add(RobotID, target, angle, PathType.UnSafe, false, true, true, true);
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return NormalSharedState.CommonInfo.ST3Cost;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Position2D tempball = ballState.Location + ballState.Speed * 0.16;
            double d1, d2;
            List<RoleBase> res = new List<RoleBase>() { new StaticDefender1(), new StaticDefender2(), new staticDefender3() };
            if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new ActiveRole());
            }
            if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new NewActiveRole());
            }

            if (FreekickDefence.StaticSecondState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new ActiveRole2017());
            }

            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }
    }
}

