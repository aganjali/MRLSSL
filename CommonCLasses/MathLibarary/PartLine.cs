using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// Representing a Part Line
    /// </summary>
    public struct PartLine
    {
        private double _a, _b, _c;
        private Position2D _head, _tail;
        /// <summary>
        /// Tail of Part Line
        /// </summary>
        public Position2D Tail
        {
            get { return _tail; }
            set
            {
                _tail = value;
                CalculateLineParameters();
            }
        }
        /// <summary>
        /// Head of part Line
        /// </summary>
        public Position2D Head
        {
            get { return _head; }
            set
            {
                _head = value;
                CalculateLineParameters();
            }
        }
        /// <summary>
        /// C
        /// </summary>
        public double C
        {
            get { return _c; }
        }
        /// <summary>
        /// B
        /// </summary>
        public double B
        {
            get { return _b; }
        }
        /// <summary>
        /// A
        /// </summary>
        public double A
        {
            get { return _a; }
        }
        /// <summary>
        /// Angle of Part Line
        /// </summary>
        public double Angle
        {
            get { return Math.Atan2(_b, _a); }
        }
        /// <summary>
        /// part Line between 2 positions
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        public PartLine(Position2D P1, Position2D P2)
        {
            this._head = P1;
            this._tail = P2;
            //Calculate Line Params
            this._a = P2.Y - P1.Y;
            this._b = P1.X - P2.X;
            this._c = -(_a * P1.X + _b * P1.Y);
        }
        /// <summary>
        /// Calculating the Line parameter a,b,c
        /// </summary>
        private void CalculateLineParameters()
        {
            this._a = _tail.Y - _head.Y;
            this._b = _head.X - _tail.X;
            this._c = -(_a * _head.X + _b * _tail.Y);
        }
        /// <summary>
        /// Distanse of Part Line and a Position
        /// </summary>
        /// <param name="P">Target POsition</param>
        /// <returns>Distance</returns>
        public double Distance(Position2D P)
        {
            if ((P - _head).InnerProduct(_tail - _head) < 0)
                return (P - _head).Size;
            if ((P - _tail).InnerProduct(_head - _tail) < 0)
                return (P - _tail).Size;
            return ((Line)this).Distance(P);
        }
        /// <summary>
        /// check if there is an intersection between the Perpendicular Line from a position with the PartLine
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public bool IntersectsWithPerpendicularFrom(Position2D P)
        {
            return (((P - _head).InnerProduct(_tail - _head) >= 0) && ((P - _tail).InnerProduct(_head - _tail) >= 0));
        }
        /// <summary>
        /// Line Distance
        /// </summary>
        /// <param name="P">Target Position</param>
        /// <returns>Distance</returns>
        public double DistanceFromLine(Position2D P)
        {
            return ((Line)this).Distance(P);
        }

        /// <summary>
        /// Determines if the triangle made by P, Head and Tail has no wide angle
        /// </summary>
        /// <param name="P">The point to speculate</param>
        /// <returns>If the triangle made by P, Head and Tail has no wide angle</returns>
        public bool IsPointInRange(Position2D P)
        {
            if ((P - _head).InnerProduct(_tail - _head) < 0)
                return false;
            if ((P - _tail).InnerProduct(_head - _tail) < 0)
                return false;
            return true;
        }
        /// <summary>
        /// Perpendicular line of a position
        /// </summary>
        /// <param name="From">target position</param>
        /// <returns>Perpendicular line of the Position</returns>
        public Line PerpenducilarLineToPoint(Position2D From)
        {
            return new Line(_b, -_a, -(_b * From.X - _a * From.Y));
        }
        /// <summary>
        /// Calculate intersection of 2 partline
        /// </summary>
        /// <param name="Pl">Target partLin</param>
        /// <returns>if rhere is any intersection return the position of it</returns>
        public Position2D? IntersectWithPartLine(PartLine Pl)
        {
            Line l1 = this, l2 = Pl;
            Position2D? intersection = l1.IntersectWithLine(l2);
            if (!intersection.HasValue)
                return null;
            if ((intersection.Value - _head).InnerProduct(_tail - _head) < 0)
                return null;
            if ((intersection.Value - _tail).InnerProduct(_head - _tail) < 0)
                return null;
            if ((intersection.Value - Pl._head).InnerProduct(Pl._tail - Pl._head) < 0)
                return null;
            if ((intersection.Value - Pl._tail).InnerProduct(Pl._head - Pl._tail) < 0)
                return null;
            return intersection;
        }
        /// <summary>
        /// Castable to a line
        /// </summary>
        /// <param name="pl">Target</param>
        /// <returns>Casting to a Line</returns>
        public static implicit operator Line(PartLine pl)
        {
            return new Line(pl._a, pl._b, pl._c);
        }
        /// <summary>
        /// String of a PartLine
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", _head, _tail);
        }

    }
}
