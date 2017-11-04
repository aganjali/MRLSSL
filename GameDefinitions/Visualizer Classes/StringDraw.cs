using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    public class StringDraw
    {
        public bool ColorChange { get; set; }

        public Position2D Posiotion { get; set; }

        public bool IsShown { get; set; }

        public Color TextColor { get; set; }

        public string Content { get; set; }

        public string Key { get; set; }

        public bool OnTop { get; set; }

        public StringDraw()
        {
            Posiotion = new Position2D();
            Content = "Empty";
            Key = "Empty";
            TextColor = Color.Black;
            ColorChange = true;
        }

        public StringDraw(string content, string key, Color color, Position2D posiotion)
        {
            Posiotion = posiotion;
            TextColor = color;
            Key = key;
            Content = content;
            IsShown = true;
            ColorChange = true;
        }

        public StringDraw(string content, string key, Color color, Position2D posiotion, bool isChecked)
        {
            Posiotion = posiotion;
            TextColor = color;
            Key = key;
            Content = content;
            IsShown = isChecked;
            ColorChange = true;
        }

        public StringDraw(string content, string key, Position2D posiotion)
        {
            Posiotion = posiotion;
            TextColor = Color.Black;
            Key = key;
            Content = content;
            IsShown = false;
            ColorChange = true;
        }

        public StringDraw(string content, Position2D posiotion)
        {
            Posiotion = posiotion;
            TextColor = Color.Black;
            Key = content;
            Content = content;
            IsShown = false;
            ColorChange = true;
        }

        public StringDraw(string content, Position2D posiotion,bool isChecked)
        {
            Posiotion = posiotion;
            TextColor = Color.Black;
            Key = content;
            Content = content;
            IsShown = isChecked;
            ColorChange = true;
        }

        public StringDraw(string content, Color color, Position2D posiotion)
        {
            Posiotion = posiotion;
            TextColor = color;
            Key = content;
            Content = content;
            IsShown = false;
            ColorChange = true;
        }

        public StringDraw(string content, Color color, Position2D posiotion,bool isChecked)
        {
            Posiotion = posiotion;
            TextColor = color;
            Key = content;
            Content = content;
            IsShown = isChecked;
            ColorChange = true;
        }

    }
}
