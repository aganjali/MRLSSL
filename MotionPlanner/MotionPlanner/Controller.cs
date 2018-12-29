using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.CommonClasses.Extentions;



namespace MRL.SSL.Planning.MotionPlanner
{

    public class Controller
    {
        public bool DontGoInDangerZone = true;
        private Vector2D
            _maxSpeed = new Vector2D(5, 5),
            _maxAcceleration = new Vector2D(6.5, 6.5),
            _maxDeceleration = new Vector2D(6.5, 6.5);

        public Vector2D TargetSpeed = new Vector2D(0, 0);
        private int _framCount = 60;
        double _mean = 0.5, _variance = 0.5;

        DrawCollection DG = new DrawCollection();
        Position2D lastPosition = Position2D.Zero;

        private double _breackingCoef = 1.2;

        public double BreackingCoef
        {
            get { return _breackingCoef; }
            set { _breackingCoef = value; }
        }


        private double _tunningDistance = 0.05;

        public double TunningDistance
        {
            get { return _tunningDistance; }
            set { _tunningDistance = value; }
        }

        private double _minDist = 0.005;

        public double MinDist
        {
            get { return _minDist; }
            set { _minDist = value; }
        }

        private double _tuningAngle = 0.1;
        public double TuningAngle
        {
            get { return _tuningAngle; }
            set { _tuningAngle = value; }
        }

        private double _minAngle = 0.02;
        public double MinAngle
        {
            get { return _minAngle; }
            set { _minAngle = value; }
        }

        double _maxAccelAngular = 20;
        public double MaxAccelAngular
        {
            get { return _maxAccelAngular; }
            set { _maxAccelAngular = value; }
        }

        public Vector2D MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }
        public Vector2D MaxDeceleration
        {
            get { return _maxDeceleration; }
            set { _maxDeceleration = value; }
        }
        public Vector2D MaxAcceleration
        {
            get { return _maxAcceleration; }
            set { _maxAcceleration = value; }
        }

        Tunner xTunner = new Tunner();
        Tunner yTunner = new Tunner();
        Tunner wTunner = new Tunner();

        public AdaptiveTunner aTunner = new AdaptiveTunner();
        bool inEndPhaseX = false, inEndPhaseY = false, inEndPhaseW = false;

