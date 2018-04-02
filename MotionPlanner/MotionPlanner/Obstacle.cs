using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.Planning.MotionPlanner
{
    public class Obstacle
    {
       
        ObstacleType type;
        Vector2D r;
        SingleObjectState state;

        public SingleObjectState State
        {
            get { return state; }
            set { state = value; }
        }
        public Vector2D R
        {
            get { return r; }
            set { r = value; }
        }
        public ObstacleType Type
        {
            get { return type; }
            set { type = value; }
        }

        public bool Meet(SingleObjectState From, SingleObjectState To, double obstacleRadi)
        {

            bool c = (type == ObstacleType.Circle || type == ObstacleType.OurRobot || type == ObstacleType.OppRobot || type == ObstacleType.ZoneCircle || type == ObstacleType.Ball);
            if (c)
                return MeetCircle(From, To, obstacleRadi);
            else 
                return MeetRectangle(From, To, obstacleRadi);
        }
        public bool Meet(SingleObjectState S1, double obstacleRadi)
        {
            // TODO: must check for rectangle
            Vector2D v = S1.Location - state.Location;
            bool c = (type == ObstacleType.Circle || type == ObstacleType.OurRobot || type == ObstacleType.OppRobot || type == ObstacleType.ZoneCircle || type == ObstacleType.Ball);
            if (c)
                return (v.Size < obstacleRadi + r.X);
            else
                return (Math.Abs(v.X) < r.X + obstacleRadi && Math.Abs(v.Y) < r.Y + obstacleRadi);
        }

        private bool MeetRectangle(SingleObjectState From, SingleObjectState To, double obstacleRadi)
        {
            Position2D N = From.Location;
            Position2D T = To.Location;
            Position2D[] c = new Position2D[4];

            c[0] = new Position2D(state.Location.X - r.X, state.Location.Y - r.Y);
            c[1] = new Position2D(state.Location.X + r.X, state.Location.Y - r.Y);
            c[2] = new Position2D(state.Location.X + r.X, state.Location.Y + r.Y);
            c[3] = new Position2D(state.Location.X - r.X, state.Location.Y + r.Y);
            double d = 0;
            // check box against oriented sweep
            for (int i = 0; i < 4; i++)
            {
                d = distance_seg_to_seg(N, T, c[i], c[(i + 1) % 4]);
                if (d < obstacleRadi) return (true);
            }
            return (Meet(new SingleObjectState(N, Vector2D.Zero, null), obstacleRadi) || Meet(new SingleObjectState(T, Vector2D.Zero, null), obstacleRadi));
        }
        private bool MeetCircle(SingleObjectState From, SingleObjectState To, double obstacleRadi)
        {
            Position2D p = point_on_segment(From.Location,To.Location,state.Location);

            double d = p.DistanceFrom(state.Location);
            return (d <= r.X + obstacleRadi);
           
        }

        Position2D point_on_segment(Position2D x0, Position2D x1, Position2D p)
        {
            Vector2D sx, sp;
            Position2D r;
            double f, l;

            sx = x1 - x0;
            sp = p - x0;

            f = sx.InnerProduct(sp);
            if (f <= 0.0) return (x0); // also handles x0=x1 case

            l = sx.SquareSize;
            if (f >= l) return (x1);

            r = x0 + sx * (f / l);

            return (r);
        }
        private double distance_seg_to_seg(Position2D s1a, Position2D s1b, Position2D s2a, Position2D s2b)
        {
            const double EPSILON = 1.0E-10;
            Vector2D dp;
            Vector2D u = s1b - s1a;
            Vector2D v = s2b - s2a;
            Vector2D w = s1a - s2a;
            double a = u.InnerProduct(u);        // always >= 0
            double b = u.InnerProduct(v);
            double c = v.InnerProduct(v);        // always >= 0
            double d = u.InnerProduct(w);
            double e = v.InnerProduct(w);
            double D = a * c - b * b;       // always >= 0
            double sc, sN, sD = D;      // sc = sN / sD, default sD = D >= 0
            double tc, tN, tD = D;      // tc = tN / tD, default tD = D >= 0

            if (D < EPSILON)
            {    // the lines are almost parallel
                sN = 0.0;
                tN = e;
                tD = c;
            }
            else
            {                // get the closest points on the infinite lines
                sN = (b * e - c * d);
                tN = (a * e - b * d);
                if (sN < 0)
                {         // sc < 0 => the s=0 edge is visible
                    sN = 0.0;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD)
                {  // sc > 1 => the s=1 edge is visible
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0)
            {           // tc < 0 => the t=0 edge is visible
                tN = 0.0;
                // recompute sc for this edge
                if (-d < 0)
                {
                    sN = 0.0;
                }
                else if (-d > a)
                {
                    sN = sD;
                }
                else
                {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD)
            {      // tc > 1 => the t=1 edge is visible
                tN = tD;
                // recompute sc for this edge
                if ((-d + b) < 0)
                {
                    sN = 0;
                }
                else if ((-d + b) > a)
                {
                    sN = sD;
                }
                else
                {
                    sN = (-d + b);
                    sD = a;
                }
            }
            // finally do the division to get sc and tc
            sc = sN / sD;
            tc = tN / tD;

            // get the difference of the two closest points
            dp = w + u * sc - v * tc; // = S1(sc) - S2(tc)

            return (dp.Size); // return the closest distance
        }
    }
    public enum ObstacleType
    { 
        Circle,
        Rectangle,
        OurRobot,
        OppRobot,
        Ball,
        ZoneCircle,
        ZoneRectangle
    }
}
