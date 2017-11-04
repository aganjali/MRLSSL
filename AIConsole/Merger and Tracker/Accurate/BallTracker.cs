
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using Meta.Numerics.Matrices;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public enum OccludeFlag { Visible, MaybeOccluded, Occluded };
    public class BallTracker : Kalman
    {
        public bool Collision = false;
        public Vector2D CV = new Vector2D();
        private bool _reset;
        // Pointer up to the complete set of trackers... for collisions,
        // occlusions, and such.
        public VTracker tracker;

        // Occlusion Information
        public OccludeFlag occluded;
        public int occluding_team;
        public int occluding_robot;
        public Vector2D occluding_offset;
        public double occluded_last_obs_time;

        public BallTracker()
            : base(4, 2, StaticVariables.FRAME_PERIOD)
        {

            _reset = true;
            occluded = OccludeFlag.Visible;
            tracker = null;
            if (StaticVariables.BALL_PRINT_KALMAN_ERROR)
                prediction_lookahead = StaticVariables.LATENCY_DELAY;

            RectangularMatrix __V = V(new RectangularMatrix(0, 0));
            RectangularMatrix __R = R(new RectangularMatrix(0, 0));
            tmpCV = __V * __R * __V.Transpose();
        }

        public bool checkIsImmobile(RectangularMatrix x)
        {
            //if ((x[2, 0] / 1000) < 0.09 && -(x[3, 0] / 1000) < 0.09)
            //    return true;
            return true;
        }
        public override bool IsImmobile()
        {
            Vector2D tmp = velocity(StaticVariables.FRAME_PERIOD);
            if (Math.Abs(tmp.X) < 0.13 && Math.Abs(tmp.Y) < 0.13)
                return true;
            return false;
        }
        public double velocity_variance(RectangularMatrix x)
        {
            if (tracker == null) return StaticVariables.BALL_VELOCITY_VARIANCE_NEAR_ROBOT;

            Position2D ball = new Position2D(x[0, 0], x[1, 0]);
            double dist = 5000.0;

            for (int i = 0; i < StaticVariables.NUM_TEAMS; i++)
            {
                for (int j = 0; j < StaticVariables.MAX_ROBOT_ID; j++)
                {
                    if (!tracker.Exists(i, j)) continue;

                    double d = (tracker.robots[i, j].position(false,0.0) - ball).Size;
                    if (d < dist) dist = d;
                }
            }

            double r = util.bound((dist - StaticVariables.ROBOT_DEF_WIDTH_H) / StaticVariables.ROBOT_DEF_WIDTH_H, 0, 1);
            if (r < 0)
                r = 0;
            if (r > 1)
                r = 1;
            return r * StaticVariables.BALL_VELOCITY_VARIANCE_NO_ROBOT +
              (1 - r) * StaticVariables.BALL_VELOCITY_VARIANCE_NEAR_ROBOT;
        }

        public bool check_occlusion()
        {
            if (tracker == null) return false;

            if (occluded != OccludeFlag.Visible) return true;

            Position2D camera = new Position2D(0, 0);
            Vector2D ball = position(0.0) - camera;

            double occluding_pct = 0.5;

            for (int i = 0; i < StaticVariables.NUM_TEAMS; i++)
            {
                for (int j = 0; j < StaticVariables.MAX_ROBOT_ID; j++)
                {
                    if (!tracker.Exists(i, j)) continue;

                    double radius = tracker.Radius(i, j);
                    double height = tracker.Height(i, j);

                    Vector2D p = tracker.robots[i, j].position(false, 0.0) - camera;
                    double from = Vector2D.offset_to_line(Position2D.Zero, new Position2D(ball.X, ball.Y), new Position2D(p.X, p.Y));
                    double along = Vector2D.offset_along_line(Position2D.Zero, new Position2D(ball.X, ball.Y), new Position2D(p.X, p.Y));
                    double ball_along = ball.Size;

                    if (Math.Abs(from) > radius) continue;
                    if (ball_along < along) continue;


                    along += Math.Sqrt(radius * radius - from * from);

                    double x = (along * height) / (StaticVariables.CAMERA_HEIGHT - height);
                    double pct = (x - (ball_along - along) + StaticVariables.BALL_RADIUS) /
                  (2.0 * StaticVariables.BALL_RADIUS);

                    if (pct < 0)
                        pct = 0;
                    if (pct > 1)
                        pct = 1;

                    if (pct > occluding_pct)
                    {
                        occluded = OccludeFlag.MaybeOccluded;
                        occluding_team = i;
                        occluding_robot = j;
                        occluding_offset = (ball - p).GetRotate(-(new Position2D(p.X, p.Y) - camera).AngleInRadians);

                        occluding_pct = pct;
                    }
                }

            }

            return (occluded != OccludeFlag.Occluded);
        }

        public void tick_occlusion(double dt)
        {
            Position2D camera = new Position2D(0, 0);

            Position2D p = tracker.robots[occluding_team, occluding_robot].position(false, 0.0);
            Vector2D v = tracker.robots[occluding_team, occluding_robot].velocity(false, 0.0);
            Vector2D b = occluding_offset.GetRotate((p - camera).AngleInRadians);

            double bdelta = Math.Max(v.InnerProduct(b.GetNormnalizedCopy()) * dt, 0.0);
            double radius;


            radius = StaticVariables.BALL_TEAMMATE_COLLISION_RADIUS;

            if (b.Size - bdelta < radius) b = b.GetNormalizeToCopy(radius);
            else b = b.GetNormalizeToCopy(b.Size - bdelta);

            occluding_offset = b.GetRotate(-(p - camera).AngleInRadians);

            // Update the x and P queue.
            RectangularMatrix x = new RectangularMatrix(4, 1);
            RectangularMatrix P = new RectangularMatrix(4, 4, true);
            Position2D xp = occluded_position(dt);
            Vector2D xv = occluded_velocity(dt);

            x[0, 0] = xp.X;
            x[1, 0] = xp.Y;
            x[2, 0] = xv.X;
            x[3, 0] = xv.Y;

            P[0, 0] *= StaticVariables.BALL_POSITION_VARIANCE;
            P[1, 1] *= StaticVariables.BALL_POSITION_VARIANCE;
            P[2, 2] *= 250000.0; // 500m/s
            P[3, 3] *= 250000.0; // 500m/s

            xs.Clear(); xs.Enqueue(x);
            Ps.Clear(); Ps.Enqueue(P);
            time += dt;
        }
        public Position2D occluded_position(double time)
        {
            if (tracker == null) return new Position2D(0.0, 0.0);

            Position2D camera = new Position2D(0, 0);
            Position2D b;

            b = tracker.robots[occluding_team, occluding_robot].position(false, time);
            b = b + occluding_offset.GetRotate((b - camera).AngleInRadians);

            return b;
        }
        public Vector2D occluded_velocity(double time)
        {
            if (tracker == null) return Vector2D.Zero;
            return tracker.robots[occluding_team, occluding_robot].velocity(false, time);
        }

        public void observe(vraw obs, double timestamp)
        {

            // mhb: Need this?
            if (double.IsNaN(xs.ElementAt(0)[0, 0]) || Math.Abs(timestamp - time) > StaticVariables.MaxPredictTime) _reset = true;

            if (_reset && obs.timestamp >= timestamp &&
                obs.conf >= (StaticVariables.BALL_CONFIDENCE_THRESHOLD))
            {
                RectangularMatrix x = new RectangularMatrix(4, 1), P = new RectangularMatrix(4, 4, true);

                x[0, 0] = obs.pos.X;
                x[1, 0] = obs.pos.Y;
                x[2, 0] = 0.0;
                x[3, 0] = 0.0;

                P[0, 0] *= StaticVariables.BALL_POSITION_VARIANCE;
                P[1, 1] *= StaticVariables.BALL_POSITION_VARIANCE;
                P[2, 2] *= 250000.0; // 500m/s
                P[3, 3] *= 250000.0; // 500m/s

                initial(obs.timestamp, ref x, ref P);

                occluded = OccludeFlag.Visible;

                _reset = false;

            }
            else
            {

                if (_reset && occluded != OccludeFlag.Occluded) return;

                // If this is a new observation.
                if (timestamp > time)
                {

                    // Tick to current time.
                    if (occluded == OccludeFlag.Occluded)
                    {
                        tick_occlusion(timestamp - time);
                    }
                    else
                    {
                        tick(timestamp - time);
                    }

                    // Make Observation Matrix
                    RectangularMatrix o = new RectangularMatrix(2, 1);
                    o[0, 0] = obs.pos.X;
                    o[1, 0] = obs.pos.Y;

                    if (StaticVariables.BALL_IMPROBABILITY_FILTERING > 0)
                    {
                        // Check for improbable observations (i.e. noise)
                        if (obs_likelihood(0.0, o) <= StaticVariables.BALL_LIKELIHOOD_THRESHOLD)
                            obs.conf = -1.0f;
                    }

                    // Make observation
                    if (obs.timestamp == timestamp &&
                    obs.conf > StaticVariables.BALL_CONFIDENCE_THRESHOLD)
                    {
                        update(o);

                        occluded = OccludeFlag.Visible;
                        occluded_last_obs_time = obs.timestamp;

                    }
                    else
                    {

                        if (occluded == OccludeFlag.Visible)
                            check_occlusion();

                        if (occluded == OccludeFlag.MaybeOccluded &&
                            timestamp - occluded_last_obs_time > StaticVariables.BALL_OCCLUDE_TIME)
                        {

                            occluded = OccludeFlag.Occluded;
                            _reset = true;
                        }
                    }

                    if (error_time_elapsed() > 10.0)
                    {

                        error_reset();
                    }

                }
            }
        }
        public void set_tracker(VTracker t)
        {
            tracker = t;
        }

        public void reset()
        {
            _reset = true;
        }
        //float state[4], float variances[16]
        public void reset(double timestamp, SingleObjectState state, RectangularMatrix variances, OccludeFlag _occluded, int _occluding_team, int _occluding_robot, Vector2D _occluding_offset)
        {
            RectangularMatrix x = new RectangularMatrix(4, 1); ;
            x[0, 0] = state.Location.X * 1000;
            x[1, 0] = -state.Location.Y * 1000;
            x[2, 0] = state.Speed.X * 1000;
            x[3, 0] = -state.Speed.Y * 1000;
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 1; j++)
            //    {
            //        xx[i, j] = state[i];
            //    }

            //}

            //double[,] vv = new double[4, 4];
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 4; j++)
            //    {
            //        vv[i, j] = variances[i * 4 + j];
            //    }
            //}
            RectangularMatrix P = variances.Copy();
            initial(timestamp, ref x, ref P);

            occluded = _occluded;
            occluding_team = _occluding_team;
            occluding_robot = _occluding_robot;
            occluding_offset = new Vector2D(_occluding_offset.X, _occluding_offset.Y);
            _reset = false;
        }
        public Position2D position(double time, bool checkCollision = true)
        {
            if (occluded == OccludeFlag.Occluded && checkCollision) return occluded_position(time);
            RectangularMatrix x = predict(time, checkCollision);
            return new Position2D(x[0, 0], x[1, 0]);
        }

        public Vector2D velocity(double time, bool checkCollision = true)
        {
            if (occluded == OccludeFlag.Occluded && checkCollision) return occluded_velocity(time);

            RectangularMatrix x = predict(time, checkCollision);
            return new Vector2D(x[2, 0], x[3, 0]);
        }
        public RectangularMatrix covariances(double time, bool checkCollision = true)
        {
            return predict_cov(time, checkCollision);
        }

        public bool collision(double time, ref int team, ref int robot)
        {
            RectangularMatrix I = predict_info(time);

            if (I.RowCount <= 1) return false;

            team = (int)Math.Round(I[1, 0]);
            robot = (int)Math.Round(I[1, 0]);

            return true;
        }

        public bool check_for_collision(RectangularMatrix x,
                      ref Vector2D cp, ref Vector2D cv,
                      ref int team, ref int robot)
        {
            if (tracker == null) return false;

            Vector2D bp = new Vector2D(x[0, 0], x[1, 0]);
            Vector2D bv = new Vector2D(x[2, 0], x[3, 0]);

            double dist = 5000.0;
            bool rv = false;

            for (int i = 0; i < StaticVariables.NUM_TEAMS; i++)
            {
                for (int j = 0; j < StaticVariables.MAX_ROBOT_ID; j++)
                {
                    if (!tracker.Exists(i, j)) continue;

                    double radius;
                    radius = (StaticVariables.BALL_TEAMMATE_COLLISION_RADIUS);

                    if (radius <= 0) continue;

                    Position2D p = tracker.robots[i, j].position(false,/*stepped_time - time*/0);
                    Vector2D v = tracker.robots[i, j].velocity(false,/*stepped_time - time*/0);
                    double d = Math.Min((p - bp).Size,
                           (p + v * stepsize - bp + bv * stepsize).Size);

                    // Ball is within radius, nothing else is closest, and ball is
                    //  moving towards the robot...  Count as collision.
                    if (d <= radius && d < dist && (new Position2D(bv.X, bv.Y) - new Position2D(v.X, v.Y)).InnerProduct(p - new Position2D(bp.X, bp.Y)) > 0.0)
                    {
                        Position2D tmp = p + (new Position2D(bp.X, bp.Y) - p).GetNormalizeToCopy(radius);
                        cp = new Vector2D(tmp.X, tmp.Y);
                        cv = v; rv = true; dist = d;
                        team = i; robot = j;
                    }
                }
            }

            return rv;
        }
        public RectangularMatrix _f;
        public override RectangularMatrix f(bool visionProblem, RectangularMatrix x, ref RectangularMatrix I, bool checkCollision = true)
        {
            I = new RectangularMatrix(0, 0);
            _f = x.Copy(); // Copy Matrix
            double _x = _f[0, 0], _y = _f[1, 0], _vx = _f[2, 0], _vy = _f[3, 0];
            double _v = Math.Sqrt(_vx * _vx + _vy * _vy);

            double _a = Math.Min(StaticVariables.BALL_FRICTION * StaticVariables.GRAVITY, _v / (double)stepsize);
            double _ax = (_v == 0.0) ? 0.0 : -_a * _vx / _v;
            double _ay = (_v == 0.0) ? 0.0 : -_a * _vy / _v;

            bool walls = false;

            if (StaticVariables.BALL_WALLS_SLOPED)
            {
                if (Math.Abs(_x) > StaticVariables.FIELD_LENGTH_H && Math.Abs(_y) > StaticVariables.GOAL_WIDTH_H)
                {
                    _ax += Math.Abs(StaticVariables.M_SQRT1_2 * StaticVariables.GRAVITY * 5.0 / 7.0) * Math.Sign(-_x);
                    walls = true;
                }

                if (Math.Abs(_y) > StaticVariables.FIELD_WIDTH_H)
                {
                    _ay += Math.Abs(StaticVariables.M_SQRT1_2 * StaticVariables.GRAVITY * 5.0 / 7.0) * Math.Sign(-_y);
                    walls = true;
                }
            }

            if (StaticVariables.BALL_WALLS_OOB)
            {
                if ((Math.Abs(_x) > StaticVariables.FIELD_LENGTH_H + StaticVariables.WALL_WIDTH &&
                 Math.Abs(_y) > StaticVariables.GOAL_WIDTH_H) ||
                (Math.Abs(_y) > StaticVariables.FIELD_WIDTH_H + StaticVariables.WALL_WIDTH))
                {
                    _vx = 0.0;
                    _vy = 0.0;
                    _ax = 0.0;
                    _ay = 0.0;

                    walls = true;
                }
            }

            // Update Position
            _x += _vx * stepsize + 0.5 * _ax * stepsize * stepsize;
            _y += _vy * stepsize + 0.5 * _ay * stepsize * stepsize;

            // If there's a collision... then set ball's velocity to the colliding
            //  object's velocity.
            Vector2D cv = Vector2D.Zero, cp = Vector2D.Zero;
            int team = 0, robot = 0;

            _f[0, 0] = _x;
            _f[1, 0] = _y;
            _f[2, 0] = _vx;
            _f[3, 0] = _vy;
            //checkCollision = false;

            bool col = false;// (checkCollision) ? check_for_collision(_f, ref cp, ref cv, ref team, ref robot) : false;
            if (!walls && col)
            {

                _vx = cv.X; _vy = cv.Y;
                I = new RectangularMatrix(2, 1);
                I[0, 0] = team;
                I[1, 0] = robot;
            }
            else
            {
                _vx += _ax * stepsize;
                _vy += _ay * stepsize;
            }
            //  DrawingObjects.AddObject(new StringDraw(col.ToString(), Position2D.Zero));
            _f[0, 0] = _x;
            _f[1, 0] = _y;
            _f[2, 0] = _vx;
            _f[3, 0] = _vy;
            return _f;
        }


        public RectangularMatrix _h = new RectangularMatrix(2, 1);
        public override RectangularMatrix h(RectangularMatrix x)
        {

            _h[0, 0] = x[0, 0];
            _h[1, 0] = x[1, 0];
            return _h;
        }

        public RectangularMatrix _Q = new RectangularMatrix(2, 2, true);
        public override RectangularMatrix Q(RectangularMatrix x)
        {
            _Q[0, 0] = _Q[1, 1] = velocity_variance(x);
            return _Q;
        }

        public RectangularMatrix _R;/// = MathMatrix.IdentityMatrix(2, 2);
        public override RectangularMatrix R(RectangularMatrix x)
        {
            if (_R == null || _R.RowCount == 0)
            {
                _R = new RectangularMatrix(2, 2, true);
                _R = StaticVariables.BALL_POSITION_VARIANCE * _R;
            }
            return _R;
        }

        public RectangularMatrix _A;/// = MathMatrix.IdentityMatrix(2, 2);
        public override RectangularMatrix A(bool visionProblem, RectangularMatrix x)
        {
            if (_A == null || _A.RowCount == 0)
            {
                _A = new RectangularMatrix(4, 4, true);
                _A[0, 2] = stepsize;
                _A[1, 3] = stepsize;
            }
            return _A;
        }

        public RectangularMatrix _W;/// = MathMatrix.IdentityMatrix(2, 2);
        public override RectangularMatrix W(RectangularMatrix x)
        {
            double[,] d = new double[4, 2] { { 0, 0 }, { 0, 0 }, { 1, 0 }, { 0, 1 } };
            _W = new RectangularMatrix(d);
            return _W;
        }

        public RectangularMatrix _H;/// = MathMatrix.IdentityMatrix(2, 2);
        public override RectangularMatrix H(RectangularMatrix x)
        {

            _H = new RectangularMatrix(2, 4, true);
            return _H;
        }

        public RectangularMatrix _V;/// = MathMatrix.IdentityMatrix(2, 2);
        private bool _immobile;
        public override RectangularMatrix V(RectangularMatrix x)
        {
            _V = new RectangularMatrix(2, 2, true);
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
            stepped_time = time = t;
        }
        //Omid : chk back and last
        public void propagate(bool checkCollision = true)
        {
            RectangularMatrix x = xs.Last();
            RectangularMatrix P = Ps.Last();
            RectangularMatrix __A = A(false, x);
            RectangularMatrix I = new RectangularMatrix(0, 0);
            //imCheck[0, 0] = x[0, 0];
            //imCheck[1, 0] = x[1, 0];
            //imCheck[2, 0] = x[2, 0];
            //imCheck[3, 0] = x[3, 0];

            x = f(false, x, ref I, checkCollision);
            RectangularMatrix __W = W(x);
            RectangularMatrix __Q = Q(x);
            tmpC = __W * __Q * __W.Transpose();
            P = __A * P * __A.Transpose() + tmpC;
            xs.Enqueue(x);
            Ps.Enqueue(P);
            Is.Enqueue(I);
            stepped_time += stepsize;
        }
        public void update(RectangularMatrix z)
        {
            RectangularMatrix x = xs.First();
            RectangularMatrix P = Ps.First();
            RectangularMatrix I = Is.First();
            RectangularMatrix __H = H(x);
            xs.Clear(); Ps.Clear(); Is.Clear(); stepped_time = time;
            RectangularMatrix K = P * (__H.Transpose()) * ((SquareMatrix)(__H * P * __H.Transpose() + tmpCV)).Inverse();
            RectangularMatrix error = K * (z - h(x));
            x = x + error;
            imCheck[0, 0] = x[0, 0];
            imCheck[1, 0] = x[1, 0];
            imCheck[2, 0] = x[2, 0];
            imCheck[3, 0] = x[3, 0];
            P = (new RectangularMatrix(P.RowCount, P.RowCount, true) - K * __H) * P;
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
                    prediction_x = predict(prediction_lookahead);
                    prediction_time = time;
                }
            }
        }

        public void tick(double dt, bool checkCollision = true)
        {
            int nsteps = (int)Math.Round(dt / stepsize);

            while (xs.Count - 1 < nsteps) propagate(checkCollision);

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
        public RectangularMatrix predict(double dt, bool checkCollision = true)
        {
            int nsteps = (int)Math.Round(dt / stepsize);

            while (xs.Count - 1 < nsteps) propagate(checkCollision);

            return xs.ElementAt(nsteps);
        }

        public RectangularMatrix predict_cov(double dt, bool checkCollision = true)
        {
            int nsteps = (int)Math.Round(dt / stepsize);
            while (xs.Count - 1 < nsteps) propagate(checkCollision);
            return Ps.ElementAt(nsteps);
        }

        public RectangularMatrix predict_info(double dt, bool checkCollision = true)
        {
            int nsteps = (int)Math.Round(dt / stepsize);

            while (xs.Count - 1 < nsteps) propagate(checkCollision);

            return Is.ElementAt(nsteps);
        }

        public RectangularMatrix predict_fast(double dt, bool checkCollision = true)
        {
            int nsteps = (int)Math.Round(dt / stepsize);
            double orig_stepsize = stepsize;

            if (xs.Count - 1 >= nsteps) return xs.ElementAt(nsteps);

            stepsize = dt - (stepped_time - time);
            propagate(checkCollision);

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
        public double obs_likelihood(double dt, RectangularMatrix z)
        {
            RectangularMatrix x = predict(dt);
            RectangularMatrix P = predict_cov(dt);
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


