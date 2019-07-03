using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;


namespace MRL.SSL.AIConsole.Skills
{
    class StarkCatchSkill
    {
        #region variables
        public int currentState = 1;
        Position2D firstBallPos;
        double angle = 0, counter = 0, stateCounter = 0;
        List<Position2D> lastBallPos = new List<Position2D>();
        List<Vector2D> vecHistoryList;
        Vector2D finalPassVec, roleBackVec;
        Line allMotionLine = new Line(), goalToRobotLine = new Line();
        Position2D PredictedBallPos = new Position2D(), target = new Position2D();
        bool startFlag = true, isSpinBack = true;
        public string MyCurrentState = "null";

        #endregion

        public void perform(GameStrategyEngine engine, WorldModel Model, int catcherID, bool PassIsChip, Position2D incomingBallTarget, bool IsSpin, int finishDelay = 60, double distanceToStartRollBack = 0.5, double rollBackLength = 0.15)
        {
            Planner.ChangeDefaulteParams(catcherID, false);
            Planner.SetParameter(catcherID, 3, 1.5);
            counter++;
            if (startFlag)
                processOnce(Model, incomingBallTarget);
            MyCurrentState = ((state)currentState).ToString();
            findPassLoc(Model, catcherID, incomingBallTarget);
            //drawOBJ(Model, catcherID);
            if (currentState == (int)state.goToPassTarget)
            {
                stateCounter++;
                angle = GameParameters.AngleModeD((Model.BallState.Location - Model.OurRobots[catcherID].Location).AngleInDegrees);
                target = incomingBallTarget;
                isSpinBack = false;
                if (Model.BallState.Speed.Size > 0.4 && lastBallPos.Count() > 0 && Model.BallState.Location.DistanceFrom(firstBallPos) > .10)
                {
                    stateCounter = 0;
                    currentState = (int)state.mainCatch;
                }

            }
            else if (currentState == (int)state.mainCatch)
            {
                stateCounter++;
                isSpinBack = true;
                angle = (finalPassVec).AngleInDegrees;
                target = PredictedBallPos;
                //Math.Exp(-(Model.BallState.Speed.Size - 0.02) * (Model.BallState.Speed.Size - 0.02) / (ActiveParameters.KpxVySide * ActiveParameters.KpxVySide));
                roleBackVec = (Model.BallState.Speed.Size < 0.7 || PassIsChip) ? (-finalPassVec).GetNormalizeToCopy(-0.05) : (-finalPassVec).GetNormalizeToCopy(rollBackLength);

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[catcherID].Location) < distanceToStartRollBack)
                {
                    stateCounter = 0;
                    currentState = (int)state.rollback;
                }
                if ((stateCounter > 180 && !PassIsChip) || stateCounter > 270)
                {
                    currentState = (int)state.failed;
                }
            }
            else if (currentState == (int)state.rollback)
            {
                stateCounter++;
                angle = (finalPassVec).AngleInDegrees;
                target = PredictedBallPos + roleBackVec;
                isSpinBack = true;
                //isSpinBack = IsSpin;
                //if (Model.OurRobots[catcherID].Location.DistanceFrom(target) < 0.15)
                //{

                //}
                if ((Model.BallState.Speed.Size < 1.5 && Model.BallState.Location.DistanceFrom(Model.OurRobots[catcherID].Location) < .125
                    && Model.OurRobots[catcherID].Location.DistanceFrom(target) < 0.10 && Model.OurRobots[catcherID].Speed.Size < 0.1) || Model.BallState.Speed.Size < .5)
                {
                    isSpinBack = true;
                    if (stateCounter > finishDelay)
                    {
                        currentState = (int)state.finished;
                    }
                }
                else if (stateCounter > finishDelay + 1)
                {
                    currentState = (int)state.failed;
                }
            }
            else if (currentState == (int)state.finished)
            {
                Planner.Add(catcherID, target, angle, PathType.UnSafe, false, true, true, true, /*IsSpin*/ false);
            }
            else if (currentState == (int)state.failed)
            {
                Planner.Add(catcherID, Model.OurRobots[catcherID].Location, angle, PathType.UnSafe, false, true, true, true, IsSpin);
            }

            if (currentState != (int)state.finished && currentState != (int)state.failed)
            {
                Planner.Add(catcherID, target, angle, PathType.UnSafe, false, true, false, false, false /* isSpinBack */);
            }

        }


        void findPassLoc(GameDefinitions.WorldModel Model, int RobotID, Position2D passTarget)
        {
            if (Model.BallState.Speed.Size > 0.5)
            {
                lastBallPos.Add(Model.BallState.Location);
                if (lastBallPos.Count > 1)
                {
                    Vector2D vec = new Vector2D();
                    vec = lastBallPos.Last() - Model.BallState.Location;
                    finalPassVec += (counter / 2) * vec + counter / 2 * (Model.PredictedBall[1 / StaticVariables.FRAME_RATE].Location - Model.BallState.Location);
                }
            }
            finalPassVec = finalPassVec.GetNormalizeToCopy(2);
            DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + finalPassVec, new Pen(Color.White, 0.01f)));
            DrawingObjects.AddObject(new Circle(Model.PredictedBall[1 / 60].Location, 0.025, new Pen(Color.White, 0.01f)));
            PredictedBallPos = finalPassVec.PrependecularPoint(Model.BallState.Location, passTarget);

        }
        void processOnce(GameDefinitions.WorldModel Model, Position2D incomingBallTarget)
        {

            MyCurrentState = "startFlag";
            lastBallPos = new List<Position2D>();
            vecHistoryList = new List<Vector2D>();
            firstBallPos = Model.BallState.Location;
            currentState = (int)state.goToPassTarget;
            startFlag = false;

        }
        void drawOBJ(GameDefinitions.WorldModel Model, int RobotID)
        {
            var robotLoc = Model.OurRobots[RobotID].Location;
            DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + (PredictedBallPos - Model.BallState.Location).GetNormalizeToCopy(3), new Pen(Color.FromArgb(100, 255, 179, 0), 0.05f)), "predicted ball pos..");
            DrawingObjects.AddObject(new StringDraw("distance to ball = " + Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location).ToString(), robotLoc.Extend(0.3, 0)), "Robot to ball distance..");
            DrawingObjects.AddObject(new StringDraw("state is: " + MyCurrentState.ToString(), robotLoc.Extend(0.4, 0)), "CurrentState..");
            DrawingObjects.AddObject(new StringDraw("ball speed =" + Model.BallState.Speed.Size.ToString(), robotLoc.Extend(0.50, 0)), "BalLspeed..");
            DrawingObjects.AddObject(new StringDraw("FrameCounter =" + counter.ToString(), robotLoc.Extend(0.60, 0)), "FrameCounter..");
            DrawingObjects.AddObject(new StringDraw("StateCounter =" + stateCounter.ToString(), robotLoc.Extend(0.7, 0)), "FrameCounter..");
            DrawingObjects.AddObject(new StringDraw("RobotSpeed =" + Model.OurRobots[RobotID].Speed.Size.ToString(), robotLoc.Extend(0.8, 0)), "RobotSpeed..");
            DrawingObjects.AddObject(new Circle(target, 0.09, new Pen(Color.FromArgb(100, 255, 179, 0), 0.01f)));

        }
        enum state
        {
            goToPassTarget = 0,
            mainCatch = 1,
            rollback = 2,
            finished = 3,
            failed = 4
        }
    }
}
