using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills
{
    class RotateWithBallSkill : SkillBase
    {
        SingleWirelessCommand swc1 = new SingleWirelessCommand();
        int counterAngle = 0;
        private double lastErr = 0;
        private double iErr = 0;
        public bool RotateWithBall(WorldModel Model, int RobotID, double targetAngle, double radious, double Kfront)
        {
            bool res = false;
            bool clockWise = true;
            double ww = 0;
            double angst = (Model.OurRobots[RobotID].Angle.Value > 180) ? Model.OurRobots[RobotID].Angle.Value - 360 : Model.OurRobots[RobotID].Angle.Value;
            double deltaAng = targetAngle - angst;
            if (clockWise)
            {

            }
            if (deltaAng > 180)
                deltaAng -= 360;
            else if (deltaAng < -180)
                deltaAng += 360;

            if (Math.Abs(deltaAng) < 0.3)
            {
                deltaAng = 0;
                // aTunner.Check4CollisionReset(PIDType.W);
            }

            if (Math.Abs(deltaAng) < 1)
            {
                // aTunner.Check4CollisionReset(PIDType.W);
                res = true;
            }

            //if (Math.Abs(deltaAng) < 8)
            //{
            //    swc1.SpinBack = 0;
            //    swc1.KickPower = 200;
            //}

            deltaAng *= Math.PI / 180.0;
            if (Math.Abs(deltaAng) > 0.1)
            {
                //angst = Model.OurRobots[RobotID].Angle.Value;// (Model.OurRobots[RobotID].Angle.Value > 180) ? Model.OurRobots[RobotID].Angle.Value - 360 : Model.OurRobots[RobotID].Angle.Value;

                //if (clockWise)
                //    deltaAng = TargetAngle - angst;
                //else
                //    deltaAng = 360 - (TargetAngle - angst);

                double tAng = 0, angAcc = 0;
                double factor = 1.0;
                double Waccuercy = 2;
                double Wafactor = 1;
                double _maxAccelAngular = 8;
                double _maxAngularSpeed = 4;
                //while (counter1 < 10)
                //{
                // factor += 0.1;
                compute_motion_1d(1, Waccuercy, -deltaAng, Model.OurRobots[RobotID].AngularSpeed.Value, 0, _maxAccelAngular * factor, _maxAngularSpeed, Wafactor, ref angAcc, ref tAng);
                //if (tAng < trajTime)
                //    break;
                //    counter1++;
                //}
                int _framCount = 60;
                double finalw = angAcc / _framCount;

                ww = Model.OurRobots[RobotID].AngularSpeed.Value + finalw;

                if (Math.Abs(ww) > _maxAngularSpeed)
                    ww = Math.Sign(ww) * Math.Abs(_maxAngularSpeed);

                ww = -ww;
            }
            else
            {
                double wout = AngularController(Model, RobotID, targetAngle);
                double alfamax = 20;
                double alfa = (wout - (-Model.lastW[RobotID])) * StaticVariables.FRAME_RATE;
                ww = wout;
                if (Math.Abs(alfa) > alfamax)
                    ww = (-Model.lastW[RobotID]) + Math.Sign(alfa) * alfamax * StaticVariables.FRAME_PERIOD;
            }


            swc1.RobotID = RobotID;
            swc1.Vx = radious * Math.Abs(ww);
            swc1.Vy = Kfront * Math.Abs(ww);
            swc1.W = ww;//Math.Min(maxAngularVelocity, swc.W);

            CharterData.AddData("W", Color.Blue, swc1.W);
            CharterData.AddData("Vy", Color.Red, swc1.Vy);
            CharterData.AddData("Vx", Color.Black, swc1.Vx);

            Planner.Add(RobotID, swc1);
            return res;
        }

        private double AngularController(WorldModel Model, int RobotID, double angle)
        {
            double Kp = 10, Ki = 0/*0.05*/, Kd = 0/*0.32*/, lamda = 0.99, PID_Max = 40;
            //double Kp = 7, Ki = 0, Kd = 0, lamda = 0.99, PID_Max = 40;
            double MaxIntegral = 100;
            double err = (angle - Model.OurRobots[RobotID].Angle.Value) * Math.PI / 180;

            if (err > Math.PI)
                err -= 2 * Math.PI;
            else if (err < -Math.PI)
                err += 2 * Math.PI;
            if (counterAngle == 0)
            {
                lastErr = err;
                iErr = 0;
            }
            counterAngle++;
            double dErr = (err - lastErr) / StaticVariables.FRAME_PERIOD;
            iErr = iErr * lamda + err * StaticVariables.FRAME_PERIOD;
            lastErr = err;
            if (iErr > MaxIntegral)
                iErr = MaxIntegral;
            else if (iErr < -MaxIntegral)
                iErr = -MaxIntegral;

            //CharterData.AddData("erromega", err);
            double outPut = Kp * err + Ki * iErr + Kd * dErr;
            if (outPut < -PID_Max)
                outPut = -PID_Max;
            if (outPut > PID_Max)
                outPut = PID_Max;
            return -outPut;
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

        private double copysign(double p, double p_2)
        {
            return Math.Abs(p) * Math.Sign(p_2);
        }

        public double compute_stop(double v, double max_a)
        {
            if (Math.Abs(v) > max_a * StaticVariables.FRAME_PERIOD) return copysign(max_a, -v);// Math.Abs(max_a) * Math.Sign(-v);
            else return -v / StaticVariables.FRAME_PERIOD;
        }

        public void Reset()
        {
            swc1 = new SingleWirelessCommand();
            counterAngle = 0;
            lastErr = 0;
            iErr = 0;
        }
    }
}
