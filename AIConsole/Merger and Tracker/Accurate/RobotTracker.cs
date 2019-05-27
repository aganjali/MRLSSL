#region old
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.GameDefinitions;

//namespace MRL.SSL.AIConsole.New_MergerAndTracker
//{
//    public enum RobotType
//    {
//        Default
//    }
//    public class RobotTracker : Kalman
//    {

//        private RobotType type;
//        private double latency;

//        private bool reset_on_obs;

//        private struct rcommand
//        {
//            public rcommand(double _t, Vector3D _vs)
//            {
//                timestamp = _t;
//                vs = _vs;
//            }
//            public double timestamp;
//            public Vector3D vs;
//        };

//        Queue<rcommand> cs; // Velocity commands
//        private static bool cr_setup = false;

//        public RobotTracker(RobotType Type, double _latency )
//        : base(7, 3, StaticVariables.FRAME_PERIOD)
//        {
//            type = Type;
//            cs = new Queue<rcommand>();
//            latency = _latency;
//            reset_on_obs = true;

//            if (StaticVariables.ROBOT_PRINT_KALMAN_ERROR > 0)
//                prediction_lookahead = StaticVariables.LATENCY_DELAY;
//        }

//        private rcommand get_command(double time)
//        {
//            if (cs.Count == 0 || cs.ElementAt(0).timestamp > time)
//                return new rcommand(0, new Vector3D(0, 0, 0));
//            int i;
//            for (i = 1; i < cs.Count; i++)
//            {
//                if (cs.ElementAt(i).timestamp > time) break;
//            }
//            return cs.ElementAt(i - 1);
//        }
//        public void command(double timestamp, Vector3D vs)
//        {
//            rcommand c = new rcommand(timestamp + latency - (StaticVariables.FRAME_PERIOD / 2.0), vs);

//            while (cs.Count > 1 && cs.ElementAt(0).timestamp < time - stepsize)
//            {
//                cs.Dequeue();
//            }
//            cs = ReverseQ(cs);
//            while (cs.Count != 0 && cs.First().timestamp == c.timestamp)
//            {
//                cs.Dequeue();
//            }
//            cs = ReverseQ(cs);
//            cs.Enqueue(c);
//        }
//        Queue<rcommand> ReverseQ(Queue<rcommand> q)
//        {
//            Queue<rcommand> q2 = new Queue<rcommand>();
//            if (q == null)
//                return null;
//            for (int i = q.Count - 1; i >= 0; i--)
//            {
//                q2.Enqueue(q.ElementAt(i));
//            }
//            return q2;
//        }
//        FMatrix _x = new FMatrix(7, 1), _P = FMatrix.IdentityMatrix(7, 7);
//        public void observe(vraw obs, double timestamp)
//        {

//            if (timestamp - time > StaticVariables.MaxPredictTime)
//                reset();
//            if (reset_on_obs)
//            {
//                if (obs.conf <= 0.0) return;
//                _x[0, 0] = obs.pos.X;
//                _x[1, 0] = obs.pos.Y;
//                _x[2, 0] = obs.angle;
//                _x[3, 0] = 0.0;
//                _x[4, 0] = 0.0;
//                _x[5, 0] = 0.0;
//                _x[6, 0] = 0.0;

//                _P[0, 0] = StaticVariables.ROBOT_POSITION_VARIANCE;
//                _P[1, 1] = StaticVariables.ROBOT_POSITION_VARIANCE;
//                _P[2, 2] = StaticVariables.ROBOT_THETA_VARIANCE;
//                _P[3, 3] = 0.0; // 0m/s
//                _P[4, 4] = 0.0; // 0m/s
//                _P[5, 5] = 0.0;
//                _P[6, 6] = 0.0;

//                initial(obs.timestamp, ref _x, ref _P);

//                reset_on_obs = false;

//            }
//            else
//            {
//                // If this is a new observation.
//                if (timestamp > time)
//                {
//                    // Tick to current time.

//                    tick(timestamp - time);


//                    // Make observation
//                    if (obs.timestamp == timestamp)
//                    {

//                        double xtheta = xs.First()[2, 0];

//                        FMatrix o = new FMatrix(3, 1);
//                        o[0, 0] = obs.pos.X;
//                        o[1, 0] = obs.pos.Y;
//                        o[2, 0] = util.anglemod(obs.angle - xtheta) + xtheta;
//                        //HiPerfTimer ht = new HiPerfTimer();
//                        //ht.Start();
//                        update(o);
//                        //ht.Stop();
//                        //Console.WriteLine(ht.Duration * 1000);
//                    }

//                    if (error_time_elapsed() > 10.0)
//                        error_reset();

//                }
//            }
//        }
//        public void reset(double timestamp, double[] state)
//        {
//            FMatrix x = new FMatrix(7, 1), P = FMatrix.IdentityMatrix(7, 7);
//            for (int i = 0; i < 7; i++)
//            {
//                x[i, 0] = state[i];
//            }
//            P[0, 0] = StaticVariables.ROBOT_POSITION_VARIANCE;
//            P[1, 1] = StaticVariables.ROBOT_POSITION_VARIANCE;
//            P[2, 2] = StaticVariables.ROBOT_THETA_VARIANCE;
//            P[3, 3] = 0.0; // 0m/s
//            P[4, 4] = 0.0; // 0m/s
//            P[5, 5] = 0.0;
//            P[6, 6] = 0.0;

