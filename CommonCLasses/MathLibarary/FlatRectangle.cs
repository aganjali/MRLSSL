using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MRL.SSL.CommonClasses.MathLibrary
{
    /// <summary>
    /// Representing the flat rectangle with a leftTop position and a size
    /// </summary>
    public class FlatRectangle
    {
        public bool PenIsChanged { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private bool _isFill;
        public bool IsFill
        {
            get { return _isFill; }
            set { _isFill = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public float Opacity { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public bool IsShown { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Color BorderColor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Color FillColor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float BorderWidth { get; set; }
        Position2D _topLeft;
        /// <summary>
        /// Top Left Positioon
        /// </summary>
        public Position2D TopLeft
        {
            get { return _topLeft; }
            set { _topLeft = value; }
        }
        Vector2D _size;
        /// <summary>
        /// Size of Rectangle
        /// </summary>
        public Vector2D Size
        {
            get { return _size; }
            set { _size = value; }
        }
        /// <summary>
        /// Top
        /// </summary>
        public double Top
        {
            get { return _topLeft.Y; }
        }
        /// <summary>
        /// Left
        /// </summary>
        public double Left
        {
            get { return _topLeft.X; }
        }
        /// <summary>
        /// topleft.y
        /// </summary>
        public double Y
        {
            get { return _topLeft.Y; }
        }
        /// <summary>
        /// topleft.x
        /// </summary>
        public double X
        {
            get { return _topLeft.X; }
        }
        /// <summary>
        /// width of rectangle size.x
        /// </summary>
        public double Width
        {
            get
            {
                return _size.X;
            }
        }
        /// <summary>
        /// height of rectangle size.y
        /// </summary>
        public double Height
        {
            get
            {
                return _size.Y;
            }
        }
        /// <summary>
        /// Bottomright corner of the rectangle
        /// </summary>
        public Position2D BottomRight
        {
            get { return _topLeft + _size; }
        }
        public FlatRectangle()
        {
        }
        /// <summary>
        /// representing With Position
        /// </summary>
        /// <param name="topLeft">LeftTop Position of FlatRectangle</param>
        /// <param name="size">size of it</param>
        public FlatRectangle(Position2D topLeft, Vector2D size)
        {
            _topLeft = topLeft;
            _size = size;
            BorderColor = Color.Black;
            Opacity = 1f;
            IsShown = true;
            IsFill = false;
            FillColor = Color.Transparent;
            BorderWidth = 0.01f;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        /// <param name="borderColor"></param>
        /// <param name="fillColor"></param>
        /// <param name="borderWidth"></param>
        /// <param name="opacity"></param>
        /// <param name="isShown"></param>
        public FlatRectangle(Position2D topLeft, Vector2D size,Color borderColor, Color fillColor,float borderWidth, float opacity, bool isShown)
        {
            _topLeft = topLeft;
            _size = size;
            BorderColor = borderColor;
            Opacity = opacity;
            IsShown = isShown;
            IsFill = true;
            FillColor = fillColor;
            BorderWidth = borderWidth;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        /// <param name="borderColor"></param>
        /// <param name="fillColor"></param>
        public FlatRectangle(Position2D topLeft, Vector2D size, Color borderColor, Color fillColor)
        {
            _topLeft = topLeft;
            _size = size;
            BorderColor = borderColor;
            Opacity = 1f;
            IsShown = true;
            IsFill = true;
            FillColor = fillColor;
            BorderWidth = 0.01f;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        public FlatRectangle(Position2D topLeft, Vector2D size, Color fillColor)
        {
            _topLeft = topLeft;
            _size = size;
            BorderColor = Color.Transparent;
            Opacity = 1f;
            IsShown = true;
            IsFill = true;
            FillColor = fillColor;
            BorderWidth = 0.01f;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        public FlatRectangle(Position2D topLeft, Vector2D size, Color fillColor,float opacity)
        {
            _topLeft = topLeft;
            _size = size;
            BorderColor = Color.Transparent;
            Opacity = opacity;
            IsShown = true;
            PenIsChanged = true;
            IsFill = true;
            FillColor = fillColor;
            BorderWidth = 0.01f;
        }
        /// <summary>
        /// representing With left,toop,width,height
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public FlatRectangle(double left, double top, double width, double height)
        {
            _topLeft = new Position2D(left, top);
            _size = new Vector2D(width, height);
            BorderColor = Color.Black;
            Opacity = 1f;
            IsShown = true;
            IsFill = false;
            FillColor = Color.Transparent;
            BorderWidth = 0.01f;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderColor"></param>
        /// <param name="fillColor"></param>
        /// <param name="opacity"></param>
        /// <param name="isShown"></param>
        public FlatRectangle(double left, double top, double width, double height, Color borderColor, Color fillColor,float borderWidth, float opacity, bool isShown)
        {
            _topLeft = new Position2D(left, top);
            _size = new Vector2D(width, height);
            BorderColor = borderColor;
            FillColor = fillColor;
            Opacity = opacity;
            IsShown = isShown;
            IsFill = true;
            BorderWidth = borderWidth;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderColor"></param>
        /// <param name="fillColor"></param>
        /// <param name="opacity"></param>
        public FlatRectangle(double left, double top, double width, double height, Color borderColor, Color fillColor, float borderWidth, float opacity)
        {
            _topLeft = new Position2D(left, top);
            _size = new Vector2D(width, height);
            BorderColor = borderColor;
            FillColor = fillColor;
            Opacity = opacity;
            IsShown = true;
            IsFill = true;
            BorderWidth = borderWidth;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderColor"></param>
        /// <param name="fillColor"></param>
        public FlatRectangle(double left, double top, double width, double height, Color borderColor, Color fillColor, float borderWidth)
        {
            _topLeft = new Position2D(left, top);
            _size = new Vector2D(width, height);
            BorderColor = borderColor;
            FillColor = fillColor;
            Opacity = 1f;
            IsShown = true;
            IsFill = true;
            BorderWidth = borderWidth;
            PenIsChanged = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderColor"></param>
        public FlatRectangle(double left, double top, double width, double height, Color borderColor)
        {
            _topLeft = new Position2D(left, top);
            _size = new Vector2D(width, height);
            BorderColor = borderColor;
            FillColor = Color.Transparent;
            Opacity = 1f;
            IsShown = true;
            IsFill = true;
            BorderWidth = 0.01f;
            PenIsChanged = true;
        }

      
    }
}
