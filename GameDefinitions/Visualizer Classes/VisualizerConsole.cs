using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace MRL.SSL.GameDefinitions
{
    public class ConsoleObject : INotifyPropertyChanged
    {
        public Color ForgroundColor { get; set; }
        public string Content { get; set; }
        public string ControlChar { get; set; }
        public string ToRealText()
        {
            return Content + ControlChar;
        }

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public static class VisualizerConsole
    {
        public static List<ConsoleObject> Data = new List<ConsoleObject>();
        
        public static void WriteLine(string content)
        {
            ConsoleObject co = new ConsoleObject()
            {
                Content = content,
                ControlChar = "\n",
                ForgroundColor = Color.White,
            };
            Data.Add(co);
        }

        public static void WriteLine(string content,Color color)
        {
            ConsoleObject co = new ConsoleObject()
            {
                Content = content,
                ControlChar = "\n",
                ForgroundColor = color,
            };
            Data.Add(co);
        }

        public static void Write(string content)
        {
            ConsoleObject co = new ConsoleObject()
            {
                Content = content,
                ControlChar = "",
                ForgroundColor = Color.Black,
            };
            Data.Add(co);
        }

        public static void Write(string content, Color color)
        {
            ConsoleObject co = new ConsoleObject()
            {
                Content = content,
                ControlChar = "",
                ForgroundColor = color,
            };
            Data.Add(co);
        }

      

    }
}
