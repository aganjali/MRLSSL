using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
using System.Xml;
using System.IO;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class LookupTable
    {
        private static Dictionary<int, double[]> ChipLookupWithSpin = new Dictionary<int, double[]>();
        private static Dictionary<int, double[]> ChipLookupWithoutSpin = new Dictionary<int, double[]>();

        public static SerializableDictionary<int, MetricChipKick> Data = new SerializableDictionary<int, MetricChipKick>();

        public static void Save()
        {
            string fileName = "LookupTable.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.WriteStartElement("LookupTable");

                writer.WriteStartElement("Count");
                writer.WriteValue(Data.Count);
                writer.WriteEndElement();
                writer.WriteStartElement("alaki");
                //writer.WriteValue("sss");
                writer.WriteEndElement();
                if (Data.Count > 0)
                    Data.WriteXml(writer);
                writer.WriteFullEndElement();
            }
        }

        public static void Load()
        {
            Data.islookup = true;
            string fileName = "LookupTable.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            if (!File.Exists(fileName))
                return;
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                reader.ReadStartElement("LookupTable");
                reader.ReadStartElement("Count");
                int count = reader.ReadContentAsInt();
                reader.ReadEndElement();

                if (count > 0)
                    Data.ReadXml(reader);
              
            }
            Initialize();
        }

        public static bool ChipKickReplaceRobotID(int first, int second)
        {
            if (first == second)
                return true;
            if (Data.ContainsKey(first) && Data != null)
            {
                if (Data.ContainsKey(first) && Data.ContainsKey(second))
                {
                    List<ChipKickInfo> temp = Data[first].KickInfo.ToList();
                    Data[first].KickInfo = Data[second].KickInfo.ToList();
                    Data[second].KickInfo = temp.ToList(); ;
                }
                else if (Data.ContainsKey(first) && !Data.ContainsKey(second))
                {
                    List<ChipKickInfo> temp = Data[first].KickInfo.ToList();
                    Data.Add(second, new MetricChipKick() { KickInfo = temp.ToList() });
                    Data.Remove(first);
                }
                return true;
            }
            return false;
        }

        public static void Initialize()
        {

            if (Data == null)
                Data = new SerializableDictionary<int, MetricChipKick>();
            
            #region Chip

            foreach (var item in Data)
            {
                double[] d = new double[1000];
                double[] dws = new double[1000];

                List<ChipKickInfo> sorted = item.Value.KickInfo.OrderBy(o => o.Power).ToList();
                double end_power_d = 0;
                double end_power_dw = 0;

                foreach (var itemSorted in sorted)
                {
                    if (itemSorted.HasSpin == false)
                    {
                        end_power_d = itemSorted.Power;
                        d[(int)Math.Round(itemSorted.Length * 100)] = itemSorted.Power;
                    }
                    else
                    {
                        end_power_dw = itemSorted.Power;
                        dws[(int)Math.Round(itemSorted.Length * 100)] = itemSorted.Power;
                    }
                }
                d[999] = end_power_d;
                dws[999] = end_power_dw;

                int i = 0;
                while (i < 1000)
                {
                    if (d[i] != 0)
                    {
                        for (int j = i + 1; j < 1000; j++)
                        {
                            if (d[i] != 0 && d[j] != 0)
                            {
                                for (int k = i; k < j; k++)
                                {
                                    d[k] = d[i] + ((d[j] - d[i]) * (k - i) / (j - i));
                                }
                                i = j - 1;
                                break;
                            }
                        }
                    }
                    i++;
                }
                ChipLookupWithoutSpin[item.Key] = d;

                i = 0;
                while (i < 1000)
                {
                    if (dws[i] != 0)
                    {
                        for (int j = i + 1; j < 1000; j++)
                        {
                            if (dws[i] != 0 && dws[j] != 0)
                            {
                                for (int k = i; k < j; k++)
                                {
                                    dws[k] = d[i] + ((dws[j] - dws[i]) * (k - i) / (j - i));
                                }
                                i = j - 1;
                                break;
                            }
                        }
                    }
                    i++;
                }
                ChipLookupWithSpin[item.Key] = dws;
            }

            #endregion

        }

        public static int GetChipPower(int robotID, double Length, double SafeRadius, bool HasSpin)
        {

            int retVal = 0;
            if ((Length * 100) > 999)
            {
                if (HasSpin == true)
                {
                    retVal = (int)Math.Round(ChipLookupWithSpin[robotID].ElementAt(999));
                }
                else
                {
                    retVal = (int)Math.Round(ChipLookupWithoutSpin[robotID].ElementAt(999));
                }

                if (retVal == 0)
                {
                    double min = 0;
                    if (ChipLookupWithSpin.Count > 0 && ChipLookupWithSpin.ContainsKey(robotID))
                        min = (int)ChipLookupWithSpin[robotID].FirstOrDefault(f => f != 0);
                    retVal = (int)((min == 0) ? 80 : min);
                }
            }
            else
            {
                if (HasSpin == true)
                {
                    if (ChipLookupWithSpin.ContainsKey(robotID))
                        retVal = (int)Math.Round(ChipLookupWithSpin[robotID].ElementAt((int)Math.Round(Length * 100)));
                    else if (ChipLookupWithSpin.Count > 0)
                        retVal = (int)Math.Round(ChipLookupWithSpin.First().Value.ElementAt((int)Math.Round(Length * 100)));
                    else
                        retVal = 120;
                    if (retVal == 0)
                    {
                        double min = 0;
                        if (ChipLookupWithSpin.Count > 0 && ChipLookupWithSpin.ContainsKey(robotID))
                            min = (int)ChipLookupWithSpin[robotID].FirstOrDefault(f => f != 0);
                        retVal = (int)((min == 0) ? 80 : min);
                    }

                }
                else
                {
                    if (ChipLookupWithoutSpin.ContainsKey(robotID))
                        retVal = (int)Math.Round(ChipLookupWithoutSpin[robotID].ElementAt((int)Math.Round(Length * 100)));
                    else if (ChipLookupWithoutSpin.Count > 0)
                        retVal = (int)Math.Round(ChipLookupWithoutSpin.First().Value.ElementAt((int)Math.Round(Length * 100)));
                    else
                        retVal = 120;
                    if (retVal == 0)
                    {
                        double min = 0;
                        if (ChipLookupWithoutSpin.Count > 0 && ChipLookupWithoutSpin.ContainsKey(robotID))
                            min = (int)ChipLookupWithoutSpin[robotID].FirstOrDefault(f => f != 0);
                        retVal = (int)((min == 0) ? 70 : min);
                    }
                }
            }


            return retVal;
        }
    }
}
