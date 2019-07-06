
///////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Diagnostics;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Runtime.InteropServices;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
	public class HiddenBallGuesser
	{
        public bool isPointinShadow = false;
        DrawCollection ShaddowGroup = new DrawCollection();
        double ORDiameter = RobotParameters.OurRobotParams.Diameter;//0.18;
        double ORHeight = RobotParameters.OurRobotParams.Height;//0.15;
        public bool DrawRegion = false;
		public List<CameraInfo> Cameras;
        Bitmap BMP = new Bitmap(700, 500, PixelFormat.Format24bppRgb);
        Graphics gr;
        public bool GuessHiddenBallPosition(WorldModel Model, PointF LastMouseLocation)
		{
            //SizeF FieldSize = new SizeF((float)Math.Abs(GameParameters.OppLeftCorner.X - GameParameters.OppRightCorner.X), (float)Math.Abs(GameParameters.OppLeftCorner.Y - GameParameters.OppRightCorner.Y));
            GraphicsPath gp = new GraphicsPath();
            RectangleF TotalFieldarea = new RectangleF(-4.5f - 0.5f, -3.0f - 0.5f, 10, 7);//new RectangleF((float)(GameParameters.OurGoalLeft.X - GameParameters.FieldMargins.X / 2), (float)(GameParameters.OurLeftCorner.Y - GameParameters.FieldMargins.Y / 2), (float)(Math.Abs(GameParameters.OurGoalLeft.X - GameParameters.OppGoalLeft.X) + GameParameters.FieldMargins.X), (float)(Math.Abs(GameParameters.OurRightCorner.Y - GameParameters.OurLeftCorner.Y) + GameParameters.FieldMargins.Y));
			Region finalRegion = null;

			List<SingleObjectState> obstacles = new List<SingleObjectState>();

            #region "Add Obstacles"
            if (Model.OurRobots != null)
            {
                foreach (int key in Model.OurRobots.Keys)
                {
                    obstacles.Add(Model.OurRobots[key]);
                }
            }
            if (Model.Opponents != null)
            {
                foreach (int key in Model.Opponents.Keys)
                {
                    obstacles.Add(Model.Opponents[key]);
                }
            }
            #endregion

            if (Cameras != null)
            {
                /////////////////////////////////////////////////////
                for (int i = 0; i < Cameras.Count; i++)
                {
                    CameraInfo cam = Cameras[i];
                    Region reg = new Region(cam.VisibleArea);
                    reg.Complement(TotalFieldarea);
                    foreach (SingleObjectState s in obstacles)
                    {
                        gp = new GraphicsPath();
                        Position2D shadowcenter = new Position2D();
                        double dx = (ORHeight) / (cam.Height - ORHeight) * Math.Abs(s.Location.X - cam.CenterLocation.X);
                        double dy = (ORHeight) / (cam.Height - ORHeight) * Math.Abs(s.Location.Y - cam.CenterLocation.Y);
                        if (s.Location.X >= cam.CenterLocation.X && s.Location.Y >=cam.CenterLocation.Y)
                        {
                            shadowcenter.X = s.Location.X + dx;
                            shadowcenter.Y = s.Location.Y + dy;
                        }
                        if (s.Location.X >= cam.CenterLocation.X && s.Location.Y <= cam.CenterLocation.Y)
                        {
                            shadowcenter.X = s.Location.X + dx;
                            shadowcenter.Y = s.Location.Y - dy;
                        }
                        if (s.Location.X <= cam.CenterLocation.X && s.Location.Y >= cam.CenterLocation.Y)
                        {
                            shadowcenter.X = s.Location.X - dx;
                            shadowcenter.Y = s.Location.Y + dy;
                        }
                        if (s.Location.X <= cam.CenterLocation.X && s.Location.Y <= cam.CenterLocation.Y)
                        {
                            shadowcenter.X = s.Location.X - dx;
                            shadowcenter.Y = s.Location.Y - dy;
                        }

                        float shadowdiameter = (float)(ORDiameter * cam.Height / (cam.Height - ORHeight));
                        gp.AddEllipse((float)(shadowcenter.X - shadowdiameter / 2),(float) (shadowcenter.Y - shadowdiameter / 2), shadowdiameter, shadowdiameter);
                        reg.Union(gp);
                        
                        gp = new GraphicsPath();
                        gp.AddEllipse((float)(s.Location.X - ORDiameter / 2), (float)(s.Location.Y - ORDiameter / 2), (float)(ORDiameter), (float)(ORDiameter));
                        reg.Exclude(gp);
                    }
                    if (finalRegion == null)
                        finalRegion = reg;
                    else
                        finalRegion.Intersect(reg);
                }
                #region "DrawShadow"
                if (finalRegion != null && DrawRegion == true)
                {
                    ShaddowGroup = new DrawCollection();
                    finalRegion.Transform(new Matrix(100f, 0f, 0f, 100f, 500f, 350f));
                    gr = Graphics.FromImage(BMP);
                    gr.Clear(Color.Black);
                    gr.FillRegion(Brushes.White, finalRegion);

                    byte[] buff = new byte[BMP.Width * BMP.Height * 4];
                    Rectangle rect = new Rectangle(0, 0, BMP.Width, BMP.Height);
                    BitmapData bmpData = BMP.LockBits(rect, ImageLockMode.ReadOnly, BMP.PixelFormat);
                    IntPtr pBits = bmpData.Scan0;
                    Marshal.Copy(pBits, buff, 0, bmpData.Stride * BMP.Height);

                    //ShaddowGroup.RegionPixelWidth = precision;
                    //if (ShaddowGroup.RegionToDraw != null) ShaddowGroup.RegionToDraw.Clear();
                    //for (int i = 0; i < BMP.Width; i += precision)
                    //{
                    //    for (int j = 0; j < BMP.Height; j += precision)
                    //    {
                    //        int counter_i = (j * bmpData.Stride) + (i * 3);
                    //        if (buff[counter_i] == 255)
                    //        {
                    //            ShaddowGroup.AddPermanently(new Point(i, j));
                    //        }
                    //    }
                    //}
                   DrawingObjects.AddObject("Shadow", ShaddowGroup);
                   finalRegion.Transform(new Matrix(0.01f, 0f, 0f, 0.01f, -5.0f, -3.5f));
                   BMP.UnlockBits(bmpData);
                }
                #endregion
                

                Matrix mtx = new Matrix();
                float scale = 30;
                mtx.Scale(scale, scale);
                RectangleF[] scans = finalRegion.GetRegionScans(mtx);
                for (int i = 0; i < scans.Length; i++)
                    scans[i] = new RectangleF(scans[i].X / scale, scans[i].Y / scale, scans[i].Width / scale, scans[i].Height / scale);
                /////////////////////////////////////////////////////
                //RegionScans = scans;
                if (scans.Length == 0)
                    return false;
                
                float bestdist = float.MaxValue;
                int bestind = -1;
                for (int i = 0; i < scans.Length; i++)
                {
                    float dist = PointToRectDistance(LastMouseLocation, scans[i]);
                    if (dist < bestdist)
                    {
                        bestdist = dist;
                        bestind = i;
                   }
                }
                if (bestdist < 0.09)
                {
                    isPointinShadow = true;
                }
                else
                {
                    isPointinShadow = false;
                }
                /////////////////////////////////////////////////////
                if (Model.BallState == null)
                    Model.BallState = new SingleObjectState();
                Model.BallState.Location = new PointF(scans[bestind].X + scans[bestind].Width / 2, scans[bestind].Y + scans[bestind].Height / 2);
                return true;
            }
            return false;
		}
		public float PointToRectDistance(PointF loc, RectangleF rect)
		{
			float xdis, ydis;
			if (loc.X < rect.X)
				xdis = rect.X - loc.X;
			else if (loc.X > rect.Right)
				xdis = loc.X - rect.Right;
			else
				xdis = 0;
			if (loc.Y < rect.Y)
				ydis = rect.Y - loc.Y;
			else if (loc.Y > rect.Bottom)
				ydis = loc.Y - rect.Bottom;
			else
				ydis = 0;
			return (float)Math.Sqrt(xdis * xdis + ydis * ydis);
		}
	}

	public struct CameraInfo
	{
		public float Height;
		public Position2D CenterLocation;
		public RectangleF VisibleArea;
	}
}