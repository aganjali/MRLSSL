using System.Collections.Generic;
using System.Linq;
using System;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
namespace MRL.SSL.GameDefinitions.General_Settings
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class LookUpTable
    {
        public static Dictionary<int, double[]> DirectLookup = new Dictionary<int, double[]>();
        public static Dictionary<int, double[]> ChipLookupWithSpin = new Dictionary<int, double[]>();
        public static Dictionary<int, double[]> ChipLookupWithoutSpin = new Dictionary<int, double[]>();

        public LookUpTable()
        {

            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
            this.SettingsLoaded += new System.Configuration.SettingsLoadedEventHandler(LookUpTable_SettingsLoaded);
        }

        void LookUpTable_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {

        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
        }

        public int GetSuitablePassSpeed(int robotID, Position2D pos1, Position2D pos2)
        {
            double x1 = 0.5;   // m
            double y1 = 4; // m / s
            double x2 = 4;   // m
            double y2 = 9;   // m / s
            double x = pos1.DistanceFrom(pos2);
            double y = Math.Max((y2 - y1) * (x - x1) / (x2 - x1), 2);
            y = Math.Min(y, 6);
            return GetDirectKickPower(robotID, y);

        }

        public int GetDirectKickPower(int robotId, double Speed)
        {
            if (DirectLookup.ContainsKey(robotId))
                return (int)DirectLookup[robotId][(int)(Speed * 10)];
            else
                if (DirectLookup.Count > 0)
                    return (int)DirectLookup.First().Value[(int)(Speed * 10)];
                else
                    return 150;
            //List<DirectKickTemplat> list = new List<DirectKickTemplat>();
            //foreach (var item in DirectKick)
            //{
            //    list.Add(new DirectKickTemplat()
            //    {
            //        RobotID = int.Parse(item.Key.Split(new char[] { '@' })[0]),
            //        Power = int.Parse(item.Key.Split(new char[] { '@' })[1]),
            //        Speed = item.Value,
            //    });
            //}
            //if (list.Any(a => a.RobotID == robotId && a.Speed == Speed))
            //    return list.First(a => a.RobotID == robotId && a.Speed == Speed).Power;
            //List<DirectKickTemplat> order = list.Where(a => a.RobotID == robotId).OrderByDescending(o => o.Speed).ToList();

            //double befor = order.First(f => f.Speed < Speed).Speed;
            //double next = order.First(f => f.Speed > Speed).Speed;

            //if ((Speed - befor) < (next - Speed))
            //    return order.First(f => f.Speed < Speed).Power;
            //return order.First(f => f.Speed > Speed).Power;
        }

        public int GetSuitableChipPower(int robotID, Position2D pos1, Position2D pos2, bool hasSpinBack)
        {
            double dist = pos1.DistanceFrom(pos2);
            return GetChipPower(robotID, dist * (2 / 4.0), 1, hasSpinBack);
        }

        public int GetChipPower(int robotID, double Length, double SafeRadius, bool HasSpin)
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
                        retVal = 110;
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
                        retVal = 110;
                    if (retVal == 0)
                    {
                        double min = 0;
                        if (ChipLookupWithoutSpin.Count > 0 && ChipLookupWithoutSpin.ContainsKey(robotID))
                            min = (int)ChipLookupWithoutSpin[robotID].FirstOrDefault(f => f != 0);
                        retVal = (int)((min == 0) ? 80 : min);
                    }
                }
            }


            return retVal;
        }

        public void Initialize()
        {

            if (MetricChipKick == null)
                MetricChipKick = new SerializableDictionary<int, MetricChipKick>();
            if (DirectKick == null)
                DirectKick = new SerializableDictionary<string, double>();
            #region Direct
            List<DirectKickTemplat> list = new List<DirectKickTemplat>();

            DirectLookup = new Dictionary<int, double[]>();
            foreach (var item in DirectKick)
            {
                list.Add(new DirectKickTemplat()
                {
                    RobotID = int.Parse(item.Key.Split(new char[] { '@' })[0]),
                    Power = int.Parse(item.Key.Split(new char[] { '@' })[1]),
                    Speed = item.Value,
                });
            }
            for (int i = 1; i < 8; i++)
            {
                if (list.Any(a => a.RobotID == i))
                {
                    int? firstX = null, secondX = null;
                    double? firstY = null, secondY = null;
                    double[] newList = new double[256];
                    double[] d = new double[150];

                    foreach (var item in list.Where(w => w.RobotID == i).OrderBy(o => o.Power))
                    {
                        if (firstX == null)
                        {
                            firstX = item.Power;
                            firstY = item.Speed;
                            for (int k = 0; k < firstX.Value; k++)
                            {
                                newList[k] = (k) * (firstY.Value) / (firstX.Value);
                            }
                        }
                        else
                        {
                            secondX = item.Power;
                            secondY = item.Speed;
                            for (int k = firstX.Value; k < secondX.Value; k++)
                            {
                                newList[k] = (k - firstX.Value) * (secondY.Value - firstY.Value) / (secondX.Value - firstX.Value) + firstY.Value;
                            }
                            firstX = secondX;
                            firstY = secondY;
                        }
                    }
                    for (int k = 0; k < 255; k++)
                    {
                        if ((int)Math.Round(newList[k] * 10) < d.GetUpperBound(0))
                        {
                            d[(int)Math.Round(newList[k] * 10)] = k;
                        }
                    }
                    double temp = 0;
                    for (int k = 0; k < 150; k++)
                    {
                        if (d[k] == 0)
                            d[k] = temp;
                        else
                            temp = d[k];
                    }

                    DirectLookup.Add(i, d);
                }
            }
            #endregion

            #region Chip


            foreach (var item in MetricChipKick)
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

        public bool DirectKickReplaceRobotID(int first, int second)
        {
            if (first == second)
                return false;
            List<DirectKickTemplat> list = new List<DirectKickTemplat>();
            foreach (var item in DirectKick)
            {
                list.Add(new DirectKickTemplat()
                {
                    RobotID = int.Parse(item.Key.Split(new char[] { '@' })[0]),
                    Power = int.Parse(item.Key.Split(new char[] { '@' })[1]),
                    Speed = item.Value,
                });
            }
            if (list.FirstOrDefault(f => f.RobotID == first) != null && list.FirstOrDefault(f => f.RobotID == second) == null)
            {
                foreach (var item in list)
                    if (item.RobotID == first)
                        item.RobotID = second;
            }
            else if (list.FirstOrDefault(f => f.RobotID == first) != null && list.FirstOrDefault(f => f.RobotID == second) != null)
            {
                foreach (var item in list)
                {
                    if (item.RobotID == first)
                        item.RobotID = second;
                    else if (item.RobotID == second)
                        item.RobotID = first;
                }
            }

            DirectKick = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, double>();

            list.ForEach(p => DirectKick.Add(p.Key, p.Speed));
            return true;
        }

        public bool ChipKickReplaceRobotID(int first, int second)
        {
            if (first == second)
                return true;
            if (MetricChipKick.ContainsKey(first) && MetricChipKick != null)
            {
                if (MetricChipKick.ContainsKey(first) && MetricChipKick.ContainsKey(second))
                {
                    List<ChipKickInfo> temp = MetricChipKick[first].KickInfo.ToList();
                    MetricChipKick[first].KickInfo = MetricChipKick[second].KickInfo.ToList();
                    MetricChipKick[second].KickInfo = temp.ToList(); ;
                }
                else if (MetricChipKick.ContainsKey(first) && !MetricChipKick.ContainsKey(second))
                {
                    List<ChipKickInfo> temp = MetricChipKick[first].KickInfo.ToList();
                    MetricChipKick.Add(second, new MetricChipKick() { KickInfo = temp.ToList() });
                    MetricChipKick.Remove(first);
                }
                return true;
            }
            return false;
        }
    }
}
