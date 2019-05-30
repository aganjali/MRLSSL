using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using Meta.Numerics.Matrices;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public class Kalman
    {
        protected int state_n, obs_n; // Number of state and observation variables
        protected int type;
        protected double stepsize;
        protected bool _isImmobile = false;
        protected Queue<RectangularMatrix> xs = new Queue<RectangularMatrix>(); // State vector. [0] is current state.
        protected Queue<RectangularMatrix> Ps = new Queue<RectangularMatrix>(); // Covariance matrix.  [0] is current covariance.
        protected Queue<RectangularMatrix> Is = new Queue<RectangularMatrix>(); // Information matrix. [0] is current information.

        protected double stepped_time; // Time of the last state in the future queue.
        protected double time; // Time of the first state in the future queue.

        // Kalman Error
        protected RectangularMatrix prediction_x;
        protected double prediction_time;
        protected double prediction_lookahead;

        protected RectangularMatrix errors;
        protected int errors_n;

        public virtual RectangularMatrix f(bool visionProblem, RectangularMatrix x, ref RectangularMatrix I, bool checkCollision) { return new RectangularMatrix(1, 1); }// noiseless dynamics
        public virtual RectangularMatrix h(RectangularMatrix x) { return new RectangularMatrix(1,1); } // noiseless observation

        public virtual RectangularMatrix Q(RectangularMatrix x) { return new RectangularMatrix(1, 1); } // Covariance of propagation noise
        public virtual RectangularMatrix R(RectangularMatrix x) { return new RectangularMatrix(1, 1); } // Covariance of observation noise

        public virtual RectangularMatrix A(bool visionProblem, RectangularMatrix x) { return new RectangularMatrix(1, 1); } // Jacobian of f w.r.t. x
        public virtual RectangularMatrix W(RectangularMatrix x) { return new RectangularMatrix(1, 1); } // Jacobian of f w.r.t. noise
        public virtual RectangularMatrix H(RectangularMatrix x) { return new RectangularMatrix(1, 1); } // Jacobian of h w.r.t. x
        public virtual RectangularMatrix V(RectangularMatrix x) { return new RectangularMatrix(1, 1); } // Jacobian of h w.r.t. noise
        protected RectangularMatrix tmpC;
        protected RectangularMatrix tmpCV;
        public Kalman(int _state_n, int _obs_n, double _stepsize)
        {
            state_n = _state_n;
            obs_n = _obs_n;
            stepsize = _stepsize;

            xs.Clear();
            xs.Enqueue(new RectangularMatrix(state_n, 1));
            Ps.Clear();
            Ps.Enqueue(new RectangularMatrix(state_n, state_n));
            Is.Clear();
            Is.Enqueue(new RectangularMatrix(1, 1));

            prediction_lookahead = 0;
            prediction_time = 0;
            errors = new RectangularMatrix(_state_n, 1);
        }

        public void initial(double t, ref RectangularMatrix x, ref RectangularMatrix P)
        {
            xs.Clear();
            xs.Enqueue(x);
            Ps.Clear();
            Ps.Enqueue(P);
            Is.Clear();
            Is.Enqueue(new RectangularMatrix(1, 1));
            stepped_time = time = t;
        }
        //Omid : chk back and last
        //protected void propagate()
        //{

        //  //  HiPerfTimer ht = new HiPerfTimer();
        //  //  ht.Start();
        //    MathMatrix x = xs.Last();
        //    MathMatrix P = Ps.Last();
        //    MathMatrix __A = A(x);

        //    MathMatrix I = new MathMatrix(0, 0);

        //    x = f(x, I);

        //    //if (time > 0.08)
        //    //{
        //    //    ;
        //    //}
        //   // HiPerfTimer ht = new HiPerfTimer();
        //  //  ht.Start();
        //    if (type == 1)
        //    {
        //        MathMatrix __W = W(x);
        //        MathMatrix __Q = Q(x);
        //        tmpC = __W * __Q * __W.Transpose;
        //    }
        //    P = __A * P * __A.Transpose + tmpC;
        // //   ht.Stop();
        //   // float time = (ht.Duration * 1000);
        //  //  Console.WriteLine(time);
        //    xs.Enqueue(x);
        //    Ps.Enqueue(P);
        //    Is.Enqueue(I);
        //    stepped_time += stepsize;
        // //   ht.Stop();
        //   // float time = ht.Duration * 1000;
        //    //if (type == 1)
        //    //{
        //    //    Console.WriteLine(time);
        //    //    if (time < 0.15)
        //    //    {
        //    //        ;
        //    //    }
        //    //}

        //}
        protected RectangularMatrix imCheck = new RectangularMatrix(4, 1);
        //public void update(MathMatrix z)
        //{

        //    MathMatrix x = xs.First();
        //    MathMatrix P = Ps.First();
        //    MathMatrix I = Is.First();
        //    MathMatrix __H = H(x);
        //    MathMatrix __V = V(x);
        //    MathMatrix __R = R(x);


        //    // We clear the prediction list because we have a new observation.

        //    xs.Clear(); Ps.Clear(); Is.Clear(); stepped_time = time;

        //    MathMatrix K = P * (__H.Transpose) * Inverse.invert(__H * P * (__H.Transpose) + __V * __R * (__V.Transpose));

        //    MathMatrix error = K * (z - h(x));

        //    x = x + error;
        //    imCheck[0, 0] = x[0, 0];
        //    imCheck[1, 0] = x[1, 0];
        //    imCheck[2, 0] = x[2, 0];
        //    imCheck[3, 0] = x[3, 0];
        //    //HiPerfTimer ht = new HiPerfTimer();
        //    //ht.Start();
        //    P = (MathMatrix.IdentityMatrix(P.Rows, P.Rows) - K * __H) * P;
        //    //ht.Stop();
        //    //Console.WriteLine(ht.Duration * 1000);
        //    // Add the current state back onto the prediction list.
        //    xs.Enqueue(x); Ps.Enqueue(P); Is.Enqueue(I);

        //    if (prediction_lookahead > 0.0)
        //    {
        //        if (time - prediction_time >= prediction_lookahead)
        //        {
        //            if (prediction_x != null)
        //            {
        //                if (prediction_time > 0.0)
        //                {
        //                    error = x - prediction_x;

        //                    for (int i = 0; i < error.Rows; i++)
        //                        errors[i, 0] += Math.Abs(error[i, 0]);
        //                    errors_n++;
        //                }
        //            }
        //            prediction_x = predict(prediction_lookahead);
        //            prediction_time = time;
        //        }
        //    }
        //}

        public virtual bool IsImmobile()
        {
            return false;
        }


        //Omid : chk if numberof dequeue is correct
        //public void tick(double dt)
        //{
        //    int nsteps = (int)Math.Round(dt / stepsize);

        //    while (xs.Count - 1 < nsteps) propagate();

        //    int i = 0;
        //    while (i < nsteps)
        //    {
        //        if (xs.Count > 0) xs.Dequeue();
        //        if (Ps.Count > 0) Ps.Dequeue();
        //        if (Is.Count > 0) Is.Dequeue();
        //        i++;
        //    }
        //    time += dt;
        //}

        ////Omid : chk whOrNot elemtAt == []
        //public MathMatrix predict(double dt)
        //{
        //    int nsteps = (int)Math.Round(dt / stepsize);

        //    while (xs.Count - 1 < nsteps) propagate();

        //    return xs.ElementAt(nsteps);
        //}


        //public MathMatrix predict_cov(double dt)
        //{
        //    int nsteps = (int)Math.Round(dt / stepsize);
        //    //HiPerfTimer ht = new HiPerfTimer();
        //   // ht.Start();
        //    int i = 0;
        //    while (xs.Count - 1 < nsteps) { i++; propagate(); }
        //  //  ht.Stop();
        //    // Console.WriteLine(ht.Duration * 1000 + "      " + i);
        //    return Ps.ElementAt(nsteps);
        //}

        //public MathMatrix predict_info(double dt)
        //{
        //    int nsteps = (int)Math.Round(dt / stepsize);

        //    while (xs.Count - 1 < nsteps) propagate();

        //    return Is.ElementAt(nsteps);
        //}

        //public MathMatrix predict_fast(double dt)
        //{
        //    int nsteps = (int)Math.Round(dt / stepsize);
        //    double orig_stepsize = stepsize;

        //    if (xs.Count - 1 >= nsteps) return xs.ElementAt(nsteps);

        //    stepsize = dt - (stepped_time - time);
        //    propagate();

        //    MathMatrix rv = xs.Last();

        //    stepped_time -= stepsize;
        //    stepsize = orig_stepsize;
        //    //Omid : implementing pop_back by reverese
        //    Ps = ReverseQ(Ps);
        //    Ps.Dequeue();
        //    Ps = ReverseQ(Ps);

        //    Is = ReverseQ(Is);
        //    Is.Dequeue();
        //    Is = ReverseQ(Is);

        //    return rv;
        //}
        //Queue<MathMatrix> ReverseQ(Queue<MathMatrix> q)
        //{
        //    Queue<MathMatrix> q2 = new Queue<MathMatrix>();
        //    if (q == null)
        //        return null;
        //    for (int i = q.Count - 1; i >= 0; i--)
        //    {
        //        q2.Enqueue(q.ElementAt(i));
        //    }
        //    return q2;
        //}
        //public double obs_likelihood(double dt, MathMatrix z)
        //{
        //    MathMatrix x = predict(dt);
        //    MathMatrix P = predict_cov(dt);
        //    MathMatrix _hx = h(x);
        //    MathMatrix _H = H(x);

        //    MathMatrix C = _H * P * (_H.Transpose);

        //    MathMatrix D = z - _hx;

        //    double likelihood = 1.0;

        //    for (int i = 0; i < D.Rows; i++)
        //        likelihood *= Math.Exp(-(D[i, 0] * D[i, 0]) / (2 * C[i, i]));

        //    return likelihood;
        //}

        //public MathMatrix error_mean()
        //{
        //    return (1.0 / (double)errors_n) * errors;
        //}

        //public void error_reset()
        //{
        //    errors = 0 * errors;
        //    errors_n = 0;
        //}

        //public double error_time_elapsed()
        //{
        //    return errors_n * prediction_lookahead;
        //}


    }
}
