using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MRL.SSL.Planning.GamePlanner.Types
{
  
    public enum GObjectType
    {
        Ball,
        OurRobot,
        Opponent
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GVector2D
    {

        public float X, Y;
        public GVector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public GVector2D(ref GVector2D vec)
        {
            X = vec.X;
            Y = vec.Y;
        }
        public float Size()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
        public float SquareSize()
        {
            return X * X + Y * Y;
        }
        public bool Scale(float a)
        {
            X *= a;
            Y *= a;
            return true;
        }
        public bool Normnalize()
        {
            float size = Size();
            if (size == 0)
                X = Y = 0;
            else
            {
                X /= size;
                Y /= size;
            }
            return true;
        }
        public bool NormalizeTo(float NewLength)
        {
            float size = Size();
            if (size == 0)
                X = Y = 0;
            else
            {
                X *= NewLength / size;
                Y *= NewLength / size;
            }
            return true;
        }
        public GVector2D GetNormnalizedCopy()
        {
            GVector2D temp = new GVector2D(X, Y);
            temp.Normnalize();
            return temp;
        }
        public GVector2D GetNormalizeToCopy(float NewLength)
        {
            GVector2D temp = new GVector2D(X, Y);
            temp.NormalizeTo(NewLength);
            return temp;
        }
        public float AngleBetweenInRadians(GVector2D P1)
        {
            float a1 = (float)Math.Atan2(P1.Y, P1.X), a2 = (float)Math.Atan2(Y, X);
            float d = a1 - a2;
            while (d > (float)Math.PI)
                d -= 2 * (float)Math.PI;

            while (d < -(float)Math.PI)
                d += 2 * (float)Math.PI;
            return d;
        }
        public float AngleBetweenInDegrees(GVector2D P1)
        {
            return AngleBetweenInRadians(P1) * 180 / (float)Math.PI;
        }
        public float InnerProduct(GVector2D V)
        {
            return X * V.X + Y * V.Y;
        }
        public GVector2D FromAngleSize(float Angle, float Size)
        {
            return new GVector2D(Size * (float)Math.Cos(Angle), Size * (float)Math.Sin(Angle));
        }
        public float AngleInRadians()
        {
            return (float)Math.Atan2(Y, X);
        }
        public float AngleInDegrees()
        {
            return (AngleInRadians() * 180 / (float)Math.PI);
        }

        public GVector2D Add(GVector2D v1)
        {
            return new GVector2D(X + v1.X, Y + v1.Y);
        }
        public GVector2D Sub(GVector2D v1)
        {
            return new GVector2D(X - v1.X, Y - v1.Y);
        }
        public static implicit operator CommonClasses.MathLibrary.Vector2D(GVector2D GV)
        {
            return new CommonClasses.MathLibrary.Vector2D(GV.X, GV.Y);
        }
        public static implicit operator GVector2D(CommonClasses.MathLibrary.Vector2D V)
        {
            return new GVector2D((float)V.X, (float)V.Y);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct GPosition2D
    {
        public float X, Y;
        public GPosition2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public GPosition2D(ref GPosition2D pos)
        {
            X = pos.X;
            Y = pos.Y;
        }
        public float Size()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
        public float SquareSize()
        {
            return (X * X + Y * Y);
        }
        public float DistanceFrom(GPosition2D From)
        {
            return (float)Math.Sqrt((X - From.X) * (X - From.X) + (Y - From.Y) * (Y - From.Y));
        }
        public GPosition2D Add(GVector2D p1)
        {
            return new GPosition2D(X + p1.X, Y + p1.Y);
        }
        public GVector2D Sub(GPosition2D p1)
        {
            return new GVector2D(X - p1.X, Y - p1.Y);
        }
        public GPosition2D Interpolate(GPosition2D End, float Amount)
        {
            return new GPosition2D(this.X * (1 - Amount) + End.X * Amount, this.Y * (1 - Amount) + End.Y * Amount);
        }
        public static implicit operator CommonClasses.MathLibrary.Position2D(GPosition2D GP)
        {
            return new CommonClasses.MathLibrary.Position2D(GP.X, GP.Y);
        }
        public static implicit operator GPosition2D(CommonClasses.MathLibrary.Position2D P)
        {
            return new GPosition2D((float)P.X, (float)P.Y);
        }
        //[CudafyIgnore]
        //public static explicit operator GPosition2D(CommonClasses.MathLibrary.Position2D P)
        //{
        //    return new GPosition2D((float)P.X, (float)P.Y);
        //}
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct GLine
    {

        public float C;
        public float B;
        public float A;
        public GPosition2D Head;
        public GPosition2D Tail;
        public float Angle()
        {
            return (float)Math.Atan2(B, A);
        }
        public GLine(GPosition2D P1, GPosition2D P2)
        {
            A = P2.Y - P1.Y;
            B = P1.X - P2.X;
            C = -(A * P1.X + B * P1.Y);
            Head = P1;
            Tail = P2;
        }

        public GLine(ref GLine l)
        {
            A = l.A;
            B = l.B;
            C = l.C;
            Head = l.Head;
            Tail = l.Tail;
        }
        public GLine(float a, float b, float c)
        {
            A = a;
            B = b;
            C = c;
            Tail = new GPosition2D(0, -C / A);
            Head = new GPosition2D(0, -C / A);
        }

        public float Distance(GPosition2D P)
        {
            return (float)Math.Abs(A * P.X + B * P.Y + C) / (float)Math.Sqrt(A * A + B * B);
        }
        public GPosition2D CalculateY(float x)
        {
            GPosition2D res = new GPosition2D(0, 0);
            res.X = x;
            res.Y = (-C - A * x) / B;
            return res;
        }
        public GPosition2D IntersectWithLine(GLine l2, out bool HasValue)
        {
            GPosition2D a = new GPosition2D(0, 0);
            float det = this.A * l2.B - l2.A * this.B;
            if ((float)Math.Abs(det) > 0.0001f)
            {
                a.X = ((l2.C * this.B - this.C * l2.B) / det);
                a.Y = ((this.C * l2.A - l2.C * this.A) / det);
                HasValue = true;
            }
            else
                HasValue = false;
            return a;
        }
        public GLine PerpenducilarLineToPoint(GPosition2D From)
        {
            return new GLine(this.B, -this.A, -(this.B * From.X + -this.A * From.Y));
            //line.A * target.Y - line.B * target.X;
        }
        public bool IsPointInRange(GPosition2D P)
        {
            if ((P.Sub(Head)).InnerProduct(Tail.Sub(Head)) < 0)
                return false;
            if ((P.Sub(Tail)).InnerProduct(Head.Sub(Tail)) < 0)
                return false;
            return true;
        }
        public static implicit operator CommonClasses.MathLibrary.Line(GLine GL)
        {
            return new CommonClasses.MathLibrary.Line(GL.A, GL.B, GL.C);
        }
        public static implicit operator GLine(CommonClasses.MathLibrary.Line L)
        {
            return new GLine((float)L.A, (float)L.B, (float)L.C);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct GPartLine
    {
        private GPosition2D _tail, _head;
        private float _a, _b, _c;
        public GPosition2D GetTail()
        {
            return _tail;
        }
        public void SetTail(GPosition2D tail)
        {
            _tail = tail;
            CalculateLineParameters();

        }
        public GPosition2D GetHead()
        {
            return _head;
        }
        public void SetHead(GPosition2D head)
        {
            _head = head;
            CalculateLineParameters();

        }

        public float GetA()
        {
            return _a;
        }
        public float GetB()
        {
            return _b;
        }
        public float GetC()
        {
            return _c;
        }

        public float Angle()
        {
            return (float)Math.Atan2(_b, _a);
        }
        public GPartLine(GPosition2D P1, GPosition2D P2)
        {
            _head = P1;
            _tail = P2;
            //Calculate Line Params
            _a = P2.Y - P1.Y;
            _b = P1.X - P2.X;
            _c = -(_a * P1.X + _b * P1.Y);
        }
        public bool IntersectsWithPerpendicularFrom(GPosition2D P)
        {
            return (((P.Sub(_head)).InnerProduct(_tail.Sub(_head)) >= 0) && ((P.Sub(_tail)).InnerProduct(_head.Sub(_tail)) >= 0));
        }
        public GPartLine(ref GPartLine PL)
        {
            _head = PL._head;
            _tail = PL._tail;
            //Calculate Line Params
            _a = PL._a;
            _b = PL._b;
            _c = PL._c;
        }
        private void CalculateLineParameters()
        {
            _a = _tail.Y - _head.Y;
            _b = _head.X - _tail.X;
            _c = -(_a * _head.X + _b * _tail.Y);
        }
        public float Distance(GPosition2D P)
        {
            if ((P.Sub(_head)).InnerProduct(_tail.Sub(_head)) < 0)
                return (P.Sub(_head)).Size();
            if ((P.Sub(_tail)).InnerProduct(_head.Sub(_tail)) < 0)
                return (P.Sub(_tail)).Size();
            GLine tmp = new GLine(_head, _tail);
            return tmp.Distance(P);
        }
        public float DistanceFromLine(GPosition2D P)
        {
            GLine tmp = new GLine(_head, _tail);
            return (tmp).Distance(P);
        }
        public bool IsPointInRange(GPosition2D P)
        {
            if ((P.Sub(_head)).InnerProduct(_tail.Sub(_head)) < 0)
                return false;
            if ((P.Sub(_tail)).InnerProduct(_head.Sub(_tail)) < 0)
                return false;
            return true;
        }
        public static implicit operator CommonClasses.MathLibrary.PartLine(GPartLine GL)
        {
            return new CommonClasses.MathLibrary.PartLine((CommonClasses.MathLibrary.Position2D)GL._head, (CommonClasses.MathLibrary.Position2D)GL._tail);
        }
    
        public static implicit operator GPartLine(CommonClasses.MathLibrary.PartLine PL)
        {
            return new GPartLine((GPosition2D)PL.Head, (GPosition2D)PL.Tail);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GCircle
    {

        public GPosition2D Center;

        public float Radious;

        public GCircle(GPosition2D center, float radious)
        {
            Center = center;
            Radious = radious;
        }
        public GPosition2D Intersect(GLine l, out int count, ref GPosition2D intersect2)
        {
            GLine perp = l.PerpenducilarLineToPoint(Center);
            bool Hasvalue;
            GPosition2D perpfoot = l.IntersectWithLine(perp, out Hasvalue);
            float dist = perpfoot.Sub(Center).Size();
            GPosition2D intersect1 = new GPosition2D(0, 0);
            count = 0;
            if (dist < Radious)
            {
                float perpAngle = (perpfoot.Sub(Center)).AngleInRadians();
                float openingAngle = (float)Math.Acos(dist / Radious);
                GVector2D tmp = new GVector2D(0, 0);
                intersect1 = (Center.Add(tmp.FromAngleSize(perpAngle + openingAngle, Radious)));
                intersect2 = (Center.Add(tmp.FromAngleSize(perpAngle - openingAngle, Radious)));
                count = 2;
            }
            else if (dist == Radious)
            {
                count = 1;
                intersect1 = (perpfoot);
            }
            return intersect1;
        }
        public int GetTangent(GPosition2D P, out GLine TangentLine1, out GLine TangentLine2, out GPosition2D TangentPoint1, out GPosition2D TangentPoint2)
        {
            GVector2D vect = Center.Sub(P);
            float dist = vect.Size();
            TangentLine1 = new GLine(new GPosition2D(0, 0), new GPosition2D(0, 0));
            TangentLine2 = new GLine(new GPosition2D(0, 0), new GPosition2D(0, 0));
            TangentPoint1 = new GPosition2D(0, 0);
            TangentPoint2 = new GPosition2D(0, 0);
            if (dist >= Radious)
            {
                GLine l = new GLine(P, Center);
                if (dist == Radious)
                {
                    TangentPoint1 = P;
                    TangentLine1 = l.PerpenducilarLineToPoint(Center);
                    return 1;
                }
                else
                {
                    float lineAngle = vect.AngleInRadians();
                    float openingAngle = (float)Math.Asin(Radious / dist);
                    float tangentDist = (float)Math.Sqrt(dist * dist - Radious * Radious);
                    GVector2D tmp = new GVector2D(0, 0);
                    GVector2D v1 = tmp.FromAngleSize(lineAngle + openingAngle, tangentDist);
                    TangentLine1 = new GLine(P, P.Add(v1));
                    TangentPoint1 = P.Add(v1);

                    v1 = tmp.FromAngleSize(lineAngle - openingAngle, tangentDist);
                    TangentLine2 = new GLine(P, P.Add(v1));
                    TangentPoint2 = P.Add(v1);
                    return 2;
                }
            }
            else
                return 0;
        }

        public GPosition2D Intersect(GCircle circle, out int count, ref GPosition2D intersect2)
        {
            GPosition2D intersect1 = new GPosition2D(0, 0);
            float d = this.Center.DistanceFrom(circle.Center);
            count = 0;
            if ((d > this.Radious + circle.Radious) || (d < Math.Abs(this.Radious - circle.Radious)))
            {
                return new GPosition2D(0, 0);
            }
            else if (d == 0 && this.Radious == circle.Radious)
            {

                return new GPosition2D(0, 0);
            }
            else
            {
                float rA = this.Radious;
                float rB = circle.Radious;
                float xA = this.Center.X;
                float yA = this.Center.Y;
                float xB = circle.Center.X;
                float yB = circle.Center.Y;


                float K = (1 / 4.0f) * (float)Math.Sqrt(((rA + rB) * (rA + rB) - d * d) * (d * d - (rA - rB) * (rA - rB)));
                float X1 = (1 / 2.0f) * (xB + xA) + (1 / 2.0f) * (xB - xA) * (rA * rA - rB * rB) / (d * d) + 2 * (yB - yA) * K / (d * d);
                float Y1 = (1 / 2.0f) * (yB + yA) + (1 / 2.0f) * (yB - yA) * (rA * rA - rB * rB) / (d * d) - 2 * (xB - xA) * K / (d * d);

                float X2 = (1 / 2.0f) * (xB + xA) + (1 / 2.0f) * (xB - xA) * (rA * rA - rB * rB) / (d * d) - 2 * (yB - yA) * K / (d * d);
                float Y2 = (1 / 2.0f) * (yB + yA) + (1 / 2.0f) * (yB - yA) * (rA * rA - rB * rB) / (d * d) + 2 * (xB - xA) * K / (d * d);
                count = 2;
                intersect1 = new GPosition2D(X1, Y1);
                intersect2 = new GPosition2D(X2, Y2);
            }
            return intersect1;
        }
        public bool IsInCircle(GPosition2D P)
        {
            if (Center.DistanceFrom(P) < Radious)
                return true;
            return false;
        }
      
       
        public static implicit operator CommonClasses.MathLibrary.Circle(GCircle GC)
        {
            return new CommonClasses.MathLibrary.Circle(GC.Center, GC.Radious);
        }
 
        public static implicit operator GCircle(CommonClasses.MathLibrary.Circle C)
        {
            return new GCircle(C.Center, (float)C.Radious);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct GObjectState
    {
        public GPosition2D Location;
        public GVector2D Speed;
        public GObjectType Type;
        public GVector2D Acceleration;
        //public float Angle;
        //public float AngularSpeed;
        public GObjectState(ref GObjectState From)
        {
            Type = From.Type;
            Location = From.Location;
            Speed = From.Speed;
            Acceleration = From.Acceleration;
            //Angle = From.Angle;
            //AngularSpeed = From.AngularSpeed;
        }
        public GObjectState(GObjectType type, GPosition2D location, GVector2D speed, GVector2D acceleration/*, float angle, float angularSpeed*/)
        {
            Type = type;
            Location = location;
            Speed = speed;
            Acceleration = acceleration;
            //Angle = angle;
            //AngularSpeed = angularSpeed;
        }
        public GObjectState(GPosition2D location, GVector2D speed/*, float angle*/)
        {
            Location = location;
            Speed = speed;
            //Angle = angle;
            //AngularSpeed = 0;
            Acceleration = new GVector2D(0, 0);
            Type = GObjectType.OurRobot;
        }
        
        public static implicit operator GameDefinitions.SingleObjectState(GObjectState GO)
        {
            return new GameDefinitions.SingleObjectState((GameDefinitions.ObjectType)GO.Type, (CommonClasses.MathLibrary.Position2D)GO.Location
                , (CommonClasses.MathLibrary.Vector2D)GO.Speed, (CommonClasses.MathLibrary.Vector2D)GO.Acceleration, 0, 0);
        }
        
        public static implicit operator GObjectState(GameDefinitions.SingleObjectState O)
        {
            return new GObjectState((GObjectType)O.Type, (GPosition2D)O.Location, (GVector2D)O.Speed, (GVector2D)O.Acceleration);
        }

    }
  
    public struct Interval
    {

        public float Start;
        public float End;

        public Interval(float start, float end)
        {
            if (start < end)
            {
                Start = start;
                End = end;
            }
            else
            {
                End = start;
                Start = end;
            }
        }

        public float Length()
        {
            return End - Start;
        }

        public bool Contains(float p)
        {
            return p >= Start && p <= End;
        }

    }
    public struct VisibleGoalInterval
    {
        public Interval interval;
        public float ViasibleWidth;

        public VisibleGoalInterval(Interval _interval, float viasibleWidth)
        {
            interval = new Interval(_interval.Start, _interval.End);
            ViasibleWidth = viasibleWidth;
        }
    }
}

