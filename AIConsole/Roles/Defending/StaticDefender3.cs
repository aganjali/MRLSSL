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
namespace MRL.SSL.AIConsole.Roles.Defending
{
    class staticDefender3 : RoleBase
    {
        public List<int> oppAttackerIds = new List<int>();
        public Position2D target = new Position2D();
        Position2D intersectG = new Position2D();
        Position2D intersect = new Position2D();
        Position2D initialpos = new Position2D();
        Position2D? regional = null;
        public static int? oppMarkID;
        double angle;
        double velocity = 0;
        double treshTime = 0;
        double ballcoeff = 0;
        double robotCoeff = 0;
        double distNear = 0.18;
        double robotIntersectTime = 0;
        double GetNormalizeBehind = 0.15;
        double distToMark = 0.5 + RobotParameters.OurRobotParams.Diameter;
        private double markDistance = 0.180;
        bool cutFlag = false;
        bool testrole = false;
        bool ballIsMove = false;
        private bool firstTime = true;
        private bool noIntersect = false;
        private bool firsttimedanger = true;
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
            foreach (var item in Model.OurRobots.Where(w => w.Key != RobotID))
            {
                Circle c = new Circle(target, 0.22);
                if (c.IsInCircle(item.Value.Location))
                {
                    double d = 0.1 + (0.1 - (target.DistanceFrom(item.Value.Location)));
                    overlapTarget = target + (target - item.Value.Location).GetNormalizeToCopy(d);
                }
            }
            //DrawingObjects.AddObject(new StringDraw(target.toString(), Position2D.Zero.Extend(0.1, 0)));
           // return (overlapTarget.HasValue ? overlapTarget.Value : target);
            if (overlapTarget.HasValue)
            {
                target = overlapTarget.Value;
            }

             Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 10, 10);
            Planner.Add(RobotID, target, angle, PathType.UnSafe, false, false, true, true);
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
            return Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Position2D tempball = ballState.Location + ballState.Speed * 0.16;
            double d1, d2;
            List<RoleBase> res = new List<RoleBase>() { new StaticDefender1(), new StaticDefender2()};
            if (FreekickDefence.StaticFirstState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new ActiveRole());
            }
            if (FreekickDefence.StaticFirstState == DefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new NewActiveRole());
            }
            if (FreekickDefence.StaticFirstState == DefenderStates.BallInFront)
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

