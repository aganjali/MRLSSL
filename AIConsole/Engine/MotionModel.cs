using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Diagnostics;
using System.Drawing;
using MRL.SSL.AIConsole.Merger_and_Tracker;

namespace MRL.SSL.AIConsole.Engine
{
    class MotionModel
    {
        Position2D BallAcceleration;
        public static Line _line;
        public Line Vector;
        private Stopwatch stopW = new Stopwatch();
        public MotionModel()
        {
            Vector = new Line();
            stopW = new Stopwatch();
            stopW.Start();
            BallAcceleration = new Position2D();
        }
        private Queue<Vector2D> _accelerationHistory = new Queue<Vector2D>();
        public WorldModel Predict(WorldModel Model, TimeSpan Time)
        {
            WorldModel newModel = new WorldModel();
            newModel = Model;
            if (Model.BallState != null)
            {
                newModel.BallState.Location.X = (BallAcceleration.X * Time.TotalSeconds * Time.TotalSeconds) / 2 + Model.BallState.Speed.X * Time.TotalSeconds + Model.BallState.Location.X;
                newModel.BallState.Location.Y = (BallAcceleration.Y * Time.TotalSeconds * Time.TotalSeconds) / 2 + Model.BallState.Speed.Y * Time.TotalSeconds + Model.BallState.Location.Y;
            }
            return newModel;
        }

        private Queue<WorldModel> _modelHistory = new Queue<WorldModel>();
        const int _numModelsToKeep = 100;
        public void FillDerivatives(WorldModel Model)
        {
            //// TODO : It is Superficial circle ///////////////
            //if (_modelHistory.Count > 4)
            //{
            //    WorldModel LastOurRobot = _modelHistory.ElementAt<WorldModel>(_modelHistory.Count - 2);
            //    foreach (int key in LastOurRobot.OurRobots.Keys)
            //    {
            //        if (LastOurRobot.OurRobots.ContainsKey(key))
            //        {
            //            if (Model.FieldIsInverted)
            //                DrawingObjects.AddObject(new Circle(LastOurRobot.OurRobots[key].Location.Reverse(), RobotParameters.OurRobotParams.Diameter / 2.0, new System.Drawing.Pen(Brushes.Magenta), true, 0.5f, false));
            //            else
            //                DrawingObjects.AddObject(new Circle(LastOurRobot.OurRobots[key].Location, RobotParameters.OurRobotParams.Diameter / 2.0, new System.Drawing.Pen(Brushes.Magenta), true, 0.5f, false));
            //        }
            //    }
            //}
            //////////////////////////////////////////////////

            Model.TimeElapsed = stopW.Elapsed;
            _modelHistory.Enqueue(Model);
            if (_modelHistory.Count > _numModelsToKeep)
                _modelHistory.Dequeue();
            if (_modelHistory.Count == 0)
                return;
            //Simplistic, should be replaced by more advanced methods
            Vector2D v, a = new Vector2D();


            v = new Vector2D();

            if (_modelHistory.Count > 10)
            {
                WorldModel lastModel = _modelHistory.ElementAt<WorldModel>(_modelHistory.Count - 10);
                //WorldModel lastModel2 = _modelHistory.ElementAt<WorldModel>(_modelHistory.Count - 15);
                if (Model.BallState.Speed != new Vector2D())
                    v = Model.BallState.Speed;
                else
                    v = (Model.BallState.Location - lastModel.BallState.Location) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds;
                a = (v - lastModel.BallState.Speed) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds;

                foreach (int key in Model.Opponents.Keys)
                {
                    if (lastModel.Opponents.ContainsKey(key))
                    {
                        double firstAngle = Model.Opponents[key].Angle.Value;
                        double secondAngle = lastModel.Opponents[key].Angle.Value;
                        if (!Model.Opponents[key].AngularSpeed.HasValue)
                        {
                            Model.Opponents[key].AngularSpeed = (float)(FindDistBetween2Angle(firstAngle, secondAngle) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds * (Math.PI / 180));
                        }
                        //TODO : I remove it to estimate by kalman
                        if (Model.Opponents[key].Speed == new Vector2D())
                        {
                            Model.Opponents[key].Speed = (Model.Opponents[key].Location - lastModel.Opponents[key].Location) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds;
                        }
                    }
                    else
                    {
                        Model.Opponents[key].AngularSpeed = 0;
                        Model.Opponents[key].Speed = new Vector2D();
                    }
                }
                //
                foreach (int key in Model.OurRobots.Keys)
                {
                    if (lastModel.OurRobots.ContainsKey(key))
                    {
                        double firstAngle = Model.OurRobots[key].Angle.Value;
                        double secondAngle = lastModel.OurRobots[key].Angle.Value;
                        //if (!Model.OurRobots[key].AngularSpeed.HasValue)
                        {
                            Model.OurRobots[key].AngularSpeed = (float)((FindDistBetween2Angle(firstAngle, secondAngle) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds) * (Math.PI / 180));
                        }
                        //TODO : I remove it to estimate by kalman
                        if (Model.OurRobots[key].Speed == new Vector2D())
                        {
                            Model.OurRobots[key].Speed = (Model.OurRobots[key].Location - lastModel.OurRobots[key].Location) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds;
                        }
                    }
                    else
                    {
                        Model.OurRobots[key].AngularSpeed = 0;
                        Model.OurRobots[key].Speed = new Vector2D();
                    }
                }
                //
                _line = new Line(lastModel.BallState.Location, Model.BallState.Location);
            }
            Model.BallState.Speed = v;
            Model.BallState.Acceleration = a;

        }

