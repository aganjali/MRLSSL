using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.IO;
using Enterprise;

namespace MRL.SSL.GameDefinitions
{

    public class ControlData
    {
        public static bool SaveData = false;
        public enum Export
        {
            excel,
            text
        }
        internal class Temp
        {
            public Vector2D VCe { get; set; }
            public double WC { get; set; }
            public Vector2D VRe { get; set; }
            public double WR { get; set; }
            public Vector2D VCr { get; set; }
            public Vector2D VRr { get; set; }

            public Vector2D VCbe { get; set; }
            public Vector2D VCbr { get; set; }
            public double Wcb { get; set; }

            public double Mtime { get; set; }
            public double Vtime { get; set; }
        }

        static List<Temp> data = new List<Temp>();

        public static void AddData(Vector2D vc, double wc, Vector2D vr, double wr, double t_model, double t_vision)
        {
            data.Add(new Temp() { Mtime = t_model, Vtime = t_vision, VCe = vc, VRe = vr, WC = wc, WR = wr });
        }

        public static void AddData(Vector2D vce, double wc, Vector2D vre, double wr, Vector2D vcr, Vector2D vrr, double t_model, double t_vision)
        {
            data.Add(new Temp() { Mtime = t_model, Vtime = t_vision, VCe = vce, VRe = vre, WC = wc, WR = wr, VCr = vcr, VRr = vrr });
        }


        public static void AddData(Vector2D vce, double wc, Vector2D vre, double wr, Vector2D vcr, Vector2D vrr, Vector2D vcbe, Vector2D vcbr, double t_model, double t_vision)
        {
            data.Add(new Temp() { Mtime = t_model, Vtime = t_vision, VCe = vce, VRe = vre, WC = wc, WR = wr, VCr = vcr, VRr = vrr, VCbe = vcbe, VCbr = vcbr });
        }

        public static void Clear()
        {
            data.Clear();
        }

        private bool isInUse(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Write(LogType.Exception, ex);
                    return true;
                }
            }
            return false;
        }

        public static void ExportTo(string p, Export mode)
        {
            string listSeparator = "";
            if (mode == Export.excel)
                listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            else
                listSeparator = "\t";
            StringBuilder cvsStringBuilder = new StringBuilder();

            cvsStringBuilder.AppendFormat("VXrR{0}VYrR{0}Wr{0}VXcR{0}VYcR{0}Wc{0}VXrE{0}VYrE{0}VXcE{0}VYcE{0}VCbR{0}VCbE{0}WCb\r\n", listSeparator);
            foreach (var item in data)
            {
                cvsStringBuilder.AppendFormat("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}", listSeparator,
                    item.VRr.X.ToString("f3"),
                    item.VRr.Y.ToString("f3"),
                    item.WR.ToString("f3"),
                    item.VCr.X.ToString("f3"),
                    item.VCr.Y.ToString("f3"),
                    item.WC.ToString("f3"),
                    item.VRe.X.ToString("f3"),
                    item.VRe.Y.ToString("f3"),
                    item.VCe.X.ToString("f3"),
                    item.VCe.Y.ToString("f3"),
                    item.VCbr.Y.ToString("f3"),
                    item.VCbe.Y.ToString("f3"),
                    item.Wcb.ToString("f3"),
                cvsStringBuilder.Append("\r\n"));
            }

            var writer = new StreamWriter(p);
            writer.Write(cvsStringBuilder.ToString());
            writer.Close();
        }
    }
}