//            initial(timestamp, ref x, ref P);
//            reset_on_obs = false;
//        }
//        public Position2D position(double time)
//        {
//            if (StaticVariables.ROBOT_FAST_PREDICT > 0)
//            {
//                if (time > latency)
//                {
//                    FMatrix x = predict(latency);
//                    return (new Position2D(x[0, 0], x[1, 0]) +
//                        new Vector2D(x[3, 0], x[4, 0]) * (time - latency));
//                }
//                else
//                {
//                    FMatrix x = predict(time);
//                    return new Position2D(x[0, 0], x[1, 0]);
//                }
//            }
//            else
//            {
//                FMatrix x = predict(time);
//                return new Position2D(x[0, 0], x[1, 0]);
//            }
//        }
//        public Vector2D velocity(double time)
//        {
//            FMatrix x;

//            if (StaticVariables.ROBOT_FAST_PREDICT > 0)
//            {
//                if (time > latency)
//                {
//                    x = predict(latency);
//                }
//                else
//                {
//                    x = predict(time);
//                }
//            }
//            else
//                x = predict(time);
//            double a = x[2, 0];
//            double c = Math.Cos(a);
//            double s = Math.Sin(a);

//            double vx = x[3, 0];
//            double vy = x[4, 0];

//            double stuck = x[6, 0];

//            if (stuck > StaticVariables.ROBOT_STUCK_THRESHOLD)
//                return Vector2D.Zero;

//            return new Vector2D(c * vx - s * vy, s * vx + c * vy);
//        }
//        // return the velocity un-rotate
//        public Vector2D velocity_raw(double time)
//        {
//            FMatrix x;
//            if (StaticVariables.ROBOT_FAST_PREDICT > 0)
//            {
//                if (time > latency)
//                {
//                    x = predict(latency);
//                }
//                else
//                {
//                    x = predict(time);
//                }
//            }
//            else
//                x = predict(time);

//            double vx = x[3, 0];
//            double vy = x[4, 0];

//            double stuck = x[6, 0];

//            if (stuck > 0.8)
//                return Vector2D.Zero;

//            return new Vector2D(vx, vy);
//        }
//        public double direction(double time)
//        {
//            FMatrix x = predict(time);
//            return x[2, 0];
//        }
//        public double angular_velocity(double time)
//        {
//            FMatrix x = predict(time);
//            return x[5, 0];
//        }
//        public double stuck(double time)
//        {
//            FMatrix x = predict(time);
//            return util.bound(x[6, 0], 0, 1);
//        }
//        public void set_type(RobotType _type)
//        {
//            type = _type;
//            reset();
//        }
//        public void reset()
//        {
//            reset_on_obs = true;
//        }


//         FMatrix _f;
//        public override FMatrix f(FMatrix x, FMatrix I)
//        {
//            I = new FMatrix(0, 0);
//            _f = new FMatrix(x);
//            rcommand c = get_command(stepped_time);

//            double
//              _x = _f[0, 0],
//              _y = _f[1, 0],
//              _theta = _f[2, 0],
//              _vpar = _f[3, 0],
//              _vperp = _f[4, 0],
//              _vtheta = _f[5, 0],
//              _stuck = _f[6, 0];

//            _stuck = util.bound(_stuck, 0, 1) * StaticVariables.ROBOT_STUCK_DECAY;

//            double avg_vpar = 0, avg_vperp = 0, avg_vtheta = 0, avg_theta = 0;
//            double avg_weight = 0.5;

//            if (StaticVariables.ROBOT_USE_AVERAGES_IN_PROPAGATION > 0)
//            {
//                avg_vpar = avg_weight * _vpar;
//                avg_vperp = avg_weight * _vperp;
//                avg_vtheta = avg_weight * _vtheta;
//            }

//            if (type == RobotType.Default)
//            {
//                _vpar = c.vs.X;
//                _vperp = c.vs.Y;
//                _vtheta = c.vs.Z;
//            }

//            if (StaticVariables.ROBOT_USE_AVERAGES_IN_PROPAGATION > 0)
//            {
//                avg_vpar += (1.0 - avg_weight) * _vpar;
//                avg_vperp += (1.0 - avg_weight) * _vperp;
//                avg_vtheta += (1.0 - avg_weight) * _vtheta;

//                avg_theta = avg_weight * _theta;

//                _theta += (1.0 - _stuck) * stepsize * avg_vtheta;

//                avg_theta += (1.0 - avg_weight) * _theta;

//                _x += (1.0 - _stuck) * stepsize *
//                  (avg_vpar * Math.Cos(avg_theta) + avg_vperp * -Math.Sin(avg_theta));
//                _y += (1.0 - _stuck) * stepsize *
//                  (avg_vpar * Math.Sin(avg_theta) + avg_vperp * Math.Cos(avg_theta));
//            }
//            else
//            {
//                _theta += (1.0 - _stuck) * stepsize * _vtheta;
//                _x += (1.0 - _stuck) * stepsize *
//                  (_vpar * Math.Cos(_theta) + _vperp * -Math.Sin(_theta));
//                _y += (1.0 - _stuck) * stepsize *
//                  (_vpar * Math.Sin(_theta) + _vperp * Math.Cos(_theta));
//            }

