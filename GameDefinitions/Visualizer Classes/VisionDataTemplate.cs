using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions.Visualizer_Classes
{
    public class VisionDataTemplate
    {
        Dictionary<int, Position2D> cam = new Dictionary<int, Position2D>();

        public Dictionary<int, Position2D> Cam
        {
            get { return cam; }
            set { cam = value; }
        }
        public string xReal { get; set; }
        public string yReal { get; set; }
        public string xCam0 { get; set; }
        public string yCam0 { get; set; }
        public string xCam1 { get; set; }
        public string yCam1 { get; set; }
        public string xCam2 { get; set; }
        public string yCam2 { get; set; }
        public string xCam3 { get; set; }
        public string yCam3 { get; set; }

        public void MakeList()
        {
            cam = new Dictionary<int, Position2D>();
            if (xCam0 != "" && yCam0 != "")
                cam.Add(0, new Position2D(double.Parse(xCam0), double.Parse(yCam0)));
            if (xCam1 != "" && yCam1 != "")
                cam.Add(1, new Position2D(double.Parse(xCam1), double.Parse(yCam1)));
            if (xCam2 != "" && yCam2 != "")
                cam.Add(2, new Position2D(double.Parse(xCam2), double.Parse(yCam2)));
            if (xCam3 != "" && yCam3 != "")
                cam.Add(3, new Position2D(double.Parse(xCam3), double.Parse(yCam3)));
        }


        public VisionDataTemplate()
        {
            xReal = "0";
            yReal = "0";
            xCam0 = "";
            yCam0 = "";
            xCam1 = "";
            yCam1 = "";
            xCam2 = "";
            yCam2 = "";
            xCam3 = "";
            yCam3 = "";
        }

        public VisionDataTemplate(double? _xReal, double? _yReal, double? _xCam0,
            double? _yCam0, double? _xCam1, double? _yCam1, double? _xCam2, double? _yCam2, double? _xCam3, double? _yCam3)
        {
            xReal = _xReal.HasValue ? _xReal.Value.ToString() : "";
            yReal = _yReal.HasValue ? _yReal.Value.ToString() : "";
            xCam0 = _xCam0.HasValue ? _xCam0.Value.ToString() : "";
            yCam0 = _yCam0.HasValue ? _yCam0.Value.ToString() : "";
            xCam1 = _xCam1.HasValue ? _xCam1.Value.ToString() : "";
            yCam1 = _yCam1.HasValue ? _yCam1.Value.ToString() : "";
            xCam2 = _xCam2.HasValue ? _xCam2.Value.ToString() : "";
            yCam2 = _yCam2.HasValue ? _yCam2.Value.ToString() : "";
            xCam3 = _xCam3.HasValue ? _xCam3.Value.ToString() : "";
            yCam3 = _yCam3.HasValue ? _yCam3.Value.ToString() : "";
        }
    }
}
