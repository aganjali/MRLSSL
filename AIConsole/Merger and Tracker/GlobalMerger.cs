using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using System.Diagnostics;
using messages_robocup_ssl_wrapper;
using MRL.SSL.AIConsole.Plays;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public class GlobalMerger
    {
        public GlobalMerger()
        {
            Global_stp.Start();
        }
        private Tracker cTracker = new Tracker();
        private DecisionMaking cDMU = new DecisionMaking();
        public double ballposition_confidence;
        private bool OnGame = true;
        private bool CorrectErrorAngle = false;
        private int max_frame_to_find_from_shadow = 30;
        public int Max_frame_to_find_from_shadow
        {
            get { return max_frame_to_find_from_shadow; }
            set { max_frame_to_find_from_shadow = value; }
        }
        Stopwatch Global_stp = new Stopwatch();
        DrawCollection myDrawCollection = new DrawCollection();
        public Dictionary<int, Position2D> ballsViwed = new Dictionary<int, Position2D>();
        private double Max_Ball_dist = 0.7;
        //private VMerger merger = new VMerger();
        private NewMerger merger = new NewMerger();
        private VTracker tracker = new VTracker();
        public frame Frame = new frame();
        static public frame visionFrame = new frame();
        long lastTimeBallViewed = 0;
        PointF lastBall_Location;
        PointF lastShadow_Location;
        WorldModel Model = null;
        public uint selectedBall_Index;
        private bool ballIndexChanged;
        private Position2D selectedBall_Loc;
        private bool cameras_notset = true;
        private int shadow_counter = 0;
        private Dictionary<uint, vball> OtherBalls = new Dictionary<uint, vball>(10);
        ChipKickPrediction Predict = new ChipKickPrediction();
        bool isChip = false;

        public void SetParameters(MergerAndTrackerSetting MATS)
        {

            CorrectErrorAngle = MATS.CorrectAngleError;

            if (MATS.OnGame != null)
            {
                OnGame = MATS.OnGame;
            }

            StaticVariables.action_delay = MATS.ActionDelay;


            Max_frame_to_find_from_shadow = MATS.MaxFrameToShadow;


            Max_Ball_dist = MATS.MaxBallDist;
            tracker.Max_Ball_distance = MATS.MaxBallDist;
            cTracker.Max_to_imagine = MATS.MaxToImagine;


            cDMU.ShowRegion(MATS.CalculateRegion);


            tracker.Max_to_imagine = MATS.MaxToImagine;
            cTracker.Max_to_imagine = MATS.MaxToImagine;


            tracker.MaxNotSeen = MATS.MaxNotSeen;
            cTracker.MaxNotSeen = MATS.MaxNotSeen;


            tracker.Max_Opponent_distance = MATS.MaxOpponenetDistance;
            cTracker.Max_Opponent_distance = MATS.MaxOpponenetDistance;

            if (MATS.CamState != null)
            {
                if (MATS.CamState == MergerAndTrackerSetting.CameraState.Cam0)
                {
                    merger.CameraID = 0;
                    merger.OneCamera = true;
                }
                else if (MATS.CamState == MergerAndTrackerSetting.CameraState.Cam1)
                {
                    merger.CameraID = 1;
                    merger.OneCamera = true;
                }
                else if (MATS.CamState == MergerAndTrackerSetting.CameraState.All)
                {
                    merger.CameraID = 2;
                    merger.OneCamera = false;
                }
            }


        }
        public void setBallIndex(int? ballIndex, Position2D pos)
        {
            if (ballIndex.HasValue)
            {
                selectedBall_Index = (uint)ballIndex.Value;
                ballIndexChanged = true;
                selectedBall_Loc = pos;
            }
            else
            {
                lastBall_Location = pos;
            }
        }
        public void CameraParameters(SSL_WrapperPacket Packet)
        {
            if (Packet != null && Packet.geometry != null && cameras_notset == true)
            {
                for (int i = 0; i < Packet.geometry.calib.Count; i++)
                {
                    #region "Setting Game Parameters"
                    if (Packet.geometry.field.field_lines.Count > 0)
                        GameParameters.BorderWidth = Packet.geometry.field.field_lines[0].thickness / 1000.0;
                    if (Packet.geometry.field.field_arcs.Any(a => a.name == "CenterCircle"))
                        GameParameters.FieldCenterCircleDiameter = 2 * Packet.geometry.field.field_arcs.Where(w => w.name == "CenterCircle").First().radius / 1000.0;

                    GameParameters.OppGoalCenter = new Position2D(-Packet.geometry.field.field_length / 2000.0, 0);
                    GameParameters.OppGoalLeft = new Position2D(-Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.goal_width / 2000.0);
                    GameParameters.OppGoalRight = new Position2D(-Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.goal_width / 2000.0); ;
                    GameParameters.OppLeftCorner = new Position2D(-Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.field_width / 2000.0);
                    GameParameters.OppRightCorner = new Position2D(-Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.field_width / 2000.0);

                    GameParameters.OurGoalCenter = new Position2D(Packet.geometry.field.field_length / 2000.0, 0); ;
                    GameParameters.OurGoalLeft = new Position2D(Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.goal_width / 2000.0); ;
                    GameParameters.OurGoalRight = new Position2D(Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.goal_width / 2000.0); ; ;
                    GameParameters.OurLeftCorner = new Position2D(Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.field_width / 2000.0); ;
                    GameParameters.OurRightCorner = new Position2D(Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.field_width / 2000.0); ;
                    #endregion
                    RectangleF viewRect;
                    float cx = -Packet.geometry.calib[i].ty / 1000.0f;
                    float cy = Packet.geometry.calib[i].tx / 1000.0f;
                    if (cx < 0)
                    {
                        viewRect = new RectangleF(-3.5f, -2.5f, 3.5f + 0.3f, 5f);
                    }
                    else
                    {
                        viewRect = new RectangleF(-0.3f, -2.5f, 3.5f + 0.3f, 5f);
                    }
                    cDMU.AddCamera(new PointF(cx, cy), Packet.geometry.calib[i].tz / 1000.0f, viewRect);
                }
                cameras_notset = false;
            }
        }
        public void CameraParameters4Cam(SSL_WrapperPacket Packet)
        {
            if (Packet != null && Packet.geometry != null && cameras_notset == true)
            {
                for (int i = 0; i < Packet.geometry.calib.Count; i++)
                {
                    #region "Setting Game Parameters"
                    if (Packet.geometry.field.field_lines.Count > 0)
                        GameParameters.BorderWidth = Packet.geometry.field.field_lines[0].thickness / 1000.0;
                    if (Packet.geometry.field.field_arcs.Any(a => a.name == "CenterCircle"))
                        GameParameters.FieldCenterCircleDiameter = 2 * Packet.geometry.field.field_arcs.Where(w => w.name == "CenterCircle").First().radius / 1000.0;

                    GameParameters.OppGoalCenter = new Position2D(-Packet.geometry.field.field_length / 2000.0, 0);
                    GameParameters.OppGoalLeft = new Position2D(-Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.goal_width / 2000.0);
                    GameParameters.OppGoalRight = new Position2D(-Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.goal_width / 2000.0); ;
                    GameParameters.OppLeftCorner = new Position2D(-Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.field_width / 2000.0);
                    GameParameters.OppRightCorner = new Position2D(-Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.field_width / 2000.0);

                    GameParameters.OurGoalCenter = new Position2D(Packet.geometry.field.field_length / 2000.0, 0); ;
                    GameParameters.OurGoalLeft = new Position2D(Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.goal_width / 2000.0); ;
                    GameParameters.OurGoalRight = new Position2D(Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.goal_width / 2000.0); ; ;
                    GameParameters.OurLeftCorner = new Position2D(Packet.geometry.field.field_length / 2000.0, Packet.geometry.field.field_width / 2000.0); ;
                    GameParameters.OurRightCorner = new Position2D(Packet.geometry.field.field_length / 2000.0, -Packet.geometry.field.field_width / 2000.0); ;
                    #endregion
                    RectangleF viewRect;
                    float cx = -Packet.geometry.calib[i].ty / 1000.0f;
                    float cy = Packet.geometry.calib[i].tx / 1000.0f;

                    float margin = 1;
                    float width = Packet.geometry.field.field_length / 2000.0f + margin, height = Packet.geometry.field.field_width / 1000.0f + margin;

                    viewRect = new RectangleF(cx - width / 2, cy - height / 2, width, height);
                    // DrawingObjects.AddObject(new FlatRectangle(new Position2D(viewRect.X, viewRect.Y), new Vector2D(viewRect.Width, viewRect.Height)) { IsShown = true }, "rectgeo" + cx + "," + cy);
                    cDMU.AddCamera(new PointF(cx, cy), Packet.geometry.calib[i].tz / 1000.0f, viewRect);
                }
                cameras_notset = false;
            }
        }
        public WorldModel GetBallState(WorldModel Model, frame Frame, ref bool colision)
        {
            // if ongame =true we must choose one of balls automatically
            if (OnGame == true)
            {
                if (!tracker.SelectedBallIndexViewed(Frame, selectedBall_Index))
                {
                    if (merger.Balls.Count > 0)
                    {
                        double mindis = double.MaxValue;
                        int minNumNotViewed = int.MaxValue;
                        int minindex = -1;
                        var bls = Frame.Balls.OrderBy(o => o.Value.vision.notSeen).ToDictionary(k => k.Key, v => v.Value);
                        for (int i = 0; i < bls.Count; i++)
                        {
                            var key = bls.ElementAt(i).Key;
                            Position2D tmpP = Vision2AI(Frame.Balls[key].vision.pos);
                            if (tmpP.X > GameParameters.OppGoalLeft.X - StaticVariables.FieldMargin && tmpP.X < GameParameters.OurGoalLeft.X + StaticVariables.FieldMargin && tmpP.Y > GameParameters.OurRightCorner.Y - StaticVariables.FieldMargin && tmpP.Y < GameParameters.OurLeftCorner.Y + StaticVariables.FieldMargin)
                            {
                                if (Frame.Balls[key].vision.notSeen < minNumNotViewed)
                                {
                                    double dist = tmpP.DistanceFrom(lastBall_Location);
                                    if (dist < mindis)
                                    {
                                        minindex = (int)key;
                                        mindis = dist;
                                        minNumNotViewed = Frame.Balls[key].vision.notSeen;
                                    }
                                }
                                if (Frame.Balls.ContainsKey(selectedBall_Index))
                                {
                                    if (Frame.Balls[key].vision.lastCamViewd != Frame.Balls[selectedBall_Index].vision.lastCamViewd)
                                    {
                                        double dist = tmpP.DistanceFrom(lastBall_Location);
                                        if (dist < mindis)
                                        {
                                            minindex = (int)key;
                                            mindis = dist;
                                            minNumNotViewed = Frame.Balls[key].vision.notSeen;
                                        }
                                    }
                                }
                            }
                        }
                        //foreach (var key in Frame.Balls.Keys)
                        //{

                        //}
                        if (minindex != -1)
                        {
                            selectedBall_Index = (uint)minindex;
                        }
                    }
                }
            }
            if (!tracker.SelectedBallIndexViewed(Frame, selectedBall_Index))
            {
                if (!cameras_notset) //cameras was initialized
                {
                    //Create Region & find guessed ball location and set it in model
                    int ret = UpdateOtherBalls(Frame.Balls);
                    if (ret == -1)
                    {
                        shadow_counter++;
                        if (shadow_counter > max_frame_to_find_from_shadow)
                        {
                            cDMU.FindShadows(Model, lastShadow_Location);
                            if (Model.BallState != null)
                            {
                                double dist = Model.BallState.Location.DistanceFrom(lastBall_Location);
                                ballposition_confidence = 0.5 - (0.5 * Math.Pow((dist / 7.34), (1.0 / 3.0)));
                                Model.BallConfidenc = ballposition_confidence;
                                lastShadow_Location = Model.BallState.Location;
                                // Smart Shadow Enabled

                                if (ballposition_confidence < 0.2 || cDMU.isPointinShadow() == true)
                                {
                                    Model.BallState.Location = lastBall_Location;
                                    //Model.BallState.btimestamp = lastballTimeStamp;
                                    //Model.BallState.BVar = new MathMatrix(lastVar);
                                }
                            }
                            else
                            {
                                Model.BallState = new SingleObjectState();
                                Model.BallState.Location = lastBall_Location;
                                //Model.BallState.btimestamp = lastballTimeStamp;
                                //Model.BallState.BVar = new MathMatrix(lastVar);
                            }
                        }
                        else
                        {
                            ballposition_confidence = 1 - ((double)shadow_counter / (double)max_frame_to_find_from_shadow) * 0.5;  // our confidence about position of ball reduced when take time.
                            Model.BallConfidenc = ballposition_confidence;
                            if (Model.BallState == null)
                                Model.BallState = new SingleObjectState();
                            if (Frame.Balls.ContainsKey(selectedBall_Index))
                            {
                                if (tracker.ball[selectedBall_Index].IsImmobile() == true)
                                {
                                    Model.BallState.Location = new PointF(lastBall_Location.X, lastBall_Location.Y);
                                    //Model.BallState.btimestamp = lastballTimeStamp;
                                    //Model.BallState.BVar = new MathMatrix(lastVar);
                                }
                                else
                                {
                                    double dt = (double)((Global_stp.ElapsedMilliseconds - lastTimeBallViewed) / 1000.0) + StaticVariables.action_delay;
                                    double t = Frame.Balls[selectedBall_Index].vision.timestamp + dt - StaticVariables.action_delay;
                                    //dt = (Math.Floor(dt / (1.5 * view_delay)) + 2) * action_delay;
                                    int idx = tracker.bId2index[(int)selectedBall_Index];
                                    if (idx >= 0)
                                    {
                                        var tmp = tracker.GetBallData(dt, idx);
                                        Model.BallState.Location = Vision2AI(tmp.state.Location);
                                        Model.BallState.Speed = Vision2AI(tmp.state.Speed);
                                    }
                                    else
                                        Model.BallState.Location = new Position2D(lastBall_Location.X, lastBall_Location.Y);
                                    //Model.BallState.btimestamp = t;
                                    //Model.BallState.BVar = new MathMatrix(tmp.variances);
                                }
                            }
                            else
                            {
                                Model.BallState.Location = new PointF(lastBall_Location.X, lastBall_Location.Y);
                                //Model.BallState.btimestamp = lastballTimeStamp;
                                //Model.BallState.BVar = new MathMatrix(lastVar);
                            }
                        }
                    }
                    else
                    {
                        Position2D tmpP = Vision2AI(Frame.Balls[(uint)ret].vision.pos);
                        if (tmpP.X > GameParameters.OppGoalLeft.X - StaticVariables.FieldMargin && tmpP.X < GameParameters.OurGoalLeft.X + StaticVariables.FieldMargin && tmpP.Y > GameParameters.OurRightCorner.Y - StaticVariables.FieldMargin && tmpP.Y < GameParameters.OurLeftCorner.Y + StaticVariables.FieldMargin)
                        {
                            shadow_counter = 0;
                            ballposition_confidence = 1;  // we have no information about ball position
                            lastBall_Location = tmpP;
                            lastShadow_Location = lastBall_Location;
                            selectedBall_Index = (uint)ret;
                            if (Model.BallState == null)
                                Model.BallState = new SingleObjectState();
                            Model.BallState.Location = Vision2AI(Frame.Balls[(uint)ret].state.Location);
                            Model.BallState.Speed = Vision2AI(Frame.Balls[(uint)ret].state.Speed);
                            //Model.BallState.btimestamp = Frame.Balls[(uint)ret].vision.timestamp;
                            //Model.BallState.BVar = new MathMatrix(Frame.Balls[(uint)ret].variances);
                            //lastVar = new MathMatrix(Frame.Balls[(uint)ret].variances);
                            //lastballTimeStamp = Frame.Balls[(uint)ret].vision.timestamp;
                            Model.BallConfidenc = ballposition_confidence;
                        }
                        else
                        {
                            shadow_counter = 0;
                            ballposition_confidence = 1;  // we are 100% insured about ball position
                            Model.BallState.Location = lastBall_Location;
                            Model.BallConfidenc = ballposition_confidence;
                            //Model.BallState.btimestamp = lastballTimeStamp;
                            //Model.BallState.BVar = new MathMatrix(lastVar);
                            lastTimeBallViewed = Global_stp.ElapsedMilliseconds;
                        }
                    }
                }
                else
                {
                    ballposition_confidence = 0;  // we have no information about ball position
                    shadow_counter = 0;
                    // if cameras wasn't set then we would return last ball location 
                    if (Model.BallState == null)
                        Model.BallState = new SingleObjectState();
                    Model.BallState.Location = new PointF(lastBall_Location.X, lastBall_Location.Y);
                    //Model.BallState.btimestamp = lastballTimeStamp;
                    //Model.BallState.BVar = new MathMatrix(lastVar);
                    Model.BallConfidenc = ballposition_confidence;
                }
            }
            else
            {
                Position2D tmpP = Vision2AI(tracker.ReturnSelectedBallIndexPosition(Frame, selectedBall_Index));
                if (tmpP.X > GameParameters.OppGoalLeft.X - StaticVariables.FieldMargin && tmpP.X < GameParameters.OurGoalLeft.X + StaticVariables.FieldMargin && tmpP.Y > GameParameters.OurRightCorner.Y - StaticVariables.FieldMargin && tmpP.Y < GameParameters.OurLeftCorner.Y + StaticVariables.FieldMargin)
                {
                    shadow_counter = 0;
                    ballposition_confidence = 1;  // we are 100% insured about ball position
                    //Model.BallState.btimestamp = Frame.Balls[selectedBall_Index].vision.timestamp;
                    //Model.BallState.BVar = new MathMatrix(Frame.Balls[selectedBall_Index].variances);
                    Model.BallState.Location = tmpP;
                    Model.BallState.Speed = Vision2AI(tracker.ReturnSelectedBallIndexSpeed(Frame, selectedBall_Index));
                    Model.BallConfidenc = ballposition_confidence;
                    lastBall_Location = Vision2AI(Frame.Balls[selectedBall_Index].vision.pos);
                    lastTimeBallViewed = Global_stp.ElapsedMilliseconds;
                    SaveOtherBalls(Frame.Balls);
                    cDMU.Repository.Clear();
                }
                else
                {
                    shadow_counter = 0;
                    ballposition_confidence = 1;  // we are 100% insured about ball position
                    Model.BallState.Location = lastBall_Location;
                    //Model.BallState.btimestamp = lastballTimeStamp;
                    //Model.BallState.BVar = new MathMatrix(lastVar);
                    Model.BallConfidenc = ballposition_confidence;
                    lastTimeBallViewed = Global_stp.ElapsedMilliseconds;
                    //SaveOtherBalls(Frame.Balls);
                }

            }
            if (Frame.Balls.ContainsKey(selectedBall_Index))
                colision = Frame.Balls[selectedBall_Index].colision;
            else
                colision = false;
            return Model;
        }
        public WorldModel GetBallState(Tracker tracker)
        {
            if (OnGame == true)
            {
                if (!tracker.SelectedBallIndexViewed((int)selectedBall_Index))
                {
                    if (merger.Balls.Count > 0)
                    {
                        double mindis = double.MaxValue;
                        int minNumNotViewed = int.MaxValue;
                        int minindex = -1;
                        foreach (int key in tracker.ballHistory.Keys)
                        {
                            if (tracker.ballHistory[key].Position.X > GameParameters.OppGoalLeft.X - RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ballHistory[key].Position.X < GameParameters.OurGoalLeft.X + RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ballHistory[key].Position.Y > GameParameters.OurRightCorner.Y - RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ballHistory[key].Position.Y < GameParameters.OurLeftCorner.Y + RobotParameters.OurRobotParams.Diameter / 2.0)
                            {
                                if (tracker.ballHistory[key].NumNotViewed < minNumNotViewed)
                                {
                                    double dist = distance(tracker.ballHistory[key].Position, lastBall_Location);
                                    if (dist < mindis)
                                    {
                                        minindex = key;
                                        mindis = dist;
                                        minNumNotViewed = tracker.ballHistory[key].NumNotViewed;
                                    }
                                }
                                if (tracker.ballHistory.ContainsKey((int)selectedBall_Index))
                                {
                                    if (tracker.ballHistory[key].lastCamViewed != tracker.ballHistory[(int)selectedBall_Index].lastCamViewed)
                                    {
                                        double dist = distance(tracker.ballHistory[key].Position, lastBall_Location);
                                        if (dist < mindis)
                                        {
                                            minindex = key;
                                            mindis = dist;
                                            minNumNotViewed = tracker.ballHistory[key].NumNotViewed;
                                        }
                                    }
                                }
                            }
                        }
                        if (minindex != -1)
                        {
                            selectedBall_Index = (uint)minindex;
                        }
                    }
                }
            }
            if (!tracker.SelectedBallIndexViewed((int)selectedBall_Index))
            {
                if (!cameras_notset) //cameras was initialized
                {
                    //Create Region & find guessed ball location and set it in model
                    int ret = UpdateOtherBalls(tracker.ballHistory);
                    if (ret == -1)
                    {
                        shadow_counter++;
                        if (shadow_counter > max_frame_to_find_from_shadow)
                        {
                            cDMU.FindShadows(tracker.model, lastShadow_Location);
                            if (tracker.model.BallState != null)
                            {
                                double dist = distance(tracker.model.BallState.Location, lastBall_Location);
                                ballposition_confidence = 0.5 - (0.5 * Math.Pow((dist / 7.34), (1.0 / 3.0)));
                                tracker.model.BallConfidenc = ballposition_confidence;
                                lastShadow_Location = tracker.model.BallState.Location;

                                if (ballposition_confidence < 0.2 || cDMU.isPointinShadow() == true)
                                {
                                    tracker.model.BallState.Location = lastBall_Location;
                                }
                            }
                            else
                            {
                                tracker.model.BallState = new SingleObjectState();
                                tracker.model.BallState.Location = lastBall_Location;
                            }
                        }
                        else
                        {
                            ballposition_confidence = 1 - ((double)shadow_counter / (double)max_frame_to_find_from_shadow) * 0.5;  // our confidence about position of ball reduced when take time.
                            tracker.model.BallConfidenc = ballposition_confidence;
                            if (tracker.model.BallState == null)
                                tracker.model.BallState = new SingleObjectState();
                            if (tracker.ballHistory.ContainsKey((int)selectedBall_Index))
                            {
                                if (tracker.ballHistory[(int)selectedBall_Index].cKalman.isImmobile() == true)
                                {
                                    tracker.model.BallState.Location = new PointF(lastBall_Location.X, lastBall_Location.Y);
                                }
                                else
                                {
                                    double dt = (double)((Global_stp.ElapsedMilliseconds - lastTimeBallViewed) / 1000.0) + StaticVariables.action_delay;
                                    //dt = (Math.Floor(dt / (1.5 * view_delay)) + 2) * action_delay;
                                    Position2D newPos = tracker.ballHistory[(int)selectedBall_Index].cKalman.getEstimatedPosition(dt, tracker.model.BallConfidenc);
                                    tracker.model.BallState.Speed = tracker.ballHistory[(int)selectedBall_Index].cKalman.getEstimatedVelocity(dt, tracker.model.BallConfidenc);
                                    tracker.model.BallState.Location = new PointF((float)newPos.X, (float)newPos.Y);
                                }
                            }
                            else
                            {
                                tracker.model.BallState.Location = new PointF(lastBall_Location.X, lastBall_Location.Y);
                            }
                        }
                    }
                    else
                    {
                        if (tracker.ballHistory[ret].Position.X > GameParameters.OppGoalLeft.X - RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ballHistory[ret].Position.X < GameParameters.OurGoalLeft.X + RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ballHistory[ret].Position.Y > GameParameters.OurRightCorner.Y - RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ballHistory[ret].Position.Y < GameParameters.OurLeftCorner.Y + RobotParameters.OurRobotParams.Diameter / 2.0)
                        {
                            shadow_counter = 0;
                            ballposition_confidence = 1;  // we have no information about ball position
                            lastBall_Location = new PointF((float)tracker.ballHistory[ret].Position.X, (float)tracker.ballHistory[ret].Position.Y);
                            lastShadow_Location = lastBall_Location;
                            selectedBall_Index = (uint)ret;
                            if (tracker.model.BallState == null)
                                tracker.model.BallState = new SingleObjectState();
                            tracker.model.BallState.Location = new PointF((float)tracker.ballHistory[ret].action_EstimatedPosition.X, (float)tracker.ballHistory[ret].action_EstimatedPosition.Y);
                            tracker.model.BallState.Speed = tracker.ballHistory[ret].action_EstimatedVelocity;
                            tracker.model.BallConfidenc = ballposition_confidence;
                        }
                        else
                        {
                            shadow_counter = 0;
                            ballposition_confidence = 1;  // we are 100% insured about ball position
                            tracker.model.BallState.Location = lastBall_Location;
                            tracker.model.BallConfidenc = ballposition_confidence;
                            lastTimeBallViewed = Global_stp.ElapsedMilliseconds;
                        }
                    }
                }
                else
                {
                    ballposition_confidence = 0;  // we have no information about ball position
                    shadow_counter = 0;
                    // if cameras wasn't set then we would return last ball location 
                    if (tracker.model.BallState == null)
                        tracker.model.BallState = new SingleObjectState();
                    tracker.model.BallState.Location = new PointF(lastBall_Location.X, lastBall_Location.Y);
                    tracker.model.BallConfidenc = ballposition_confidence;
                }
            }
            else
            {
                if (tracker.ReturnSelectedBallIndexPosition((int)selectedBall_Index).X > GameParameters.OppGoalLeft.X - RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ReturnSelectedBallIndexPosition((int)selectedBall_Index).X < GameParameters.OurGoalLeft.X + RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ReturnSelectedBallIndexPosition((int)selectedBall_Index).Y > GameParameters.OurRightCorner.Y - RobotParameters.OurRobotParams.Diameter / 2.0 && tracker.ReturnSelectedBallIndexPosition((int)selectedBall_Index).Y < GameParameters.OurLeftCorner.Y + RobotParameters.OurRobotParams.Diameter / 2.0)
                {
                    shadow_counter = 0;
                    ballposition_confidence = 1;  // we are 100% insured about ball position
                    tracker.model.BallState.Location = tracker.ReturnSelectedBallIndexPosition((int)selectedBall_Index);
                    tracker.model.BallState.Speed = tracker.ReturnSelectedBallIndexSpeed((int)selectedBall_Index);
                    tracker.model.BallConfidenc = ballposition_confidence;
                    lastBall_Location = tracker.ballHistory[(int)selectedBall_Index].Position;
                    lastTimeBallViewed = Global_stp.ElapsedMilliseconds;
                    SaveOtherBalls(tracker.ballHistory);
                    cDMU.Repository.Clear();
                }
                else
                {
                    shadow_counter = 0;
                    ballposition_confidence = 1;  // we are 100% insured about ball position
                    tracker.model.BallState.Location = lastBall_Location;
                    tracker.model.BallConfidenc = ballposition_confidence;
                    lastTimeBallViewed = Global_stp.ElapsedMilliseconds;
                }

            }
            return tracker.model;
        }
        public void DrawingBalls(bool isReverseSide, frame Frame)
        {
            myDrawCollection = new DrawCollection();
            ballsViwed.Clear();

            double cornerPoints = 0.015;
            Color cornerColor = Color.GreenYellow;

            if (isReverseSide == false)
            {
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalLeft, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalRight, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppLeftCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppRightCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalCenter, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalLeft, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalRight, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurLeftCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurRightCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalCenter, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                foreach (var h in Frame.Balls)
                {
                    ballsViwed.Add((int)h.Key, Vision2AI(h.Value.vision.pos));
                }

                foreach (var h in Frame.OurRobots)
                {
                    myDrawCollection.AddObject(new StringDraw("*" + h.Key.ToString(), Color.Black, Vision2AI(h.Value.vision.pos)) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.OurRobot, Vision2AI(h.Value.vision.pos), Vector2D.Zero, Vector2D.Zero, (float?)Vision2AI(h.Value.vision.angle), 0) { IsShown = false }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, "*" + h.Key.ToString());
                }
                foreach (var h in Frame.OppRobots)
                {
                    myDrawCollection.AddObject(new StringDraw(h.Key.ToString(), Color.Black, Vision2AI(h.Value.vision.pos)) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.Opponent, Vision2AI(h.Value.vision.pos), Vector2D.Zero, Vector2D.Zero, (float?)Vision2AI(h.Value.vision.angle), 0) { IsShown = false }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, h.Key.ToString());
                }
            }
            else
            {
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalLeft.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalRight.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppLeftCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppRightCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalCenter.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalLeft.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalRight.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurLeftCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurRightCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalCenter.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                foreach (var h in Frame.Balls)
                {
                    Position2D newPos = Vision2AI(h.Value.vision.pos);
                    ballsViwed.Add((int)h.Key, new Position2D(-newPos.X, -newPos.Y));
                }
                foreach (var h in Frame.OurRobots)
                {
                    myDrawCollection.AddObject(new StringDraw("*" + h.Key.ToString(), Color.Black, Vision2AI(h.Value.vision.pos).Reverse()) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.OurRobot, Vision2AI(h.Value.vision.pos).Reverse(), Vector2D.Zero, Vector2D.Zero, (float?)Vision2AI(h.Value.vision.angle) + 180, 0) { IsShown = true }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, "*" + h.Key.ToString());
                }
                foreach (var h in Frame.OppRobots)
                {
                    myDrawCollection.AddObject(new StringDraw(h.Key.ToString(), Color.Black, Vision2AI(h.Value.vision.pos).Reverse()) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.Opponent, Vision2AI(h.Value.vision.pos).Reverse(), Vector2D.Zero, Vector2D.Zero, (float?)Vision2AI(h.Value.vision.angle) + 180, 0) { IsShown = true }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, h.Key.ToString());
                }
            }
            DrawingObjects.AddObject("Merger Tracker", myDrawCollection);
        }
        public void DrawingBalls(bool isReverseSide, Tracker tracker)
        {
            myDrawCollection = new DrawCollection();
            ballsViwed.Clear();

            double cornerPoints = 0.015;
            Color cornerColor = Color.GreenYellow;

            if (isReverseSide == false)
            {
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalLeft, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalRight, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppLeftCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppRightCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalCenter, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalLeft, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalRight, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurLeftCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurRightCorner, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalCenter, cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                foreach (KeyValuePair<int, History> h in tracker.ballHistory)
                {
                    ballsViwed.Add(h.Key, h.Value.Position);
                }

                foreach (KeyValuePair<int, History> h in tracker.ourRobotHistory)
                {
                    myDrawCollection.AddObject(new StringDraw("*" + h.Value.ID.ToString(), Color.Black, h.Value.Position) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.OurRobot, h.Value.Position, Vector2D.Zero, Vector2D.Zero, (float?)h.Value.Angle, 0) { IsShown = false }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, "*" + h.Value.ID.ToString());
                }
                foreach (KeyValuePair<int, History> h in tracker.opponentRobotHistory)
                {
                    myDrawCollection.AddObject(new StringDraw(h.Value.ID.ToString(), Color.Black, h.Value.Position) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.Opponent, h.Value.Position, Vector2D.Zero, Vector2D.Zero, (float?)h.Value.Angle, 0) { IsShown = false }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, h.Value.ID.ToString());
                }
            }
            else
            {
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalLeft.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalRight.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppLeftCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppRightCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OppGoalCenter.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalLeft.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalRight.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurLeftCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurRightCorner.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });
                myDrawCollection.AddObject(new Circle(GameParameters.OurGoalCenter.Reverse(), cornerPoints, new Pen(cornerColor, 0.01f)) { IsShown = false });

                foreach (KeyValuePair<int, History> h in tracker.ballHistory)
                {
                    Position2D newPos = h.Value.Position;
                    ballsViwed.Add(h.Key, new Position2D(-newPos.X, -newPos.Y));
                }
                foreach (KeyValuePair<int, History> h in tracker.ourRobotHistory)
                {
                    myDrawCollection.AddObject(new StringDraw("*" + h.Value.ID.ToString(), Color.Black, h.Value.Position.Reverse()) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.OurRobot, h.Value.Position.Reverse(), Vector2D.Zero, Vector2D.Zero, (float?)h.Value.Angle + 180, 0) { IsShown = true }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, "*" + h.Value.ID.ToString());
                }
                foreach (KeyValuePair<int, History> h in tracker.opponentRobotHistory)
                {
                    myDrawCollection.AddObject(new StringDraw(h.Value.ID.ToString(), Color.Black, h.Value.Position.Reverse()) { IsShown = false });
                    SingleObjectState s = new SingleObjectState(ObjectType.Opponent, h.Value.Position.Reverse(), Vector2D.Zero, Vector2D.Zero, (float?)h.Value.Angle + 180, 0) { IsShown = true }; s.Opacity = 0.3;
                    myDrawCollection.AddObject(s, h.Value.ID.ToString());
                }
            }
            DrawingObjects.AddObject("Merger Tracker", myDrawCollection);
        }

        public WorldModel GenerateWorldModel(SSL_WrapperPacket Packet, RobotCommands Commands, bool isYellow, bool isReverseSide)
        {
            WorldModel model = null;
            frame newFrame = new frame();
            CameraParameters(Packet);
            bool merged = merger.Merge(Packet, ref Frame, ref newFrame, isYellow, Position2D.Zero, false);

            if (merged)
            {
                Frame = tracker.FillHistory(Frame, merger.Balls);
                tracker.CallObservs(Frame, Commands);
                Frame = tracker.GetEstimated(Frame);
                model = tracker.CreateModelWitoutBall(Frame);
                DrawingBalls(isReverseSide, Frame);
                bool col = false;
                model = GetBallState(model, Frame, ref col);
                if (isYellow == true)
                    model.OurMarkerISYellow = true;
                else
                    model.OurMarkerISYellow = false;
                model.CurrentVisionPacket0 = merger.sslpacketCam0;
                model.CurrentVisionPacket0 = merger.sslpacketCam1;
            }
            if (model != null)
                Model = new WorldModel(model);
            else
                Model = null;
            return model;
        }
        List<Dictionary<uint, vrobot>> ModelsHistory = new List<Dictionary<uint, vrobot>>();
        int histIdx = 0;


        public WorldModel GenerateWorldModel4Cam(SSL_WrapperPacket Packet, RobotCommands Commands, bool isYellow, bool isReverseSide, TrackerType RobotType, TrackerType BallType, bool newCordinate, GameStatus status = GameStatus.Normal)
        {
            //vahid for test
            if (Commands != null && Commands.Commands.ContainsKey(testInfo.robotId))
            {
                testInfo.Vx = Commands.Commands[testInfo.robotId].Vx;
                testInfo.Vy = Commands.Commands[testInfo.robotId].Vy; 
            }
            //RobotType = TrackerType.Fast;
            WorldModel model = null;
            //   BallType = TrackerType.Fast;
            frame newFrame = new frame();
            CameraParameters4Cam(Packet);
            if (Packet != null && Packet.geometry != null)
                merger.sslVisionGeometry = Packet.geometry;
            
            //bool merged = merger.Merge4cam(Packet, ref Frame, ref newFrame, isYellow);
            bool merged;
            // try
            // {
            //merged = merger.Merge4cam(Packet, ref Frame, ref newFrame, isYellow);
            merged = merger.Merge(Packet, ref Frame, ref newFrame, isYellow, selectedBall_Loc, ballIndexChanged);
            //}
            //catch (Exception ex)
            //{

            //    merged = false;
            //}
            if (merged)
            {
                ballIndexChanged = false;
                visionFrame = Frame;
                if (RobotType == TrackerType.Accurate || BallType == TrackerType.Accurate)
                {
                    #region AccurateTracker
                    bool AccurateBall = (BallType == TrackerType.Accurate) ? true : false;
                    bool AccurateRobot = (RobotType == TrackerType.Accurate) ? true : false;
                    Frame = tracker.FillHistoryNew(Frame, ModelsHistory, merger.Balls, AccurateBall, AccurateRobot);
                    tracker.CallObservsNew(Frame, Commands, AccurateBall, AccurateRobot, newCordinate);
                    Frame = tracker.GetEstimatedNew(Frame, AccurateBall, AccurateRobot);
                    if (AccurateRobot)
                    {
                        WorldModel tmpModel = tracker.CreateModelWitoutBallNew(Frame, newCordinate);
                        model = new WorldModel();
                        // model.Opponents = tmpModel.Opponents;
                        model.OurRobots = tmpModel.OurRobots;
                    }
                    #endregion
                }
                //  if (RobotType == TrackerType.Fast || BallType == TrackerType.Fast)
                //{
                bool FastBall = (BallType == TrackerType.Fast) ? true : false;
                bool FastRobot = (RobotType == TrackerType.Fast) ? true : false;
                Frame = cTracker.FillHistoryNew(Frame, newFrame, merger.Balls, FastBall, FastRobot);
                cTracker.CreateEstimationNew(StaticVariables.action_delay, StaticVariables.viewDelay, status, FastBall, FastRobot);

                cTracker.CreateWorldModelNew(FastRobot);
                if (model == null)
                    model = new WorldModel();
                model.Opponents = cTracker.model.Opponents;
                if (FastRobot)
                    model.OurRobots = cTracker.model.OurRobots;
                //}
                if (BallType == TrackerType.Fast)
                {
                    DrawingBalls(isReverseSide, cTracker);
                    cTracker.model.BallState = new SingleObjectState();
                    cTracker.model = GetBallState(cTracker);
                    if (model == null)
                        model = new WorldModel();
                    model.BallState = cTracker.model.BallState;
                    #region "Restart Estimation at vicinity of our robots"
                    bool isRestarted = false;
                    foreach (var item in model.OurRobots)
                    {
                        if (item.Value.Location.DistanceFrom(cTracker.model.BallState.Location) < RobotParameters.OurRobotParams.Diameter / 2 + 0.05)
                        {
                            isRestarted = true;
                            RestartEstimationForCurrentBall();
                        }
                    }
                    foreach (var item in model.Opponents)
                    {
                        if (item.Value.Location.DistanceFrom(cTracker.model.BallState.Location) < RobotParameters.OurRobotParams.Diameter / 2 + 0.05)
                        {
                            isRestarted = true;
                            RestartEstimationForCurrentBall();
                        }
                    }
                    if (isRestarted == false)
                    {
                        if (cTracker.ballHistory.ContainsKey((int)selectedBall_Index))
                            cTracker.ballHistory[(int)selectedBall_Index].cKalman.IsRestarted = false;
                    }
                    #endregion
                }
                else if (BallType == TrackerType.Accurate)
                {
                    DrawingBalls(isReverseSide, Frame);
                    if (model == null)
                        model = new WorldModel();
                    model.BallState = new SingleObjectState();
                    bool col = false;
                    model = GetBallState(model, Frame, ref col);
                    //                DrawingObjects.AddObject(new StringDraw("col: " + col, "collision", Position2D.Zero));
                    model.PredictedBall.states = new List<GameDefinitions.SingleObjectState>();
                    model.PredictedBall.states = tracker.GetPredictedBallStateList(Frame, selectedBall_Index, StaticVariables.BallPredictTime, isReverseSide, false, col);
                    if (model.PredictedBall.states.Count == 0)
                        model.PredictedBall.states.Add(model.BallState);
                    DrawingObjects.AddObject(new Circle(model.PredictedBall[0.5].Location, 0.05) { DrawPen = new Pen(Color.DarkCyan, 0.01f) });
                }
                if (isYellow == true)
                    model.OurMarkerISYellow = true;
                else
                    model.OurMarkerISYellow = false;
                model.CurrentVisionPacket0 = merger.sslpacketCam0;
                model.CurrentVisionPacket1 = merger.sslpacketCam1;
                model.CurrentVisionPacket2 = merger.sslpacketCam2;
                model.CurrentVisionPacket3 = merger.sslpacketCam3;
                model.CurrentVisionPacket4 = merger.sslpacketCam4;
                model.CurrentVisionPacket5 = merger.sslpacketCam5;
                model.CurrentVisionPacket6 = merger.sslpacketCam6;
                model.CurrentVisionPacket7 = merger.sslpacketCam7;
                model.SslVisionGeometry = merger.sslVisionGeometry;
                MathMatrix A = new MathMatrix(2 * 4, 4), X = new MathMatrix(4, 1), B = new MathMatrix(8, 1);

                model.TimeElapsed = TimeSpan.FromSeconds(Frame.timeofcapture);

            }
            if (model != null)
                Model = model;
            else
            {
                Model = null;
                return null;
            }

            int maxCount = (int)Math.Round(StaticVariables.viewDelay / StaticVariables.FRAME_PERIOD);

            ModelsHistory.Add(Frame.OurRobots.Clone());
            if (ModelsHistory.Count > maxCount)
                ModelsHistory.RemoveAt(0);

            return new WorldModel(model);
        }

        private void SaveOtherBalls(Dictionary<uint, vball> Balls)
        {
            OtherBalls.Clear();
            foreach (var key1 in Balls.Keys)
            {
                bool exist = false;
                uint minKey = 0;

                //PointF Pos1 = new PointF((float)Balls[key1].vision.pos.X, (float)Balls[key1].vision.pos.Y);
                foreach (var key2 in OtherBalls.Keys)
                {
                    if (OtherBalls[key2].vision.pos.DistanceFrom(Vision2AI(Balls[key1].vision.pos)) < Max_Ball_dist)
                    {
                        minKey = key2;
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    if (Vision2AI(Balls[key1].vision.pos).DistanceFrom(lastBall_Location) > Max_Ball_dist)
                    {
                        vraw tmp = new vraw();
                        tmp.pos = Vision2AI(Balls[key1].vision.pos);
                        tmp.camera = 0;
                        tmp.conf = 1;
                        OtherBalls.Add((uint)OtherBalls.Keys.Count, new vball(tmp, new SingleObjectState()));
                    }
                }
                else
                {
                    OtherBalls.Remove(minKey);
                    vraw tmp = new vraw();
                    tmp.pos = Vision2AI(Balls[key1].vision.pos);
                    tmp.camera = 0;
                    tmp.conf = 1;
                    OtherBalls.Add(minKey, new vball(tmp, new SingleObjectState()));
                }
            }
        }
        private void SaveOtherBalls(Dictionary<int, History> Balls)
        {
            OtherBalls.Clear();
            foreach (int key1 in Balls.Keys)
            {
                bool exist = false;
                uint minKey = 0;

                PointF Pos1 = new PointF((float)Balls[key1].Position.X, (float)Balls[key1].Position.Y);
                foreach (var key2 in OtherBalls.Keys)
                {
                    if (distance(Pos1, OtherBalls[key2].vision.pos) < Max_Ball_dist)
                    {
                        minKey = key2;
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    if (distance(Pos1, lastBall_Location) > Max_Ball_dist)
                    {
                        vraw tmp = new vraw();
                        tmp.pos = Pos1;
                        tmp.camera = 0;
                        tmp.conf = 1;
                        OtherBalls.Add((uint)OtherBalls.Keys.Count, new vball(tmp, new SingleObjectState()));
                    }
                }
                else
                {
                    OtherBalls.Remove(minKey);
                    vraw tmp = new vraw();
                    tmp.pos = Pos1;
                    tmp.camera = 0;
                    tmp.conf = 1;
                    OtherBalls.Add(minKey, new vball(tmp, new SingleObjectState()));
                }
            }
        }
        private int UpdateOtherBalls(Dictionary<uint, vball> Balls)
        {
            foreach (var key1 in Balls.Keys)
            {
                if (Balls[key1].vision.notSeen >= 0)
                    continue;
                uint minKey = 0;
                double dist = 0;
                double minDist = double.MaxValue;

                // PointF Pos1 = new PointF((float)Balls[key1].Position.X, (float)Balls[key1].Position.Y);
                Position2D Pos1 = Vision2AI(Balls[key1].vision.pos);
                foreach (var key2 in OtherBalls.Keys)
                {
                    dist = Pos1.DistanceFrom(OtherBalls[key2].vision.pos);
                    if (dist < minDist)
                    {
                        minKey = key2;
                        minDist = dist;
                    }
                }
                if (minDist < Max_Ball_dist)
                {
                    //Update Current Position
                    OtherBalls.Remove(minKey);
                    vraw tmp = new vraw();
                    tmp.pos = Vision2AI(Balls[key1].vision.pos);
                    tmp.camera = 0;
                    tmp.conf = 1;
                    OtherBalls.Add(minKey, new vball(tmp, new SingleObjectState()));
                }
                else
                {
                    if (Balls[key1].vision.notSeen == -1)
                    {
                        //Return-->Key
                        return (int)key1;
                    }
                }
            }
            return -1;
        }
        private int UpdateOtherBalls(Dictionary<int, History> Balls)
        {
            foreach (int key1 in Balls.Keys)
            {
                if (Balls[key1].NumNotViewed >= 1)
                    continue;
                uint minKey = 0;
                double dist = 0;
                double minDist = double.MaxValue;

                PointF Pos1 = new PointF((float)Balls[key1].Position.X, (float)Balls[key1].Position.Y);
                foreach (var key2 in OtherBalls.Keys)
                {
                    dist = distance(Pos1, OtherBalls[key2].vision.pos);
                    if (dist < minDist)
                    {
                        minKey = key2;
                        minDist = dist;
                    }
                }
                if (minDist < Max_Ball_dist)
                {
                    //Update Current Position
                    OtherBalls.Remove(minKey);
                    vraw tmp = new vraw();
                    tmp.camera = 0;
                    tmp.conf = 1;
                    tmp.pos = Pos1;
                    OtherBalls.Add(minKey, new vball(tmp, new SingleObjectState()));
                }
                else
                {
                    if (Balls[key1].NumNotViewed == 0)
                    {
                        return key1;
                    }
                }
            }
            return -1;
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
        private double distance(PointF src, PointF dest)
        {
            double dx = (dest.X - src.X);
            double dy = (dest.Y - src.Y);
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public void RestartEstimationForCurrentBall()
        {
            if (cTracker.ballHistory.ContainsKey((int)selectedBall_Index))
                if (cTracker.ballHistory[(int)selectedBall_Index].cKalman.IsRestarted == false)
                {
                    cTracker.ballHistory[(int)selectedBall_Index].cKalman.IsRestarted = true;
                    cTracker.ballHistory[(int)selectedBall_Index].cKalman.RestartState();
                }
        }
    }
    public enum TrackerType
    {
        None,
        Accurate,
        Fast
    }
}