//            _theta = util.anglemod(_theta);
//            _f[0, 0] = _x;
//            _f[1, 0] = _y;
//            _f[2, 0] = _theta;
//            _f[3, 0] = _vpar;
//            _f[4, 0] = _vperp;
//            _f[5, 0] = _vtheta;
//            _f[6, 0] = _stuck;
//            return _f;
//        }
//         FMatrix _h = new FMatrix(3, 1);
//        public override FMatrix h(FMatrix x)
//        {

//            _h[0, 0] = x[0, 0];
//            _h[1, 0] = x[1, 0];
//            _h[2, 0] = x[2, 0];

//            return _h;
//        }
//         FMatrix _Q = FMatrix.IdentityMatrix(4, 4);
//        public override FMatrix Q(FMatrix x)
//        {

//            _Q[0, 0] = StaticVariables.ROBOT_OMNI_VELOCITY_VARIANCE;
//            _Q[1, 1] = StaticVariables.ROBOT_OMNI_VELOCITY_VARIANCE;
//            _Q[2, 2] = StaticVariables.ROBOT_OMNI_ANGVEL_VARIANCE;
//            _Q[3, 3] = StaticVariables.ROBOT_STUCK_VARIANCE;


//            return _Q;
//        }
//         FMatrix _R;
//        public override FMatrix R(FMatrix x)
//        {

//            if (_R == null || _R.rows == 0)
//            {
//                _R = FMatrix.IdentityMatrix(3, 3);
//                _R[0, 0] = StaticVariables.ROBOT_POSITION_VARIANCE;
//                _R[1, 1] = StaticVariables.ROBOT_POSITION_VARIANCE;
//                _R[2, 2] = StaticVariables.ROBOT_THETA_VARIANCE;
//            }

//            return _R;
//        }

//         FMatrix _A = FMatrix.IdentityMatrix(7, 7);
//        public override FMatrix A(FMatrix x)
//        {

//            double theta = x[2, 0];
//            double vpar = x[3, 0], vperp = x[4, 0], vtheta = x[5, 0];
//            double stuck = x[6, 0];
//            double cos_theta = Math.Cos(theta), sin_theta = Math.Sin(theta);

//            _A[0, 2] = (1.0 - stuck) * stepsize *
//              (vpar * -sin_theta + vperp * -cos_theta);
//            _A[0, 3] = cos_theta * (1.0 - stuck) * stepsize;
//            _A[0, 4] = -sin_theta * (1.0 - stuck) * stepsize;
//            _A[0, 6] = -stepsize * (vpar * cos_theta + vperp * -sin_theta);
//            _A[1, 2] = (1.0 - stuck) * stepsize *
//              (vpar * cos_theta + vperp * -sin_theta);
//            _A[1, 3] = sin_theta * (1.0 - stuck) * stepsize;
//            _A[1, 4] = cos_theta * (1.0 - stuck) * stepsize;
//            _A[1, 6] = -stepsize * (vpar * sin_theta + vperp * cos_theta);
//            _A[2, 5] = (1.0 - stuck) * stepsize;
//            _A[2, 6] = -stepsize * vtheta;
//            _A[3, 3] = StaticVariables.ROBOT_VELOCITY_NEXT_STEP_COVARIANCE;
//            _A[4, 4] = StaticVariables.ROBOT_VELOCITY_NEXT_STEP_COVARIANCE;
//            _A[5, 5] = StaticVariables.ROBOT_VELOCITY_NEXT_STEP_COVARIANCE;
//            _A[6, 6] = StaticVariables.ROBOT_STUCK_DECAY;

//            return _A;
//        }

//        FMatrix _W = new FMatrix(new double[,] { { 0, 0, 0, 0 }, 
//                               { 0, 0, 0, 0 }, 
//                               { 0, 0, 0, 0 }, 
//                               { 1, 0, 0, 0 }, 
//                               { 0, 1, 0, 0 }, 
//                               { 0, 0, 1, 0 }, 
//                               { 0, 0, 0, 1 } });
//        public override FMatrix W(FMatrix x)
//        {
//            return _W;
//        }
//         FMatrix _H = new FMatrix(new double[,]{ { 1, 0, 0, 0, 0, 0, 0 }, 
//                               { 0, 1, 0, 0, 0, 0, 0 }, 
//                               { 0, 0, 1, 0, 0, 0, 0 } });
//        public override FMatrix H(FMatrix x)
//        {
//            return _H;
//        }

//        FMatrix _V = new FMatrix(new double[,]{ { 1, 0, 0 }, 
//                               { 0, 1, 0 }, 
//                               { 0, 0, 1 } });

//        public override FMatrix V(FMatrix x)
//        {
//            return _V;
//        }

