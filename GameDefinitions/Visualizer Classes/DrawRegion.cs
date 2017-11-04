using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonCLasses.MathLibarary;

namespace MRL.SSL.GameDefinitions
{
    public class DrawRegion
    {
        public float Opacity { get; set; }
        public bool Filled { get; set; }
        public bool IsShown { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public List<Position2D> Path { get; set; }

        public DrawRegion()
        {
            Path = new List<Position2D>();
        }

        public DrawRegion(List<Position2D> path, bool filled, bool isShown, Color fillColor, Color borderColor)
        {
            Opacity = 1;
            Path = path;
            Filled = filled;
            IsShown = isShown;
            FillColor = fillColor;
            BorderColor = borderColor;
        }

        public DrawRegion(List<Position2D> path, bool isShown, bool filled, Color Color, Color borderColor, float opacity)
        {
            Opacity = opacity;
            Path = path;
            Filled = filled;
            IsShown = isShown;
            FillColor = Color;
            BorderColor = borderColor;
        }

        public DrawRegion(List<Position2D> path, bool isShown, Color Color, float opacity)
        {
            Opacity = opacity;
            Path = path;
            Filled = true;
            IsShown = isShown;
            FillColor = Color;
            BorderColor = Color.Transparent;
        }

        public DrawRegion(List<Position2D> path, bool filled, bool isShown, Color color)
        {
            Opacity = 1;
            Path = path;
            Filled = filled;
            IsShown = isShown;
            FillColor = color;
            BorderColor = color;
        }

        public DrawRegion(List<Position2D> path, Color color)
        {
            Opacity = 1;
            Path = path;
            Filled = true;
            IsShown = false;
            FillColor = color;
            BorderColor = color;
        }

    }

    public class DrawRegion3D
    {
        public float Opacity { get; set; }
        public bool Filled { get; set; }
        public bool IsShown { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public List<Position3D> Path { get; set; }

        public DrawRegion3D()
        {
            Path = new List<Position3D>();
        }

        public DrawRegion3D(List<Position3D> path, bool filled, bool isShown, Color fillColor, Color borderColor)
        {
            Opacity = 1;
            Path = path;
            Filled = filled;
            IsShown = isShown;
            FillColor = fillColor;
            BorderColor = borderColor;
        }

        public DrawRegion3D(List<Position3D> path, bool isShown, bool filled, Color Color, Color borderColor, float opacity)
        {
            Opacity = opacity;
            Path = path;
            Filled = filled;
            IsShown = isShown;
            FillColor = Color;
            BorderColor = borderColor;
        }

        public DrawRegion3D(List<Position3D> path, bool isShown, Color Color, float opacity)
        {
            Opacity = opacity;
            Path = path;
            Filled = true;
            IsShown = isShown;
            FillColor = Color;
            BorderColor = Color.Transparent;
        }

        public DrawRegion3D(List<Position3D> path, bool filled, bool isShown, Color color)
        {
            Opacity = 1;
            Path = path;
            Filled = filled;
            IsShown = isShown;
            FillColor = color;
            BorderColor = color;
        }

        public DrawRegion3D(List<Position3D> path, Color color)
        {
            Opacity = 1;
            Path = path;
            Filled = true;
            IsShown = false;
            FillColor = color;
            BorderColor = color;
        }

    }
}
