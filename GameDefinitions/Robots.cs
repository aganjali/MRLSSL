using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MRL.SSL.GameDefinitions
{
    public class Robots
    {
        private Color _color;
        /// <summary>
        /// 
        /// </summary>
        public Color TeamColor
        {
            get { return _color; }
            set { _color = value; }
        }
        private int _id;
        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private int _engineId;
        /// <summary>
        /// 
        /// </summary>
        public int EngineId
        {
            get { return _engineId; }
            set { _engineId = value; }
        }
    }
}
