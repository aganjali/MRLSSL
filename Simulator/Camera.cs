using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StillDesign.PhysX.MathPrimitives;
using System.Drawing;
using SlimDX.Direct3D10;
using SlimDX;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace Simulator
{
    public class Camera
    {
        
        private float _x, _y, _z;
        private RectangleF ViewPort;
        private List<SingleObjectState> ourRobots ;
        private int _id = 0;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public List<SingleObjectState> OurRobots
        {
            get { return ourRobots; }
            set { ourRobots = value; }
        }
        private List<SingleObjectState> oppRobots = new List<SingleObjectState>();

        public List<SingleObjectState> OppRobots
        {
            get { return oppRobots; }
            set { oppRobots = value; }
        }
        public float Z
        {
            get { return _z; }
        }

        public float Y
        {
            get { return _y; }
        }

        public float X
        {
            get { return _x; }
        }

        public Camera() { }
        public Camera(float x, float y, float z, RectangleF viewport, int cameraID)
        {
            _x = x;
            _y = y;
            _z = z;
            ViewPort = new RectangleF(viewport.X, viewport.Y, viewport.Width, viewport.Height);
            _id = cameraID;
        }

        public bool IsInCamera(StillDesign.PhysX.MathPrimitives.Vector3 objPosition)
        {
            float dx = ViewPort.X - objPosition.X;
            float dy = ViewPort.Y - objPosition.Y;

            return (dx < ViewPort.Width && dx >= 0) && (dy < ViewPort.Height && dy >= 0);
            
        }
        public StillDesign.PhysX.MathPrimitives.Vector3 CalculateObjectPosition(StillDesign.PhysX.MathPrimitives.Vector3 realObjPosition)
        {
            StillDesign.PhysX.MathPrimitives.Vector3 v = new StillDesign.PhysX.MathPrimitives.Vector3(realObjPosition.X - _x, realObjPosition.Y - _y, realObjPosition.Z - _z);
            StillDesign.PhysX.MathPrimitives.Vector3 tmp = new StillDesign.PhysX.MathPrimitives.Vector3();
            tmp.X = (realObjPosition.X - v.X * realObjPosition.Z) * 1000;
            tmp.Y = 0;
            tmp.Z = (realObjPosition.Y - v.Y * realObjPosition.Z) * 1000;
            return tmp;
        }
    }
}
