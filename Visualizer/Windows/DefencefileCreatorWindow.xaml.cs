using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.IO;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for DefencefileCreatorWindow.xaml
    /// </summary>
    public partial class DefencefileCreatorWindow : Window
    {
        public DefencefileCreatorWindow()
        {
            InitializeComponent();
        }

        private void smallerx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            smallerxtext.Text = smallerx.Value.ToString();
            biggerx.Minimum = smallerx.Value;
            Minimumoflowerbound.Content = "-4.045";
            MaximumofHigherbound.Content = smallerx.Value;
        }

        private void biggerx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            biggerxtext.Text = biggerx.Value.ToString();
            smallerx.Maximum = biggerx.Value;
            Maximumoflowerbound.Content = biggerx.Value;
            MinimumofHigherbound.Content = "4.045";
        }

        private void smalery_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            smallerytext.Text = smalery.Value.ToString();
            biggery.Minimum = smalery.Value;
            Minimumofylowerbound.Content = "-3.025";
            MaximumofyHigherbound.Content = smalery.Value;
        }

        private void biggery_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            biggerytext.Text = biggery.Value.ToString();
            smalery.Maximum = biggery.Value;
            Maximumofylowerbound.Content = biggery.Value;
            MinimumofyHigherbound.Content = "3.025";
        }
        List<double> xlist = new List<double>();
        List<double> ylist = new List<double>();
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            double devidesize = (int)divideNum.Value;
            double maxx = biggerx.Value;
            double minx = smallerx.Value;
            double maxy = biggery.Value;
            double miny = smalery.Value;
            double xstep = (maxx - minx) / devidesize;
            double ystep = (maxy - miny) / devidesize;

            for (int i = 0; i < (int)devidesize; i++)
            {
                for (int j = 0; j < (int)devidesize; j++)
                {
                    xlist.Add(minx + (xstep * i));
                    ylist.Add(miny + (ystep * j));
                }
            }
            SaveData((int)Math.Pow(devidesize,2));
        }
        List<double> xposes = new List<double>();
        List<double> yposes = new List<double>();
        private void SaveData(int dataCount)
        {
            Random randomNumber = new Random();
            string fileName = "DefenceTest.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("DefenceTest");
                writeValue(writer, "CountOfDataGo", dataCount);
                for (int i = 0; i < dataCount; i++)
                {
                    writeValue(writer, "PosX" + i, xlist[i]);
                    writeValue(writer, "PosY" + i, ylist[i]);

                }
                writer.WriteFullEndElement();
            }
        }
        private static void writeValue(XmlTextWriter xml, string tagName, object value)
        {
            xml.WriteStartElement(tagName);
            xml.WriteValue(value.ToString());
            xml.WriteEndElement();
        }
    }
}