//    }
//} 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using Meta.Numerics.Matrices;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public enum RobotType
    {
        Default
    }
    public class RobotTracker : Kalman
    {

        private RobotType type;
        private double latency;
        private bool reset_on_obs;
        private struct rcommand
        {
            public rcommand(double _t, Vector3D _vs)
            {
                timestamp = _t;
                vs = _vs;
            }
            public double timestamp;
            public Vector3D vs;
        };

        Queue<rcommand> cs; // Velocity commands
        private static bool cr_setup = false;

        public RobotTracker(RobotType Type, double _latency)
            : base(7, 3, StaticVariables.FRAME_PERIOD)
        {
            type = Type;
            cs = new Queue<rcommand>();
            latency = _latency;
            reset_on_obs = true;

            if (StaticVariables.ROBOT_PRINT_KALMAN_ERROR > 0)
                prediction_lookahead = StaticVariables.LATENCY_DELAY;
            RectangularMatrix _W = W(new RectangularMatrix(0, 0));
            RectangularMatrix _Q = Q(new RectangularMatrix(0, 0));
            tmpC = _W * _Q * _W.Transpose();

            RectangularMatrix __V = V(new RectangularMatrix(0, 0));
            RectangularMatrix __R = R(new RectangularMatrix(0, 0));
            tmpCV = __V * __R * __V.Transpose();
        }

        private rcommand get_command(double time)
        {
            if (cs.Count == 0 || cs.ElementAt(0).timestamp > time)
                return new rcommand(0, new Vector3D(0, 0, 0));
            int i;
            for (i = 1; i < cs.Count; i++)
            {
                if (cs.ElementAt(i).timestamp > time) break;
            }
            return cs.ElementAt(i - 1);//Export Last command in time
        }
        public void command(double timestamp, Vector3D vs)
        {
            rcommand c = new rcommand(timestamp + latency - (StaticVariables.FRAME_PERIOD / 2.0), vs);

            while (cs.Count > 1 && cs.ElementAt(0).timestamp < time - stepsize)
            {
                cs.Dequeue();
            }
            cs = ReverseQ(cs);
            while (cs.Count != 0 && cs.First().timestamp == c.timestamp)
            {
                cs.Dequeue();
            }
            cs = ReverseQ(cs);
            cs.Enqueue(c);
            //DrawingObjects.AddObject(new StringDraw("cs.count: " + cs.Count, new Position2D(0.5, 0)));
        }
        Queue<rcommand> ReverseQ(Queue<rcommand> q)
        {
            Queue<rcommand> q2 = new Queue<rcommand>();
            if (q == null)
                return null;
            for (int i = q.Count - 1; i >= 0; i--)
            {
                q2.Enqueue(q.ElementAt(i));
            }
            return q2;
        }
        RectangularMatrix _x = new RectangularMatrix(7, 1),
                          _P = new RectangularMatrix(7, 7, true);
        public void observe(bool visionProblem, vraw obs, double timestamp)
        {
            bool b = xs.Count > 0 && xs.First().RowCount > 2 ;
            RectangularMatrix fx =(b)? xs.First():null;
            if (Math.Abs(timestamp - time) > StaticVariables.MaxPredictTime || (fx != null && (double.IsNaN(fx[0, 0]) || double.IsNaN(fx[1, 0]) || double.IsNaN(fx[2, 0]))))
                reset() ;
            if (reset_on_obs)
            {
                if (obs.conf <= 0.0) return;
                _x[0, 0] = obs.pos.X;
                _x[1, 0] = obs.pos.Y;
                _x[2, 0] = obs.angle - Math.PI / 2;
                _x[3, 0] = 0.0;
                _x[4, 0] = 0.0;
                _x[5, 0] = 0.0;
                _x[6, 0] = 0.0;


                _P[0, 0] = StaticVariables.ROBOT_POSITION_VARIANCE;
                _P[1, 1] = StaticVariables.ROBOT_POSITION_VARIANCE;
                _P[2, 2] = StaticVariables.ROBOT_THETA_VARIANCE;
                _P[3, 3] = 0.0; // 0m/s
                _P[4, 4] = 0.0; // 0m/s
                _P[5, 5] = 0.0;
                _P[6, 6] = 0.0;

                initial(obs.timestamp, ref _x, ref _P);

                reset_on_obs = false;

            }
            else
            {
                // If this is a new observation.
                if (timestamp > time)
                {
                    // Tick to current time.

                    tick(visionProblem, timestamp - time);


                    // Make observation
                    if (obs.timestamp == timestamp)
                    {

                        double xtheta = xs.First()[2, 0];

                        RectangularMatrix o = new RectangularMatrix(3, 1);
                        o[0, 0] = obs.pos.X;
                        o[1, 0] = obs.pos.Y;
                        o[2, 0] = util.anglemod((obs.angle - Math.PI / 2) - xtheta) + xtheta;
                        //HiPerfTimer ht = new HiPerfTimer();
                        //ht.Start();
                        update(visionProblem, o);
                        //ht.Stop();
                        //Console.WriteLine(ht.Duration * 1000);
                    }

                    if (error_time_elapsed() > 10.0)
                        error_reset();

                }
            }
        }
        public void reset(double timestamp, double[] state)
        {
            RectangularMatrix x = new RectangularMatrix(7, 1), P = new RectangularMatrix(7, 7, true);
            for (int i = 0; i < 7; i++)
            {
                x[i, 0] = state[i];
            }
            P[0, 0] = StaticVariables.ROBOT_POSITION_VARIANCE;
            P[1, 1] = StaticVariables.ROBOT_POSITION_VARIANCE;
            P[2, 2] = StaticVariables.ROBOT_THETA_VARIANCE;
            P[3, 3] = 0.0; // 0m/s
            P[4, 4] = 0.0; // 0m/s
            P[5, 5] = 0.0;
            P[6, 6] = 0.0;

            initial(timestamp, ref x, ref P);
            reset_on_obs = false;
        }
        public Position2D position(bool visionProblem, double time)
        {
            if (StaticVariables.ROBOT_FAST_PREDICT > 0)
            {
                if (time > latency)
                {
                    RectangularMatrix x = predict(visionProblem,latency);
                    return (new Position2D(x[0, 0], x[1, 0]) +
                        new Vector2D(x[3, 0], x[4, 0]) * (time - latency));
                }
                else
                {
                    RectangularMatrix x = predict(visionProblem,time);
                    return new Position2D(x[0, 0], x[1, 0]);
                }
            }
            else
            {
                RectangularMatrix x = predict(visionProblem,time);
 
                return new Position2D(x[0, 0], x[1, 0]);
            }
        }
        public Vector2D velocity(bool visionProblem, double time)
        {
            RectangularMatrix x;

            if (StaticVariables.ROBOT_FAST_PREDICT > 0)
            {
                if (time > latency)
                {
                    x = predict(visionProblem,latency);
                }
                else
                {
                    x = predict(visionProblem,time);
                }
            }
            else
                x = predict(visionProblem,time);
            double a = x[2, 0];
            double c = Math.Cos(a);
            double s = Math.Sin(a);

            double vx = x[3, 0];
            double vy = x[4, 0];

            double stuck = x[6, 0];

            if (stuck > StaticVariables.ROBOT_STUCK_THRESHOLD)
                return Vector2D.Zero;

            return new Vector2D(c * vx - s * vy, s * vx + c * vy);
        }
        // return the velocity un-rotate
        public Vector2D velocity_raw(bool visionProblem, double time)
        {
            RectangularMatrix x;
            if (StaticVariables.ROBOT_FAST_PREDICT > 0)
            {
                if (time > latency)
                {
                    x = predict(visionProblem,latency);
                }
                else
                {
                    x = predict(visionProblem,time);
                }
            }
            else
                x = predict(visionProblem,time);

            double vx = x[3, 0];
            double vy = x[4, 0];

            double stuck = x[6, 0];

            //if (stuck > 0.8)
            //    return Vector2D.Zero;

            return new Vector2D(vx, vy);
        }
        public double direction(bool visionProblem, double time)
        {
            RectangularMatrix x = predict(visionProblem,time);
            return x[2, 0];
        }
        public double angular_velocity(bool visionProblem, double time)
        {
            RectangularMatrix x = predict(visionProblem, time);
            return x[5, 0];
        }
        public double stuck(bool visionProblem, double time)
        {
            RectangularMatrix x = predict(visionProblem,time);
            return util.bound(x[6, 0], 0, 1);
        }
        public void set_type(RobotType _type)
        {
            type = _type;
            reset();
        }
        public void reset()
        {
            reset_on_obs = true;
        }


        RectangularMatrix _f;
        public override RectangularMatrix f(bool visionProblem, RectangularMatrix x, ref RectangularMatrix I, bool checkCollision = true)
        {
            I = new RectangularMatrix(0, 0);
            // _f = new MathMatrix(x);
            rcommand c = get_command(stepped_time);

            //double
            //  _x = _f[0, 0],
            //  _y = _f[1, 0],
            //  _theta = _f[2, 0],
            //  _vpar = _f[3, 0],
            //  _vperp = _f[4, 0],
            //  _vtheta = _f[5, 0],
            //  _stuck = _f[6, 0];
         //   DrawingObjects.AddObject(new StringDraw(
            x[6, 0] = util.bound(x[6, 0], 0, 1) * ((visionProblem) ? 1 : StaticVariables.ROBOT_STUCK_DECAY);

            double avg_vpar = 0, avg_vperp = 0, avg_vtheta = 0, avg_theta = 0;
            double avg_weight = 0.5;

            if (StaticVariables.ROBOT_USE_AVERAGES_IN_PROPAGATION > 0)
            {
                avg_vpar = avg_weight * x[3, 0];
                avg_vperp = avg_weight * x[4, 0];
                avg_vtheta = avg_weight * x[5, 0];
            }

            //if (type == RobotType.Default)
            //{
            x[3, 0] = c.vs.X;
            x[4, 0] = c.vs.Y;
            x[5, 0] = c.vs.Z;
            //}

            if (StaticVariables.ROBOT_USE_AVERAGES_IN_PROPAGATION > 0)
            {
                avg_vpar += (1.0 - avg_weight) * x[3, 0];
                avg_vperp += (1.0 - avg_weight) * x[4, 0];
                avg_vtheta += (1.0 - avg_weight) * x[5, 0];

                avg_theta = avg_weight * x[2, 0];

                x[2, 0] += (1.0 - x[6, 0]) * stepsize * avg_vtheta;

                avg_theta += (1.0 - avg_weight) * x[2, 0];

                x[0, 0] += (1.0 - x[6, 0]) * stepsize *
                  (avg_vpar * Math.Cos(avg_theta) + avg_vperp * -Math.Sin(avg_theta));
                x[1, 0] += (1.0 - x[6, 0]) * stepsize *
                  (avg_vpar * Math.Sin(avg_theta) + avg_vperp * Math.Cos(avg_theta));
            }
            else
            {
                x[2, 0] += (1.0 - x[6, 0]) * stepsize * x[5, 0];
                x[0, 0] += (1.0 - x[6, 0]) * stepsize *
                  (x[3, 0] * Math.Cos(x[2, 0]) + x[4, 0] * -Math.Sin(x[2, 0]));
                x[1, 0] += (1.0 - x[6, 0]) * stepsize *
                  (x[3, 0] * Math.Sin(x[2, 0]) + x[4, 0] * Math.Cos(x[2, 0]));
            }

            x[2, 0] = util.anglemod(x[2, 0]);
            //_f[0, 0] = _x;
            //_f[1, 0] = _y;
            //_f[2, 0] = _theta;
            //_f[3, 0] = _vpar;
            //_f[4, 0] = _vperp;
            //_f[5, 0] = _vtheta;
            //_f[6, 0] = _stuck;
            return x;
            //   I = new MathMatrix(0, 0);
            //  // _f = new MathMatrix(x);
            //   rcommand c = get_command(stepped_time);

            //   double
            //     _x = _f[0, 0],
            //     _y = _f[1, 0],
            //     _theta = _f[2, 0],
            //     _vpar = _f[3, 0],
            //     _vperp = _f[4, 0],
            //     _vtheta = _f[5, 0],
            //     _stuck = _f[6, 0];

            //   _stuck = util.bound(_stuck, 0, 1) * StaticVariables.ROBOT_STUCK_DECAY;

            ////   double avg_vpar = 0, avg_vperp = 0, avg_vtheta = 0, avg_theta = 0;
            //   //double avg_weight = 0.5;

            //   //if (StaticVariables.ROBOT_USE_AVERAGES_IN_PROPAGATION > 0)
            //   //{
            //   //    avg_vpar = avg_weight * _vpar;
            //   //    avg_vperp = avg_weight * _vperp;
            //   //    avg_vtheta = avg_weight * _vtheta;
            //   //}

            //   //if (type == RobotType.Default)
            //   //{
            //       _vpar = c.vs.X;
            //       _vperp = c.vs.Y;
            //       _vtheta = c.vs.Z;
            //   //}

            //   //if (StaticVariables.ROBOT_USE_AVERAGES_IN_PROPAGATION > 0)
            //   //{
            //   //    avg_vpar += (1.0 - avg_weight) * _vpar;
            //   //    avg_vperp += (1.0 - avg_weight) * _vperp;
            //   //    avg_vtheta += (1.0 - avg_weight) * _vtheta;

            //   //    avg_theta = avg_weight * _theta;

            //   //    _theta += (1.0 - _stuck) * stepsize * avg_vtheta;

            //   //    avg_theta += (1.0 - avg_weight) * _theta;

            //   //    _x += (1.0 - _stuck) * stepsize *
            //   //      (avg_vpar * Math.Cos(avg_theta) + avg_vperp * -Math.Sin(avg_theta));
            //   //    _y += (1.0 - _stuck) * stepsize *
            //   //      (avg_vpar * Math.Sin(avg_theta) + avg_vperp * Math.Cos(avg_theta));
            //   //}
            //   //else
            //   //{
            //       _theta += (1.0 - _stuck) * stepsize * _vtheta;
            //       _x += (1.0 - _stuck) * stepsize *
            //         (_vpar * Math.Cos(_theta) + _vperp * -Math.Sin(_theta));
            //       _y += (1.0 - _stuck) * stepsize *
            //         (_vpar * Math.Sin(_theta) + _vperp * Math.Cos(_theta));
            //   //}

            //   _theta = util.anglemod(_theta);
            //   _f[0, 0] = _x;
            //   _f[1, 0] = _y;
            //   _f[2, 0] = _theta;
            //   _f[3, 0] = _vpar;
            //   _f[4, 0] = _vperp;
            //   _f[5, 0] = _vtheta;
            //   _f[6, 0] = _stuck;
            //   return _f;
        }
        RectangularMatrix _h = new RectangularMatrix(3, 1);
        public override RectangularMatrix h(RectangularMatrix x)
        {

            _h[0, 0] = x[0, 0];
            _h[1, 0] = x[1, 0];
            _h[2, 0] = x[2, 0];

            return _h;
        }
        RectangularMatrix _Q = new RectangularMatrix(4, 4, true);
        public override RectangularMatrix Q(RectangularMatrix x)
        {

            _Q[0, 0] = StaticVariables.ROBOT_OMNI_VELOCITY_VARIANCE;
            _Q[1, 1] = StaticVariables.ROBOT_OMNI_VELOCITY_VARIANCE;
            _Q[2, 2] = StaticVariables.ROBOT_OMNI_ANGVEL_VARIANCE;
            _Q[3, 3] = StaticVariables.ROBOT_STUCK_VARIANCE;


            return _Q;
        }
        RectangularMatrix _R;
        public override RectangularMatrix R(RectangularMatrix x)
        {

            if (_R == null || _R.RowCount == 0)
            {
                _R = new RectangularMatrix(3, 3, true);
                _R[0, 0] = StaticVariables.ROBOT_POSITION_VARIANCE;
                _R[1, 1] = StaticVariables.ROBOT_POSITION_VARIANCE;
                _R[2, 2] = StaticVariables.ROBOT_THETA_VARIANCE;
            }

            return _R;
        }

        RectangularMatrix _A = new RectangularMatrix(7, 7, true);
        public override RectangularMatrix A(bool visionProblem, RectangularMatrix x)
        {
            
            double theta = x[2, 0];
            double vpar = x[3, 0], vperp = x[4, 0], vtheta = x[5, 0];
            double stuck = x[6, 0]; 
            double cos_theta = Math.Cos(theta), sin_theta = Math.Sin(theta);

            _A[0, 2] = (1.0 - stuck) * stepsize *
              (vpar * -sin_theta + vperp * -cos_theta);
            _A[0, 3] = cos_theta * (1.0 - stuck) * stepsize;
            _A[0, 4] = -sin_theta * (1.0 - stuck) * stepsize;
            _A[0, 6] = -stepsize * (vpar * cos_theta + vperp * -sin_theta);
            _A[1, 2] = (1.0 - stuck) * stepsize *
              (vpar * cos_theta + vperp * -sin_theta);
            _A[1, 3] = sin_theta * (1.0 - stuck) * stepsize;
            _A[1, 4] = cos_theta * (1.0 - stuck) * stepsize;
            _A[1, 6] = -stepsize * (vpar * sin_theta + vperp * cos_theta);
            _A[2, 5] = (1.0 - stuck) * stepsize;
            _A[2, 6] = -stepsize * vtheta;
            _A[3, 3] = StaticVariables.ROBOT_VELOCITY_NEXT_STEP_COVARIANCE;
            _A[4, 4] = StaticVariables.ROBOT_VELOCITY_NEXT_STEP_COVARIANCE;
            _A[5, 5] = StaticVariables.ROBOT_VELOCITY_NEXT_STEP_COVARIANCE;
            _A[6, 6] = (visionProblem) ? 1 : StaticVariables.ROBOT_STUCK_DECAY;

            return _A;
        }

        RectangularMatrix _W = new RectangularMatrix(new double[,] { { 0, 0, 0, 0 }, 
                               { 0, 0, 0, 0 }, 
                               { 0, 0, 0, 0 }, 
                               { 1, 0, 0, 0 }, 
                               { 0, 1, 0, 0 }, 
                               { 0, 0, 1, 0 }, 
                               { 0, 0, 0, 1 } });
        public override RectangularMatrix W(RectangularMatrix x)
        {
            return _W;
        }
        RectangularMatrix _H = new RectangularMatrix(new double[,]{ { 1, 0, 0, 0, 0, 0, 0 }, 
                               { 0, 1, 0, 0, 0, 0, 0 }, 
                               { 0, 0, 1, 0, 0, 0, 0 } });
        public override RectangularMatrix H(RectangularMatrix x)
        {
            return _H;
        }

        RectangularMatrix _V = new RectangularMatrix(new double[,]{ { 1, 0, 0 }, 
                               { 0, 1, 0 }, 
                               { 0, 0, 1 } });

        public override RectangularMatrix V(RectangularMatrix x)
        {
            return _V;
        }



        public void initial(double t, ref RectangularMatrix x, ref RectangularMatrix P)
        {
            xs.Clear();
            xs.Enqueue(x);
            Ps.Clear();
            Ps.Enqueue(P);
            Is.Clear();
            Is.Enqueue(new RectangularMatrix(0, 0));
            cs.Clear();
            cs.Enqueue(new rcommand(t, new Vector3D()));
            stepped_time = time = t;
        }
        //Omid : chk back and last
        public void propagate(bool visionProblem)
        {

            //  HiPerfTimer ht = new HiPerfTimer();
            //  ht.Start();
            RectangularMatrix x = xs.Last().Copy();
            RectangularMatrix P = Ps.Last();
            RectangularMatrix __A = A(visionProblem, x);

            RectangularMatrix I = new RectangularMatrix(0, 0);
            int c = 0;
            x = f(visionProblem, x, ref I);

            //if (time > 0.08)
            //{
            //    ;
            //}
            // HiPerfTimer ht = new HiPerfTimer();
            //  ht.Start();
            P = __A * P * __A.Transpose() + tmpC;
            //   ht.Stop();
            // float time = (ht.Duration * 1000);
            //  Console.WriteLine(time);
            xs.Enqueue(x);
            Ps.Enqueue(P);
            Is.Enqueue(I);
            stepped_time += stepsize;
            //   ht.Stop();
            // float time = ht.Duration * 1000;
            //if (type == 1)
            //{
            //    Console.WriteLine(time);
            //    if (time < 0.15)
            //    {
            //        ;
            //    }
            //}

        }
        public void update(bool visionProblem, RectangularMatrix z)
        {

            RectangularMatrix x = xs.First();
            RectangularMatrix P = Ps.First();
            RectangularMatrix I = Is.First();
            RectangularMatrix __H = H(x);
            RectangularMatrix __V = V(x);
            RectangularMatrix __R = R(x);


            // We clear the prediction list because we have a new observation.

            xs.Clear(); Ps.Clear(); Is.Clear(); stepped_time = time;

            RectangularMatrix K = P * __H.Transpose() * ((SquareMatrix)(__H * P * __H.Transpose() + tmpCV)).Inverse();

            RectangularMatrix error = K * (z - h(x));
         
            x = x + error;
            
            imCheck[0, 0] = x[0, 0];
            imCheck[1, 0] = x[1, 0];
            imCheck[2, 0] = x[2, 0];
            imCheck[3, 0] = x[3, 0];
            //HiPerfTimer ht = new HiPerfTimer();
            //ht.Start();
            P = (new RectangularMatrix(P.RowCount, P.RowCount, true) - K * __H) * P;
            //ht.Stop();
            //Console.WriteLine(ht.Duration * 1000);
            // Add the current state back onto the prediction list.
            xs.Enqueue(x); Ps.Enqueue(P); Is.Enqueue(I);

            if (prediction_lookahead > 0.0)
            {
                if (time - prediction_time >= prediction_lookahead)
                {
                    if (prediction_x != null)
                    {
                        if (prediction_time > 0.0)
                        {
                            error = x - prediction_x;

                            for (int i = 0; i < error.RowCount; i++)
                                errors[i, 0] += Math.Abs(error[i, 0]);
                            errors_n++;
                        }
                    }
                    prediction_x = predict(visionProblem,prediction_lookahead);
                    prediction_time = time;
                }
            }
        }

        //Omid : chk if numberof dequeue is correct
        public void tick(bool visionProblem, double dt)
        {
            int nsteps = (int)Math.Round(dt / stepsize);

            while (xs.Count - 1 < nsteps) propagate(visionProblem);

            int i = 0;
            while (i < nsteps)
            {
                if (xs.Count > 0) xs.Dequeue();
                if (Ps.Count > 0) Ps.Dequeue();
                if (Is.Count > 0) Is.Dequeue();
                i++;
            }
            time += dt;
        }

        //Omid : chk whOrNot elemtAt == []
        public RectangularMatrix predict(bool visionProblem, double dt)
        {
            int nsteps = (int)Math.Round(dt / stepsize);

            while (xs.Count - 1 < nsteps) propagate(visionProblem);

            return xs.ElementAt(nsteps);
        }


        public RectangularMatrix predict_cov(bool visionProblem, double dt)
        {
            int nsteps = (int)Math.Round(dt / stepsize);
            //HiPerfTimer ht = new HiPerfTimer();
            // ht.Start();
            int i = 0;
            while (xs.Count - 1 < nsteps) { i++; propagate(visionProblem); }
            //  ht.Stop();
            // Console.WriteLine(ht.Duration * 1000 + "      " + i);
            return Ps.ElementAt(nsteps);
        }

        public RectangularMatrix predict_info(bool visionProblem,double dt)
        {
            int nsteps = (int)Math.Round(dt / stepsize);

            while (xs.Count - 1 < nsteps) propagate(visionProblem);

            return Is.ElementAt(nsteps);
        }

        public RectangularMatrix predict_fast(bool visionProblem, double dt)
        {
            int nsteps = (int)Math.Round(dt / stepsize);
            double orig_stepsize = stepsize;

            if (xs.Count - 1 >= nsteps) return xs.ElementAt(nsteps);

            stepsize = dt - (stepped_time - time);
            propagate(visionProblem);

            RectangularMatrix rv = xs.Last();

            stepped_time -= stepsize;
            stepsize = orig_stepsize;
            //Omid : implementing pop_back by reverese
            Ps = ReverseQ(Ps);
            Ps.Dequeue();
            Ps = ReverseQ(Ps);

            Is = ReverseQ(Is);
            Is.Dequeue();
            Is = ReverseQ(Is);

            return rv;
        }
        Queue<RectangularMatrix> ReverseQ(Queue<RectangularMatrix> q)
        {
            Queue<RectangularMatrix> q2 = new Queue<RectangularMatrix>();
            if (q == null)
                return null;
            for (int i = q.Count - 1; i >= 0; i--)
            {
                q2.Enqueue(q.ElementAt(i));
            }
            return q2;
        }
        public double obs_likelihood(bool visionProblem, double dt, RectangularMatrix z)
        {
            RectangularMatrix x = predict(visionProblem, dt);
            RectangularMatrix P = predict_cov(visionProblem, dt);
            RectangularMatrix _hx = h(x);
            RectangularMatrix _H = H(x);

            RectangularMatrix C = _H * P * (_H.Transpose());

            RectangularMatrix D = z - _hx;

            double likelihood = 1.0;

            for (int i = 0; i < D.RowCount; i++)
                likelihood *= Math.Exp(-(D[i, 0] * D[i, 0]) / (2 * C[i, i]));

            return likelihood;
        }

        public RectangularMatrix error_mean()
        {
            return (1.0 / (double)errors_n) * errors;
        }

        public void error_reset()
        {
            errors = 0 * errors;
            errors_n = 0;
        }

        public double error_time_elapsed()
        {
            return errors_n * prediction_lookahead;
        }
    }
}
