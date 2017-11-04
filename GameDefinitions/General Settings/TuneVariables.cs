using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonClasses.Extentions;
using System;
using System.Collections.Generic;
namespace MRL.SSL.GameDefinitions.General_Settings
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class TuneVariables
    {
        Dictionary<string, Position2D> Positions = new Dictionary<string, Position2D>();
        Dictionary<string, double> doubles = new Dictionary<string, double>();
        Dictionary<string, bool> bools = new Dictionary<string, bool>();

        public TuneVariables()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
            this.SettingsLoaded += new System.Configuration.SettingsLoadedEventHandler(TuneVariables_SettingsLoaded);
        }

        void TuneVariables_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            if (Position2Ds != null)
                Positions = Position2Ds;
            if (Doubles != null)
                doubles = Doubles;
            if (Booleans != null)
                bools = Booleans;
        }

        public void Refresh(SerializableDictionary<string, double> dic1, SerializableDictionary<string, int> dic2, SerializableDictionary<string, Position2D> dic3, SerializableDictionary<string, bool> dic4)
        {

            if (dic1 != null)
            {
                if (TuneVariables.Default.Doubles == null)
                    TuneVariables.Default.Doubles = dic1;
                else
                    lock (TuneVariables.Default.Doubles)
                    {
                        foreach (string key in dic1.Keys)
                        {
                            if (!TuneVariables.Default.Doubles.ContainsKey(key))
                                TuneVariables.Default.Doubles.Add(key, dic1[key]);
                            else
                                TuneVariables.Default.Doubles[key] = dic1[key];
                        }
                    }
            }
            else
                TuneVariables.Default.Doubles = new SerializableDictionary<string, double>();



            if (dic2 != null)
            {
                if (TuneVariables.Default.Integers == null)
                    TuneVariables.Default.Integers = dic2;

                else
                    lock (TuneVariables.Default.Integers)
                    {
                        foreach (string key in dic2.Keys)
                        {
                            if (!TuneVariables.Default.Integers.ContainsKey(key))
                                TuneVariables.Default.Integers.Add(key, dic2[key]);
                            else
                                TuneVariables.Default.Integers[key] = dic2[key];
                        }
                    }
            }
            else
                TuneVariables.Default.Integers = new SerializableDictionary<string, int>();

            if (dic3 != null)
            {
                if (TuneVariables.Default.Position2Ds == null)
                    TuneVariables.Default.Position2Ds = dic3;
                else
                    lock (TuneVariables.Default.Position2Ds)
                    {
                        foreach (string key in dic3.Keys)
                        {
                            if (!TuneVariables.Default.Position2Ds.ContainsKey(key))
                                TuneVariables.Default.Position2Ds.Add(key, dic3[key]);
                            else
                                TuneVariables.Default.Position2Ds[key] = dic3[key];
                        }
                    }
            }
            else
                TuneVariables.Default.Position2Ds = new SerializableDictionary<string, Position2D>();

            if (dic4 != null)
            {
                if (TuneVariables.Default.Booleans == null)
                    TuneVariables.Default.Booleans = dic4;
                else
                    lock (TuneVariables.Default.Booleans)
                    {
                        foreach (string key in dic4.Keys)
                        {
                            if (!TuneVariables.Default.Booleans.ContainsKey(key))
                                TuneVariables.Default.Booleans.Add(key, dic4[key]);
                            else
                                TuneVariables.Default.Booleans[key] = dic4[key];
                        }
                    }
            }
            else
                TuneVariables.Default.Booleans = new SerializableDictionary<string, bool>();

            if (Refreshed != null)
                Refreshed(null);
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
        }
        /// <summary>
        /// Add a parameter
        /// </summary>
        /// <param name="key">parameter name</param>
        /// <param name="value">can be Position2D , Double ,bool or integer</param>
        public void Add(string key, object value)
        {
            if (value is Position2D)
            {
                if (Position2Ds == null)
                    Position2Ds = new SerializableDictionary<string, Position2D>();
                if (!Position2Ds.ContainsKey(key))
                {
                    Position2Ds[key] = value.As<Position2D>();
                    Positions[key] = value.As<Position2D>();
                    Save();
                }
            }
            else if (value is bool)
            {
                if (Booleans == null)
                    Booleans = new SerializableDictionary<string, bool>();
                if (!Booleans.ContainsKey(key))
                {
                    Booleans[key] = value.As<bool>();
                    bools[key] = value.As<bool>();
                    Save();
                }
            }
            else
            {
                if (Doubles == null)
                    Doubles = new SerializableDictionary<string, double>();
                if (!Doubles.ContainsKey(key))
                {
                    Doubles[key] = value.As<double>();
                    doubles[key] = value.As<double>();
                    Save();
                }
            }
        }
        /// <summary>
        /// get a parameter
        /// </summary>
        /// <param name="key">parameter name</param>
        /// <param name="T">can be Position2D , Double ,bool or integer</param>
        public T GetValue<T>(string key)
        {
            double ddouble = 0.0;
            Position2D dpos = Position2D.Zero;
            bool dbool = false;

            if (typeof(T) == typeof(Position2D))
            {
                if (Positions != null && Positions.ContainsKey(key))
                    return Positions[key].As<T>();
                else
                    return Position2D.Zero.As<T>();
            }

            else if (typeof(T) == typeof(bool))
            {
                if (bools != null && bools.ContainsKey(key))
                    return bools[key].As<T>();
                else
                    return dbool.As<T>();
            }
            else
            {
                if (doubles != null && doubles.ContainsKey(key))
                    return doubles[key].As<T>();
                else
                    return ddouble.As<T>();
            }
        }

        public delegate void RefreshedEventHandler(object sender);
        public static event RefreshedEventHandler Refreshed;
    }
}
