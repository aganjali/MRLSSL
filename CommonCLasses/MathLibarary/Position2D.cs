using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.WindowsAPICodePack.DirectX.Direct2D1;
using System.Xml.Serialization;
using System.Drawing;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// MRL.SSL Positions will represent by Position2D
    /// </summary>
    [XmlRoot("position2D")]
    public struct Position2D : IXmlSerializable
    {
       
        
        public static Position2D Average(Position2D p1,Position2D p2)
        {
            double x = (p1.X + p2.X) / 2;
            double y = (p1.Y + p2.Y) / 2;
            return new Position2D(x, y);
        }

        public Position2D Reverse()
        {
            return new Position2D(-this._x,- this._y);
        }
        /// <summary>
        /// Represent a 2D point with X and Y
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public Position2D(double X, double Y)
        {
            _x = X;
            _y = Y;
            _isShown = false;
            _drawColor = Color.Black;
        }
        Color _drawColor;
        public Color DrawColor
        {
            get { return _drawColor; }
            set { _drawColor = value; }
        }
        private bool _isShown ;
        public bool IsShown
        {
            get
            {
                return _isShown;
            }
            set
            {
                _isShown = value;
            }
        }
        double _x, _y;
        /// <summary>
        /// Represent X axis
        /// </summary>
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }
        /// <summary>
        /// Represent Y axis
        /// </summary>
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }
        /// <summary>
        /// The size of a Position from center
        /// </summary>
        public double Size
        {
            get { return Math.Sqrt(_x * _x + _y * _y); }
        }
        /// <summary>
        /// Square Size of a Position
        /// </summary>
        public double SquareSize
        {
            get { return _x * _x + _y * _y; }
        }
        /// <summary>
        /// Zero Position X = 0 and Y = 0 
        /// </summary>
        public static Position2D Zero
        {
            get { return new Position2D(0, 0); }

        }
        /// <summary>
        /// castable to a point
        /// </summary>
        /// <param name="V"></param>
        /// <returns>return a Point from Position </returns>
        public static implicit operator System.Drawing.PointF(Position2D V)
        {
            return new System.Drawing.PointF((float)V._x, (float)V._y);
        }
        /// <summary>
        /// castable to a PointF 
        /// </summary>
        /// <param name="V"></param>
        /// <returns>return a PointF from position</returns>
        public static implicit operator System.Windows.Point(Position2D V)
        {
            return new System.Windows.Point((float)V._x, (float)V._y);
        }
        ///// <summary>
        ///// castable to a Point2F 
        ///// </summary>
        ///// <param name="V"></param>
        ///// <returns>return a PointF from position</returns>
        //public static implicit operator Point2F(Position2D V)
        //{
        //    return new Point2F((float)V._x, (float)V._y);
        //}
        /// <summary>
        /// castable from a PointF
        /// </summary>
        /// <param name="V"></param>
        /// <returns>return a Position2D from a PointF</returns>
        public static implicit operator Position2D(System.Drawing.PointF V)
        {
            return new Position2D(V.X, V.Y);
        }
        /// <summary>
        /// castable from a Point
        /// </summary>
        /// <param name="V"></param>
        /// <returns>return a Position2D from a Point</returns>
        public static implicit operator Position2D(System.Windows.Point V)
        {
            return new Position2D(V.X, V.Y);
        }
        /// <summary>
        /// Operate a - betwint 2 Position2D
        /// </summary>
        /// <param name="Left">1st parameter of -</param>
        /// <param name="Right">2nd parameter of -</param>
        /// <returns>Vector2D between 2 Position2D</returns>
        public static Vector2D operator -(Position2D Left, Position2D Right)
        {
            return new Vector2D(Left._x - Right._x, Left._y - Right._y);
        }
        /// <summary>
        /// Operate a + betwint 2 Position2D
        /// </summary>
        /// <param name="Left">1st parameter of +</param>
        /// <param name="Right">2nd parameter of +</param>
        /// <returns>Position2D of summation</returns>
        public static Position2D operator +(Position2D P, Vector2D V)
        {
            return new Position2D(P._x + V.X, P._y + V.Y);
        }
        /// <summary>
        /// Operate a - betwint a Position2D and a Vector2D
        /// </summary>
        /// <param name="Left">1st parameter of -</param>
        /// <param name="Right">2nd parameter of -</param>
        /// <returns>moving in - direction a Position2D by a Vector2D to a new Position2D</returns>
        public static Position2D operator -(Position2D P, Vector2D V)
        {
            return new Position2D(P._x - V.X, P._y - V.Y);
        }
        public static Position2D operator -(Position2D P)
        {
            return new Position2D(-P._x, -P._y);
        }
        /// <summary>
        /// Operate a + betwint a Position2D and a Vector2D
        /// </summary>
        /// <param name="Left">1st parameter of -</param>
        /// <param name="Right">2nd parameter of -</param>
        /// <returns>moving in + direction a Position2D by a Vector2D to a new Position2D</returns>
        public static Position2D operator +(Vector2D V, Position2D P)
        {
            return new Position2D(P._x + V.X, P._y + V.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale">Double variable that a Position has to scale </param>
        /// <param name="P">the Position2D</param>
        /// <returns>Position2D that has been scaled</returns>
        public static Position2D operator *(double scale, Position2D P)
        {
            return new Position2D(P._x * scale, P._y * scale);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="P">the Position2D</param>
        /// <param name="scale">Double variable that a Position has to scale </param>
        /// <returns>Position2D that has been scaled</returns>
        public static Position2D operator *(Position2D P, double scale)
        {
            return new Position2D(P._x * scale, P._y * scale);
        }
        /// <summary>
        /// checking equality of 2 Postion2D
        /// </summary>
        /// <param name="Left">1st Position</param>
        /// <param name="Right">2nd Position</param>
        /// <returns>True if they are equal</returns>
        public static bool operator ==(Position2D Left, Position2D Right)
        {
            return Math.Abs(Left._x - Right._x) < 0.00001 && Math.Abs(Left._y - Right._y) < 0.00001;
        }
        /// <summary>
        /// checking NOT equality of 2 Postion2D
        /// </summary>
        /// <param name="Left">1st Position</param>
        /// <param name="Right">2nd Position</param>
        /// <returns>True if they are NOT equal</returns>
        public static bool operator !=(Position2D Left, Position2D Right)
        {
            return Math.Abs(Left._x - Right._x) >= 0.00001 || Math.Abs(Left._y - Right._y) >= 0.00001;
        }
        /// <summary>
        /// Calculates a point lying on the part-line connecting Start and End
        /// </summary>
        /// <param name="Start">The Start Point</param>
        /// <param name="End">The End Point</param>
        /// <param name="Amount">The control parameter, 0 returns Start, 1 returns End, 0.5 the midpoint between them</param>
        /// <returns>a point lying on the part-line connecting Start and End</returns>
        public static Position2D Interpolate(Position2D Start, Position2D End, double Amount)
        {
            return new Position2D(Start._x * (1 - Amount) + End._x * Amount, Start._y * (1 - Amount) + End._y * Amount);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// String Format of Position2D
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0},{1})", _x, _y);
        }

        public string toString()
        {
            return string.Format("({0},{1})", _x.ToString("f3"), _y.ToString("f3"));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position2D))
            {
                return false;
            }
            Position2D tf = (Position2D)obj;
            return (((tf.X == this.X) && (tf.Y == this.Y)) && tf.GetType().Equals(base.GetType()));
        }
        /// <summary>
        /// Gets the Distance From a Position2D
        /// </summary>
        /// <param name="From">Target Position2D</param>
        /// <returns>Distance from the target Position2D</returns>
        public double DistanceFrom(Position2D From)
        {
            return Math.Sqrt((this.X - From.X) * (this.X - From.X) + (this.Y - From.Y) * (this.Y - From.Y));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Position2D Extend(double x, double y)
        {
            return new Position2D(X + x, Y + y);
        }
        
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer Reader = new XmlSerializer(typeof(double));
            
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;

            reader.ReadStartElement("X");
            _x = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("Y");
            _y = (double)Reader.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer Raeder = new XmlSerializer(typeof(double));

            writer.WriteStartElement("X");
            Raeder.Serialize(writer, _x);
            writer.WriteEndElement();

            writer.WriteStartElement("Y");
            Raeder.Serialize(writer, _y);
            writer.WriteEndElement();
        }

        #endregion


        public static bool IsBetween(Position2D right, Position2D left, Position2D p)
        {
            return (right - left).InnerProduct(p - left) >= 0 && (left - right).InnerProduct(p - right) >= 0;
        }
    }

}
