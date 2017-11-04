using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Skills
{
    class ActiveRotateSkill : SkillBase
    {
        double targetAngle = 180;
        bool isFirst = true;
        int counter = 0;
        double W;
        public void Rotate(WorldModel Model, int RobotID, Position2D target, double passSpeed, bool isLeft)
        {
            if (isFirst)
            {
                counter = 0;
                isFirst = false;
            }
            NormalSharedState.ActiveInfo.isRotateStarted = true;
            SingleObjectState robot = Model.OurRobots[RobotID];
            SingleWirelessCommand SWC = new SingleWirelessCommand();
            double angst = (Model.OurRobots[RobotID].Angle.Value > 180) ? Model.OurRobots[RobotID].Angle.Value - 360 : Model.OurRobots[RobotID].Angle.Value;
            targetAngle = (target - robot.Location).AngleInDegrees;
            double deltaAng = targetAngle - angst;
            if (deltaAng > 180)
                deltaAng -= 360;
            else if (deltaAng < -180)
                deltaAng += 360;
            SWC.KickSpeed = 0;
            if (Math.Abs(deltaAng) < 0.1)
            {
                deltaAng = 0;
            }

            deltaAng *= Math.PI / 180.0;
            double tAng = 0, angAcc = 0;
            double factor = 0.0;

            factor = 2.3;
            compute_motion_1d(1, 2, -deltaAng, robot.AngularSpeed.Value, 0, 40 * factor, 20, 1, ref angAcc, ref tAng);

            W = angAcc / 60;
            SWC.RobotID = RobotID;

            DrawingObjects.AddObject(new StringDraw("W: " + W.ToString(), new Position2D(3, 2)), "sdasdGHKIO");
            DrawingObjects.AddObject(new StringDraw("delta: " + deltaAng.ToString(), new Position2D(3.2, 2)), "sdasdGghfgHKIO");

            if (Math.Abs(deltaAng) < 0.1)
            {
                counter++;
                SWC.isChipKick = true;
                SWC.KickSpeed = passSpeed;
                SWC.SpinBack = 0;
                if (counter > 4)
                    NormalSharedState.ActiveInfo.isRotateStarted = false;
            }
            else
            {
                SWC.KickPower = 0;
                SWC.SpinBack = 100;
            }
            SWC.W = isLeft ? -Math.Abs(W) : Math.Abs(W);
            SWC.Vy = isLeft ? SWC.W * 0.2 : -SWC.W * 0.2;
            SWC.Vx = 0;

            Planner.Add(RobotID, SWC);
        }

        public void compute_motion_1d(int dim, double accuercyCoef, double x0, double v0, double v1,
                               double a_max, double v_max, double a_factor,
                               ref double traj_accel, ref double traj_time)
        {
            double Astate = 0;
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
                Astate = -2;
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

                Astate = -1;
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
                    Astate = 0;
                    traj_accel = copysign(a_max * a_factor, -x0);
                }
                else if (t_accel < period && t_decel > 0.0)
                {
                    Astate = 1;
                    traj_accel = compute_stop(v0, a_max * a_factor);
                }
                else if (t_accel < period)
                {
                    Astate = 2;
                    traj_accel = copysign((2.0 * t_accel / (period) - 1) * a_max * a_factor, v0);
                }
                else
                {
                    Astate = 3;
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
    }
}