        public SingleWirelessCommand CalculateTargetSpeed(WorldModel Model, int RobotID, Position2D Target, double TargetAngle, List<SingleObjectState> path, bool useDefultParams, ref Vector2D lastV, ref double lastAngularSpeed)
        {
            double _maxAngularSpeed = 10;
            double afactor = 0;
            double Wafactor = 0;
            double accuercy = 0;
            double Waccuercy = 0;

            Line lP = new Line(new Position2D(0, 1), new Position2D(_maxSpeed.X, 1));
            Line lI = new Line(new Position2D(0, 5), new Position2D(_maxSpeed.X, 0));
            if (useDefultParams)
            {
                _maxAcceleration = new Vector2D(ControlParameters.Accel, ControlParameters.Accel);
                _maxDeceleration = new Vector2D(ControlParameters.Decel, ControlParameters.Decel);
                afactor = ControlParameters.aFactor;
                Wafactor = ControlParameters.WaFactor;
                accuercy = ControlParameters.Accuercy;
                Waccuercy = ControlParameters.Waccuercy;
                _maxSpeed = new Vector2D(ControlParameters.MaxSpeed, ControlParameters.MaxSpeed);

                _maxAngularSpeed = ControlParameters.wMaxS;
                _maxAccelAngular = ControlParameters.wAccel;

                xTunner.P0 = ControlParameters.P0;
                yTunner.P0 = ControlParameters.P0;
                wTunner.P0 = ControlParameters.wP0;

                xTunner.P1 = ControlParameters.P1;
                yTunner.P1 = ControlParameters.P1;
                wTunner.P1 = ControlParameters.wP1;

                xTunner.Q0 = ControlParameters.Q0;
                yTunner.Q0 = ControlParameters.Q0;
                wTunner.Q0 = ControlParameters.wQ0;

                xTunner.Q1 = ControlParameters.Q1;
                yTunner.Q1 = ControlParameters.Q1;
                wTunner.Q1 = ControlParameters.wQ1;

                TunningDistance = ControlParameters.TunningDist;
                TuningAngle = ControlParameters.TunningAngle;
            }
            else
            {
                _maxAcceleration = new Vector2D(ControlParameters.Get(RobotID).Accel, ControlParameters.Get(RobotID).Accel);
                _maxDeceleration = new Vector2D(ControlParameters.Get(RobotID).Decel, ControlParameters.Get(RobotID).Decel);
                afactor = ControlParameters.Get(RobotID).aFactor;
                Wafactor = ControlParameters.Get(RobotID).WaFactor;
                accuercy = ControlParameters.Get(RobotID).Accuercy;
                Waccuercy = ControlParameters.Get(RobotID).Waccuercy;
                _maxSpeed = new Vector2D(ControlParameters.Get(RobotID).MaxSpeed, ControlParameters.Get(RobotID).MaxSpeed);

                _maxAngularSpeed = ControlParameters.Get(RobotID).wMaxS;
                _maxAccelAngular = ControlParameters.Get(RobotID).wAccel;

                xTunner.P0 = ControlParameters.Get(RobotID).P0;
                yTunner.P0 = ControlParameters.Get(RobotID).P0;
                wTunner.P0 = ControlParameters.Get(RobotID).wP0;

                xTunner.P1 = ControlParameters.Get(RobotID).P1;
                yTunner.P1 = ControlParameters.Get(RobotID).P1;
                wTunner.P1 = ControlParameters.Get(RobotID).wP1;

                xTunner.Q0 = ControlParameters.Get(RobotID).Q0;
                yTunner.Q0 = ControlParameters.Get(RobotID).Q0;
                wTunner.Q0 = ControlParameters.Get(RobotID).wQ0;

                xTunner.Q1 = ControlParameters.Get(RobotID).Q1;
                yTunner.Q1 = ControlParameters.Get(RobotID).Q1;
                wTunner.Q1 = ControlParameters.Get(RobotID).wQ1;

                TunningDistance = ControlParameters.Get(RobotID).TunningDist;
                TuningAngle = ControlParameters.Get(RobotID).TunningAngle;

            }

            _mean = ControlParameters.Mean;
            _variance = ControlParameters.Variance;

            #region ALL
            SingleObjectState stat = new SingleObjectState(Model.OurRobots[RobotID]);
            TargetSpeed = Vector2D.Zero;
            stat.AngularSpeed = (float?)lastAngularSpeed;
            stat.Speed = lastV;
            //if (RobotID == 2)
            //{
            //    CharterData.AddData("robotspeed2" + RobotID, Model.OurRobots[RobotID].Speed.Size);
            //    CharterData.AddData("robotspeedCom2" + RobotID, lastV.Size); 
            //    DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Model.OurRobots[RobotID].Speed, new Pen(Color.RosyBrown, 0.01f)));
            //}
            aTunner.UpdateCoefs(stat.Location, stat.Speed, stat.Angle.Value, stat.AngularSpeed.Value, Target, TargetAngle);
            //if (RobotID == 9)
            //    aTunner.Drawings(PIDType.X, RobotID);

            double pathWeight = ControlParameters.PathWeight;
            double pathLength = 0;

            if (path.Count > 2)
            {
                int idx = 0;
                Position2D tmpTarget = UpdateTarget(path, stat.Speed.Size, ref idx, ref pathLength);
                TargetSpeed = PathTrajectorySpeed3(Model.OurRobots[RobotID], path, tmpTarget, stat.Speed.Size, _maxSpeed.X, _maxAcceleration.X / afactor, pathLength, idx);
                Target = tmpTarget;
            }
            DrawingObjects.AddObject(new Circle(Target, 0.08, new Pen(Brushes.Aqua, 0.01f)));
            lastPosition = Target;
            double dX = Target.X - stat.Location.X, dY = Target.Y - stat.Location.Y, v0X = stat.Speed.X,
                v0Y = stat.Speed.Y, vfX = TargetSpeed.X, vfY = TargetSpeed.Y;
            double timeX = 0;
            double finalAx = 0;
            double timeY = 0;
            double finalAy = 0;
            double a = _maxAcceleration.Y;
            double d = _maxDeceleration.Y;

            if (Math.Abs(dX) < 0.001)
            {
                dX = 0;

                //  aTunner.Check4CollisionReset(PIDType.X);
            }

            if (Math.Abs(dY) < 0.001)
            {
                dY = 0;
                // aTunner.Check4CollisionReset(PIDType.Y);
            }

            #region Min

            int counter = 0;
            double u = Math.PI / 2, du = -Math.PI / 2;
            double vxMax = 0;
            double vyMax = 0;

            while (counter < 25)
            {
                du *= 0.5;
                double alpha = u + du;
                double axMax = Math.Sin(alpha) * _maxAcceleration.X;
                double ayMax = Math.Cos(alpha) * _maxAcceleration.Y;

                vxMax = Math.Sin(alpha) * _maxSpeed.X;//Math.Max(Math.Sin(alpha) * _maxSpeed.X, Math.Abs(stat.Speed.X));
                vyMax = Math.Cos(alpha) * _maxSpeed.Y;// Math.Max(Math.Cos(alpha) * _maxSpeed.Y, Math.Abs(stat.Speed.Y)); ;
                //if (counter == 24)
                //{ 
                //    ;
                //}
                compute_motion_1d(0, accuercy, -dX, stat.Speed.X, TargetSpeed.X, axMax, vxMax, afactor, ref finalAx, ref timeX);
                compute_motion_1d(0, accuercy, -dY, stat.Speed.Y, TargetSpeed.Y, ayMax, vyMax, afactor, ref finalAy, ref timeY);

                if (timeX - timeY <= 0)
                    u = alpha;

                counter++;
            }
            //if (Math.Abs(stat.Speed.X) - vxMax > Math.Abs(finalAx) * StaticVariables.FRAME_PERIOD )
            //{
            //    ;
            //}


            double trajTime = Math.Max(timeX, timeY);

            double angst = (Model.OurRobots[RobotID].Angle.Value > 180) ? Model.OurRobots[RobotID].Angle.Value - 360 : Model.OurRobots[RobotID].Angle.Value;
            double deltaAng = TargetAngle - angst;
            if (deltaAng > 180)
                deltaAng -= 360;
            else if (deltaAng < -180)
                deltaAng += 360;

            if (Math.Abs(deltaAng) < 0.3)
            {
                deltaAng = 0;
                // aTunner.Check4CollisionReset(PIDType.W);
            }

            deltaAng *= Math.PI / 180.0;

            counter = 0;
            double tAng = 0, angAcc = 0;
            double factor = 0.0;

            while (counter < 10)
            {
                factor += 0.1;
                compute_motion_1d(1, Waccuercy, -deltaAng, stat.AngularSpeed.Value, 0, _maxAccelAngular * factor, _maxAngularSpeed, Wafactor, ref angAcc, ref tAng);
                if (tAng < trajTime)
                    break;
                counter++;
            }
            #endregion

            double finalVx = finalAx / _framCount;
            double finalVy = finalAy / _framCount;
            double finalw = angAcc / _framCount;

            Vector2D Vtemp = new Vector2D(stat.Speed.X + finalVx, stat.Speed.Y + finalVy);
            double ww = stat.AngularSpeed.Value + finalw;

            if (Math.Abs(Vtemp.X) > vxMax)
                Vtemp.X = Math.Sign(Vtemp.X) * Math.Abs(vxMax);

            if (Math.Abs(Vtemp.Y) > vyMax)
                Vtemp.Y = Math.Sign(Vtemp.Y) * Math.Abs(vyMax);

            if (Math.Abs(ww) > _maxAngularSpeed)
                ww = Math.Sign(ww) * Math.Abs(ww);

            Vector2D V = new Vector2D();
            double outW = 0;
            #endregion
            //if (RobotID == 5)
            //    aTunner.Drawings(PIDType.X, 5);
            #region Accuercy
            if ((Math.Abs(dX) < TunningDistance && path.Count <= 2) || (path.Count > 2 && pathLength < _tunningDistance))
            {

                inEndPhaseX = true;
                //Vtemp.X = xTunner.Tune(dX, stat.Speed.X, 1, RobotID);
                Vtemp.X = aTunner.Tune(dX, RobotID, PIDType.X);
                double alfamax = 9;
                double alfa = (Vtemp.X - stat.Speed.X) * StaticVariables.FRAME_RATE;
                if (Math.Abs(alfa) > alfamax)
                    Vtemp.X = (stat.Speed.X) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;

            }
            else
            {
                if (inEndPhaseX)
                    aTunner.Check4CollisionReset(PIDType.X);

                inEndPhaseX = false;
                aTunner.Reset(PIDType.X);
            }
            // xTunner.Reset();

            if ((Math.Abs(dY) < TunningDistance && path.Count <= 2) || (path.Count > 2 && pathLength < _tunningDistance))
            {
                inEndPhaseY = true;
                //Vtemp.Y = yTunner.Tune(dY, stat.Speed.Y, 2, RobotID);
                Vtemp.Y = aTunner.Tune(dY, RobotID, PIDType.Y);
                double alfamax = 9;
                double alfa = (Vtemp.Y - stat.Speed.Y) * StaticVariables.FRAME_RATE;
                if (Math.Abs(alfa) > alfamax)
                    Vtemp.Y = (stat.Speed.Y) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
            }
            else
            {
                if (inEndPhaseY)
                    aTunner.Check4CollisionReset(PIDType.Y);
                inEndPhaseY = false;
                aTunner.Reset(PIDType.Y);
            }
            //yTunner.Reset();


            if (Math.Abs(deltaAng) < _tuningAngle)
            {
                inEndPhaseW = true;
                ww = wTunner.Tune(deltaAng, stat.AngularSpeed.Value, 3, RobotID);

                //ww = aTunner.Tune(deltaAng, RobotID, PIDType.W);
                double alfamax = 50;
                double alfa = (ww - stat.AngularSpeed.Value) * StaticVariables.FRAME_RATE;
                if (Math.Abs(alfa) > alfamax)
                    ww = (stat.AngularSpeed.Value) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
                //if (RobotID == 5)
                //{
                //    aTunner.Drawings(PIDType.W, RobotID);
                //}
            }
            else
            {
                if (inEndPhaseW)
                    aTunner.Check4CollisionReset(PIDType.W);

                inEndPhaseW = false;
                //aTunner.Reset(PIDType.W);
                wTunner.Reset();
            }
            //if (RobotID == 4||RobotID == 2)
            //{
            //    DrawingObjects.AddObject("TargetCircle" + RobotID, new Circle(Target, 0.09, new Pen(Color.BlueViolet, 0.01f)));
            //    DrawingObjects.AddObject("errorXControl" + RobotID, new StringDraw(dX.ToString(), Target + new Vector2D(0, 0.2)));
            //}

            lastAngularSpeed = ww;
            lastV = Vtemp;

            double Rotation = Model.OurRobots[RobotID].Angle.Value;
            Rotation *= Math.PI / (double)180;

            //      Rotation +=  100 * lastAngularSpeed * StaticVariables.FRAME_PERIOD;

            // lastLastW = lastAngularSpeed;

            #endregion


            #region Converting to Robot Axis

            Vector2D temp = new Vector2D(Vtemp.Y, Vtemp.X);
            V.X = temp.X * Math.Cos(Rotation) - temp.Y * Math.Sin(Rotation);
            V.Y = temp.Y * Math.Cos(Rotation) + temp.X * Math.Sin(Rotation);
            outW = -ww;



            #endregion

            DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vtemp), "6544");
            return new SingleWirelessCommand(V, outW, false, 0, 0, false, false);
        }
        //double lastLastW = 0;

        double Trajectory_1D(double deltaS, double startVel, double finalVel, double aMax, double vMax, ref double time)
        {

            double SamplingTime = 1.0 / (double)_framCount;
            if (deltaS == 0 && startVel == finalVel)
            {
                time = 0;
                return 0;
            }
            double timeToFinalVel = Math.Abs(startVel - finalVel) / aMax;
            // - ya + finalVel - startVel
            double distanceToFinalVel = Math.Abs(finalVel - startVel) / 2 * timeToFinalVel;
            double timeTemp, timeAcc, timeDec;
            if (Math.Abs(startVel) > Math.Abs(finalVel))
            {
                timeTemp = (Math.Sqrt((startVel * startVel + finalVel * finalVel) / 2 + Math.Abs(deltaS) * aMax) - Math.Abs(startVel)) / aMax;

                if (timeTemp < 0)
                    timeTemp = 0;
                timeAcc = timeTemp;
                timeDec = timeTemp + timeToFinalVel;
            }
            else if (distanceToFinalVel > Math.Abs(deltaS))
            {
                timeTemp = (Math.Sqrt(startVel * startVel + 2 * aMax * Math.Abs(deltaS)) - Math.Abs(startVel)) / aMax;
                timeAcc = timeTemp;
                timeDec = 0;
            }
            else
            {
                timeTemp = (Math.Sqrt((startVel * startVel + finalVel * finalVel) / 2 + Math.Abs(deltaS) * aMax) - Math.Abs(finalVel)) / aMax;
                if (timeTemp < 0)
                    timeTemp = 0;
                timeAcc = timeTemp + timeToFinalVel;
                timeDec = timeTemp;
            }

            double trajTime = timeAcc + timeDec;
            if (timeAcc * aMax + Math.Abs(startVel) > vMax)
                trajTime += (vMax - (aMax * timeAcc + Math.Abs(startVel))) * (vMax - (aMax * timeAcc + Math.Abs(startVel))) / (aMax * vMax);

            double trajAcc = 0;
            if (timeAcc < SamplingTime && timeDec < 0.1)
                trajAcc = (finalVel - startVel) * (SamplingTime);
            else if (timeAcc < SamplingTime && timeDec > 0)
                trajAcc = Math.Sign(finalVel - startVel) * (aMax * timeAcc + (double)_framCount * (aMax * (SamplingTime - timeAcc))); /* trajAcc = (aMax * timeAcc) + (-aMax * (SamplingTime - timeAcc))*/
            else
                trajAcc = aMax * Math.Sign(deltaS);

            time = trajTime;
            return trajAcc;

        }

        public void compute_motion_1d(int dim, double accuercyCoef, double x0, double v0, double v1,
                               double a_max, double v_max, double a_factor,
                               ref double traj_accel, ref double traj_time)
        {
            // First check to see if nothing needs to be done...
            if (x0 == 0 && v0 == v1) { traj_accel = 0; traj_time = 0; return; }


            // Need to do some motion.
            a_max /= a_factor;

            double time_to_v1 = Math.Abs(v0 - v1) / a_max;  // Por que?
            double x_to_v1 = Math.Abs((v0 + v1) / 2.0) * time_to_v1; //

            double period = accuercyCoef * StaticVariables.FRAME_PERIOD; // Minimum time that vision feedback can be used for precise motion


            v1 = copysign(v1, -x0);
            // state 0
            if (v0 * x0 > 0 || (Math.Abs(v0) > Math.Abs(v1) && x_to_v1 > Math.Abs(x0)))
            {
                // Time to reach goal after stopping + Time to stop.
                double time_to_stop = Math.Abs(v0) / a_max;
                double x_to_stop = v0 * v0 / (2 * a_max);

                compute_motion_1d(0, accuercyCoef, x0 + copysign(x_to_stop, v0), 0, v1, a_max * a_factor,
                                  v_max, a_factor, ref traj_accel, ref traj_time);
                traj_time += time_to_stop;

                // Decelerate
                if (traj_time < period)
                    traj_accel = compute_stop(v0, a_max * a_factor);
                else if (time_to_stop < period)
                    traj_accel = time_to_stop / period * -copysign(a_max * a_factor, v0) +
                  (1.0 - time_to_stop / period) * traj_accel;
                else traj_accel = -copysign(a_max * a_factor, v0);
                return;
            }


            // At this point we have two options.  We can maximally accelerate
            // and then maximally decelerate to hit the target.  Or we could
            // find a single acceleration that would reach the target with zero
            // velocity.  The later is useful when we are close to the target
            // where the former is less stable.

            // OPTION 1
            // This computes the amount of time to accelerate before decelerating.
            double t_a, t_accel, t_decel;
            if (Math.Abs(v0) > Math.Abs(v1))
            {
                t_a = (Math.Sqrt((3 * v1 * v1 + v0 * v0) / 2.0 - Math.Abs(v0 * v1) + Math.Abs(x0) * a_max)
                   - Math.Abs(v0)) / a_max;
                //t_a = (Math.Sqrt((v0 * v0 + v1 * v1) / 2.0 + Math.Abs(x0) * a_max)
                //   - Math.Abs(v0)) / a_max;

                if (t_a < 0.0) t_a = 0;
                t_accel = t_a;
                t_decel = t_a + time_to_v1;
            }
            else if (x_to_v1 > Math.Abs(x0))
            {
                t_a = (Math.Sqrt(v0 * v0 + 2 * a_max * Math.Abs(x0)) - Math.Abs(v0)) / a_max;
                t_accel = t_a;
                t_decel = 0.0;
            }
            else
            {
                //    t_a = (sqrt((3*v0*v0 + v1*v1) / 2.0 - fabs(v0 * v1) + fabs(x0) * a_max) 
                //  - fabs(v1)) / a_max;

                t_a = (Math.Sqrt((v0 * v0 + v1 * v1) / 2.0 + Math.Abs(x0) * a_max)
                 - Math.Abs(v1)) / a_max;

                if (t_a < 0.0) t_a = 0;
                t_accel = t_a + time_to_v1;
                t_decel = t_a;
            }

            // OPTION 2
            double a_to_v1_at_x0 = (v0 * v0 - v1 * v1) / (2 * Math.Abs(x0));
            double t_to_v1_at_x0 =
              (-Math.Abs(v0) + Math.Sqrt(v0 * v0 + 2 * Math.Abs(a_to_v1_at_x0) * Math.Abs(x0))) /
              Math.Abs(a_to_v1_at_x0);

            // We follow OPTION 2 if t_a is less than a FRAME_PERIOD making it
            // difficult to transition to decelerating and stopping exactly.
            if (false && a_to_v1_at_x0 < a_max && a_to_v1_at_x0 > 0.0 &&
                t_to_v1_at_x0 < 2.0 * StaticVariables.FRAME_PERIOD && true)
            {

                // OPTION 2
                // Use option 1 time, even though we're not following it.
                traj_time = t_accel + t_decel; ;

                // Target acceleration to stop at x0.
                traj_accel = -copysign(a_to_v1_at_x0, v0);

                return;
            }
            else
            {

                // OPTION 1
                // Time to accelerate and decelerate.
                traj_time = t_accel + t_decel;
                // timeAcc = 0.6;
                // If the acceleration time would get the speed above v_max, then
                //  we need to add time to account for cruising at max speed.
                if (t_accel * a_max + Math.Abs(v0) > v_max)
                {
                    traj_time +=
                  Math.Pow(v_max - (a_max * t_accel + Math.Abs(v0)), 2.0) / (a_max * v_max);
                }

                // Accelerate (unless t_accel is less than FRAME_PERIOD, then set
                // acceleration to average acceleration over the period.)
                if (t_accel < period && t_decel == 0.0)
                {
                    traj_accel = copysign(a_max * a_factor, -x0);
                }
                else if (t_accel < period && t_decel > 0.0)
                {
                    traj_accel = compute_stop(v0, a_max * a_factor);
                }
                else if (t_accel < period)
                {
                    traj_accel = copysign((2.0 * t_accel / (period) - 1) * a_max * a_factor, v0);
                }
                else
                {
                    traj_accel = copysign(a_max * a_factor, -x0);
                }

            }
            if (dim == 1)
            {
                // CharterData.AddData("stateW", (double)(timeAcc));
                //   CharterData.AddData("aA", (double)(Astate));
            }
            //else if (dim == 2)
            //    CharterData.AddData("stateY", (double)(timeAcc));
        }

        private double bound(double x, int low, int high)
        {

            if (x < low) x = low;
            if (x > high) x = high;
            return (x);
        }

        private double anglemod(double a)
        {
            a -= Math.PI * 2 * (int)Math.Round(a / (Math.PI * 2));
            return (a);
        }

        private double anglemodDegree(double a)
        {
            a -= 360 * (int)Math.Round(a / (360));
            return (a);
        }
        private double hypot(double time_x, double time_y)
        {
            return Math.Sqrt(time_x * time_x + time_y * time_y);
        }

        private double copysign(double p, double p_2)
        {
            return Math.Abs(p) * Math.Sign(p_2);
        }

        public double compute_stop(double v, double max_a)
        {
            if (Math.Abs(v) > max_a * StaticVariables.FRAME_PERIOD) return copysign(max_a, -v);// Math.Abs(max_a) * Math.Sign(-v);
            else return -v / StaticVariables.FRAME_PERIOD;
        }

        private Vector2D PathTrajectorySpeed3(SingleObjectState Robot, List<SingleObjectState> path, Position2D tmpTarget, double v0, double v_max, double a_max, double pl, int idx)
        {

            double v_size = 0;
            if (path.Count < 3)
                return Vector2D.Zero;
            //for (int i = 0; i < path.Count - 1; i++)
            //{
            //    d += path[i].Location.DistanceFrom(path[i + 1].Location);
            //}

            double d_accel = 0, d_cruiz = 0, d_deccel = 0;
            double dc = pl - ((2 * v_max * v_max - v0 * v0) / (2 * a_max));

            if (dc < 0)
            {
                double v = Math.Sqrt(a_max * pl + v0 * v0 / 2);
                d_accel = (v * v - v0 * v0) / (2 * a_max);
                d_deccel = v * v / (2 * a_max);
                d_cruiz = 0;
            }
            else
            {
                d_accel = (v_max * v_max - v0 * v0) / (2 * a_max);
                d_deccel = v_max * v_max / (2 * a_max);
                d_cruiz = Math.Max(pl - d_accel - d_deccel, 0);
            }
            double dr = path[path.Count - 1].Location.DistanceFrom(tmpTarget);
            if (dr < d_accel)
            {
                v_size = Math.Sqrt(v0 * v0 + 2 * a_max * dr);
            }
            else if (dr < d_accel + d_cruiz)
            {
                v_size = v_max;
            }
            else
            {
                double tmpd = d_accel + d_cruiz + d_deccel - dr;
                v_size = Math.Sqrt(2 * a_max * tmpd);
            }
            //Vector2D v1 = tmpTarget - path[path.Count - 1].Location;
            //Vector2D v2 = path[idx].Location - tmpTarget; 
            //double alfa = v1.AngleInRadians;
            //double cos = Math.Cos(Math.Min(4 * Math.Abs(Vector2D.AngleBetweenInRadians(v1, v2)),Math.PI/2));

            //double cos = Math.Abs(Math.Cos(Vector2D.AngleBetweenInRadians(v1, v2)));

            //double min = 0.99;
            //double k = (Math.Max(cos, min) - min);
            //k /= (1 - min);
            //k *= 0.2;
            //k = 1;
            //return Vector2D.FromAngleSize(alfa, k*cos * v_size);

            int i = idx + 1;
            int count = 10;
            int endIdx = Math.Max(i - count, 0);
            double s = 0;
            double d0 = 0;
            for (int k = i + 1; k < path.Count - 1; k++)
            {
                d0 += path[k].Location.DistanceFrom(path[k + 1].Location);
            }
            for (int j = i; j > endIdx; j--)
            {
                Vector2D v1 = path[j].Location - path[j + 1].Location;
                Vector2D v2 = path[j - 1].Location - path[j].Location;
                double theta = Math.Abs(Vector2D.AngleBetweenInRadians(v1, v2));
                d0 += path[j].Location.DistanceFrom(path[j + 1].Location);
                s += (theta / d0 / 2);
            }
            Vector2D V = tmpTarget - path[path.Count - 1].Location;
            double alfa = V.AngleInRadians;
            double cos = Math.Abs(Math.Cos(s));
            return Vector2D.FromAngleSize(alfa, cos * v_size);

        }
        private Position2D UpdateTarget(List<SingleObjectState> path, double speed, ref int idx, ref double pathLength)
        {

            double distInterPol = 0.3, maxPathLength = 2;
            pathLength = 0;
            for (int j = path.Count - 2; j >= 0; j--)
            {
                pathLength += path[j].Location.DistanceFrom(path[j + 1].Location);
            }
            distInterPol = Math.Max(0.5, Math.Min(1, (pathLength / maxPathLength))) * distInterPol;
            double lambda = 1;
            double sumD = 0;
            Position2D tar = Position2D.Zero;
            int i = path.Count - 2;
            for (i = path.Count - 2; i >= 0; i--)
            {
                double d = path[i].Location.DistanceFrom(path[i + 1].Location);
                if (d + sumD <= distInterPol)
                    sumD += d;
                else
                    break;
            }
            i = Math.Max(i, 0);
            idx = Math.Max(i - 1, 0);

            return Position2D.Interpolate(path[path.Count - 2].Location, path[i].Location, lambda);
        }
    }

}