        public void FillDerivatives(frame frm, uint selctedball, WorldModel mainModel, out WorldModel Model)
        {
            Model = new WorldModel(mainModel);
            Model.TimeElapsed = stopW.Elapsed;

            if (frm.Balls.ContainsKey(selctedball))
                Model.BallState = new SingleObjectState(frm.Balls[selctedball].vision.pos, Vector2D.Zero, null);


            _modelHistory.Enqueue(Model);
            if (_modelHistory.Count > _numModelsToKeep)
                _modelHistory.Dequeue();
            if (_modelHistory.Count == 0)
                return;
            //Simplistic, should be replaced by more advanced methods
            Vector2D v, a = new Vector2D();


            v = new Vector2D();

            if (_modelHistory.Count > 10)
            {
                WorldModel lastModel = _modelHistory.ElementAt<WorldModel>(_modelHistory.Count - 10);
                //WorldModel lastModel2 = _modelHistory.ElementAt<WorldModel>(_modelHistory.Count - 15);
                if (Model.BallState.Speed != new Vector2D())
                    v = Model.BallState.Speed;
                else
                    v = (Model.BallState.Location - lastModel.BallState.Location) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds;
                a = (v - lastModel.BallState.Speed) / (Model.TimeElapsed - lastModel.TimeElapsed).TotalSeconds;

                _line = new Line(lastModel.BallState.Location, Model.BallState.Location);
            }
            Model.BallStateFast = new SingleObjectState();
            Model.BallStateFast.Speed = Vision2AI(v);
            Model.BallStateFast.Location = frm.Balls.ContainsKey(selctedball) ? Vision2AI(frm.Balls[selctedball].vision.pos) : Model.BallState.Location;
            Model.BallStateFast.Acceleration = Vision2AI(a);

        }

        private double FindDistBetween2Angle(double firstAngle, double secondAngle)
        {
            if (firstAngle < 0) firstAngle += 360;
            if (secondAngle < 0) secondAngle += 360;
            double ret2;

            if (firstAngle > secondAngle)
                ret2 = secondAngle + (360 - firstAngle);
            else
                ret2 = -(firstAngle + (360 - secondAngle));

            double ret3 = secondAngle - firstAngle;

            double ret = Math.Min(Math.Abs(ret2), Math.Abs(ret3));
            if (Math.Abs(ret) == Math.Abs(ret2))
                ret = ret2;
            else
                ret = ret3;

            return ret;
        }
        public Line PredictVector(WorldModel Model)
        {
            WorldModel lastModel = new WorldModel();
            if (_modelHistory.Count > 10)
            {
                lastModel = _modelHistory.ElementAt<WorldModel>(_modelHistory.Count - 10);
            }
            else
                lastModel = Model;

            return new Line(lastModel.BallState.Location, Model.BallState.Location);
        }

        private Position2D Vision2AI(Position2D pos)
        {
            return new Position2D(pos.X / 1000, -pos.Y / 1000);
        }
        private Vector2D Vision2AI(Vector2D vec)
        {
            return new Vector2D(vec.X / 1000, -vec.Y / 1000);
        }
        private double Vision2AI(double ang)
        {
            float Angle = (float)(-1 * ((180 / Math.PI) * ang));
            if (Angle > 180)
                Angle -= 360;
            else if (Angle < -180)
                Angle += 360;
            return Angle;
        }
    }
}
