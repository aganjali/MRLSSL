using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.GameDefinitions.General_Settings;

namespace MRL.SSL.AIConsole.Plays
{
    public class MoveTestPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            if (Model.Status == GameDefinitions.GameStatus.TestDefend)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, 8, typeof(TestRole)))
            //    Functions[8] = (eng, wmd) => GetRole<TestRole>(8).GetData(Model, 8, 0.5, 30);
            #region ScoreColor
            //if (isFirst)
            //{
            //    for (double i = -4; i <= 4; i += .2)
            //    {
            //        for (double j = -3; j <= 3; j += .2)
            //        {
            //            Position2D center = new Position2D(i, j);
            //            double score = Math.Abs(Model.BallState.Location.DistanceFrom(center));
            //            Color drawColor = new Color();

            //            if (score < 0.1)
            //                drawColor = Color.FromArgb(0, 128, 1);
            //            else
            //            {
            //                int scoreFit = ((int)(283.3 * score - 28.33) > 255) ? 255 : ((int)(283.3 * score - 28.33));

            //                drawColor = Color.FromArgb(scoreFit, scoreFit, scoreFit);
            //            }

            //            DrawingObjects.AddObject(new Circle(center, StaticVariables.BALL_RADIUS * 2, new Pen(drawColor, 0.01f), true, 1, true), "sdfsd" + i.ToString() + j.ToString());
            //        }
            //    }
            //    isFirst = false;
            //}

            #endregion
            ////int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            //List<int> oppIds;
            //if(goalieID.HasValue)
            //    oppIds = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != goalieID).OrderByDescending(w => w.Value).Select(s => s.Key).ToList();
            //else
            ////    oppIds = engine.GameInfo.OppTeam.Scores.OrderByDescending(w => w.Value).Select(s => s.Key).ToList();
            //int RobotID = 2;
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, RobotID, typeof(TestRole)))
            //    Functions[RobotID] = (eng, wmd) => GetRole<TestRole>(RobotID).GetData(Model, RobotID, 1, 30);
            //int id = 2;
            //SingleObjectState robot = Model.OurRobots[id];
            //bool isLeft = true;
            //SingleWirelessCommand SWC = new SingleWirelessCommand();
            //if (isFirst)
            //{
            //    if (isLeft)
            //    {
            //        if (robot.Angle.HasValue)
            //        {
            //            double angle = robot.Angle.Value;
            //            DrawingObjects.AddObject(new StringDraw("angle= " + angle.ToString(), new Position2D(1, 1)), "fds");
            //            Vector2D vec = Vector2D.FromAngleSize((angle - 90).ToRadian(), 0.5);
            //            Position2D center = robot.Location + vec;
            //            c = new Circle(center, 0.5, new Pen(Color.Red, 0.02f));
            //            isFirst = false;
            //        }
            //        //Vector2D circleR = Vector2D.FromAngleSize(
            //    }

            //}
            //DrawingObjects.AddObject(c, "fsdfsd)");

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, 2, typeof(TestRole)))
            //    Functions[2] = (eng, wmd) => GetRole<TestRole>(2).GetData(Model, 2, 0.5, 30);
            //SingleWirelessCommand SWC = new SingleWirelessCommand();
            //int id = 5;
            //SWC.RobotID = id;
            //SWC.KickSpeed = 8;
            //SWC.Vy = 0.2;

            //Planner.Add(id, SWC);

            //SingleObjectState robot = Model.OurRobots[0];
            //SingleObjectState target = Model.OurRobots[1];
            //if (robot.Angle.HasValue)
            //{
            //    double angle = robot.Angle.Value;
            //    if (angle>180)
            //    {
            //        angle -= 360;
            //    }
            //    DrawingObjects.AddObject(new StringDraw("angle= " + angle.ToString(), new Position2D(1, 1)), "fds");
            //    double targetangle = (target.Location - robot.Location).AngleInDegrees;
            //    DrawingObjects.AddObject(new StringDraw("targetAngle= " + targetangle.ToString(), new Position2D(1.3, 1)), "fddfdsfss");
            //    Planner.Add(0, robot.Location, targetangle, PathType.UnSafe, true, true, true, true);
            //}

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
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

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {

        }
    }
}
