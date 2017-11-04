using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MRL.SSL.CommonClasses.MathLibrary;

using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.CommonControls.D2DControls;
using System.Windows.Forms.Integration;
using MRL.SSL.Visualizer.Classes;
using Visualizer.Classes;
using System.Threading;
using MRL.SSL.Visualizer.Extentions;
namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for AnalizerWindow.xaml
    /// </summary>
    public partial class AnalizerWindow : Window
    {
        DrawMode drawMode = DrawMode.Non;
        int lastourID = 1;
        int lastBallID = 1;
        int lastoppID = 1;
        bool mouseLeftDown = false;
        bool mouseRightDown = false;
        Position2D startloc = new Position2D();
        public Color PaintColor { get; set; }
        public float PaintWidth { get; set; }
        float? angle = new float();
        public enum DrawMode
        {
            Non,
            Line,
            BlueRobot,
            YellowRobot,
            Circle,
            Pen,
            Ball,
        }
        ManualResetEvent run;
        Thread t;
        
        public AnalizerWindow()
        {
            InitializeComponent();
            PaintColor = Color.Black;
            PaintWidth = 0.01f;
            mainField.MouseMove += new System.Windows.Forms.MouseEventHandler(mainField_MouseMove);
            mainField.MouseDown += new System.Windows.Forms.MouseEventHandler(mainField_MouseDown);
            mainField.MouseUp += new System.Windows.Forms.MouseEventHandler(mainField_MouseUp);
            mainField.AnalizeMode = true; 
            mainField.CurrentWrapper = new AiToVisualizerWrapper();
            mainField.Model = new WorldModel();
            mainField.Model.OurRobots = new Dictionary<int, SingleObjectState>();
            mainField.Model.Opponents = new Dictionary<int, SingleObjectState>();
            mainField.Model.BallState = new SingleObjectState();
            mainField.CurrentWrapper.AllBalls = new Dictionary<int, Position2D>();
            mainField.MouseClick += new System.Windows.Forms.MouseEventHandler(mainField_MouseClick);
            WindowExtensions.Closing += new WindowExtensions.TabItemClosing(WindowExtensions_Closing);
            run = new ManualResetEvent(false);
            t = new Thread(new ThreadStart(generatModel));
            t.Start();
            run.Set();
        }

        void WindowExtensions_Closing(object Sender, string Tag)
        {
            if (Tag.ToString() == "analize")
            {
                if (t != null)
                {
                    t.Suspend();
                    t = null;
                }
            }
        }

        void mainField_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Position2D pos;
                if (mainField.SelectedBallPosition(out pos))
                {
                    ContextMenu c = new ContextMenu();
                    MenuItem mi = new MenuItem();
                    mi.Header = "Select Ball";
                    mi.Click += (s, ev) =>
                    {
                        mainField.Model.BallState.Location = pos;
                    };

                    mainHost.ContextMenu = c;
                    c.IsOpen = true;
                }
            }
        }

        void mainField_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (drawMode == DrawMode.Line)
                    mainField.AddAnalizeObject(new Line(startloc, e.Location.ToPosition(mainField.Orentation,mainField.Transform), new System.Drawing.Pen(PaintColor, PaintWidth)));
                else if (drawMode == DrawMode.Circle)
                    mainField.AddAnalizeObject(new Circle(startloc, e.Location.ToPosition(mainField.Orentation, mainField.Transform).DistanceFrom(startloc), new System.Drawing.Pen(PaintColor, PaintWidth)));
                
                else if (drawMode == DrawMode.YellowRobot)
                {
                    SingleObjectState s = new SingleObjectState() { Location = startloc, Type = ObjectType.Opponent };
                    s.Angle = angle;
                    mainField.AddAnalizeObject(s);
                    mainField.Model.Opponents.Add(lastoppID, s);
                    lastoppID++;
                }
                else if (drawMode == DrawMode.Ball)
                {
                    mainField.CurrentWrapper.AllBalls.Add(lastBallID, startloc);
                    mainField.Model.BallState = new SingleObjectState() { Location = startloc, Type = ObjectType.Ball };
                    mainField.AddAnalizeObject(new SingleObjectState() { Location = startloc, Type = ObjectType.Ball });
                    lastBallID++;
                }
                else if (drawMode == DrawMode.BlueRobot)
                {
                    SingleObjectState s = new SingleObjectState() { Location = startloc, Type = ObjectType.OurRobot };
                    s.Angle = angle;
                    mainField.AddAnalizeObject(s);
                    mainField.Model.OurRobots.Add(lastourID, s);
                    lastourID++;
                }
                mouseLeftDown = false;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                mouseRightDown = false;
                angle = null;
            }
        }

        void mainField_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                mouseRightDown = true;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                startloc = e.Location.ToPosition(mainField.Transform);
                mouseLeftDown = true;
            }
        }

        void mainField_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (mouseRightDown && mouseLeftDown)
            {
                angle = (float)(e.Location.ToPosition(mainField.Transform) - startloc).AngleInDegrees;
            }
            else if (mouseLeftDown)
            {
                if (drawMode == DrawMode.Line)
                {
                    mainField.DrawLine(new Line(startloc, e.Location.ToPosition(mainField.Transform), new System.Drawing.Pen(PaintColor, PaintWidth)));
                }
                else if (drawMode == DrawMode.Circle)
                {
                    mainField.DrawCircle(new Circle(startloc, e.Location.ToPosition(mainField.Transform).DistanceFrom(startloc), new System.Drawing.Pen(PaintColor, PaintWidth)));
                }
            }
            else if (drawMode == DrawMode.YellowRobot)
            {
                mainField.DrawRobot(new SingleObjectState() { Angle = angle, Location = e.Location.ToPosition(mainField.Transform), Type = MRL.SSL.GameDefinitions.ObjectType.Opponent });
                startloc = e.Location.ToPosition(mainField.Transform);
            }
            else if (drawMode == DrawMode.Ball)
            {
                mainField.DrawRobot(new SingleObjectState() { Location = e.Location.ToPosition(mainField.Transform), Type = MRL.SSL.GameDefinitions.ObjectType.Ball });
                startloc = e.Location.ToPosition(mainField.Transform);
            }
            else if (drawMode == DrawMode.BlueRobot)
            {
                mainField.DrawRobot(new SingleObjectState() { Angle = angle, Location = e.Location.ToPosition(mainField.Transform), Type = MRL.SSL.GameDefinitions.ObjectType.OurRobot });
                startloc = e.Location.ToPosition(mainField.Transform);
            }
        }

        private void lineButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("lineButton",toolGrid);
            drawMode = DrawMode.Line;
            mainHost.Cursor = Cursors.Pen;
        }

        private void lineButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void circleButton_Checked(object sender, RoutedEventArgs e)
        {
            
            UncheckeOther("circleButton",toolGrid);
            drawMode = DrawMode.Circle;
        }

        private void circleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void UncheckeOther(string name, Grid grid)
        {
            List<UIElement> list = grid.Children.Cast<UIElement>().Where(w => w.GetType() == typeof(Telerik.Windows.Controls.RadToggleButton) && ((Telerik.Windows.Controls.RadToggleButton)w).Name != name).ToList();
            list.ForEach(a => ((Telerik.Windows.Controls.RadToggleButton)a).IsChecked = false);
        }

        private void bluerobotButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("bluerobotButton",toolGrid);
            drawMode = DrawMode.BlueRobot;
        }

        private void bluerobotButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void yellowrobotButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("yellowrobotButton",toolGrid);
            drawMode = DrawMode.YellowRobot;
        }

        private void yellowrobotButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            mainField.Undo();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            mainField.Clear();
        }

        private void penButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("penButton",toolGrid);
            drawMode = DrawMode.Pen;
        }

        private void penButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject();
        }

        private void ballButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("ballButton", toolGrid);
            drawMode = DrawMode.Ball;
        }

        private void ballButton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawMode = DrawMode.Non;
            mainField.ResetMomentlyObject(); 
        }

        private void storksmallButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("storksmallButton", strokGrid);
            PaintWidth = 0.01f;
        }

        private void storksmallButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void strokmiddleButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("strokmiddleButton", strokGrid);
            PaintWidth = 0.02f;
        }

        private void strokmiddleButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void stroklargButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("stroklargButton", strokGrid);
            PaintWidth = 0.03f;
        }

        private void stroklargButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void blackButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("blackButton", colorGrid);
            PaintColor = Color.Black;
        }

        private void redButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("redButton", colorGrid);
            PaintColor = Color.Red;
        }

        private void yellowButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("yellowButton", colorGrid);
            PaintColor = Color.Yellow;
        }

        private void greenButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("greenButton", colorGrid);
            PaintColor = Color.GreenYellow;
        }

        private void witeButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("witeButton", colorGrid);
            PaintColor = Color.White;
        }

        private void blueButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("blueButton", colorGrid);
            PaintColor = Color.Blue;
        }

        private void getRealModelButton_Click(object sender, RoutedEventArgs e)
        {
            GetData(DataReciever.CurrentWrapper);
        }

        private void getloggermodelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.Cast<Window>().Any(s => s.ToString() == "Visualizer.Windows.FieldWindow"))
            {
                //Application.Current.Windows.Cast<Window>().Single(s => s.ToString() == "Visualizer.Windows.FieldWindow").Content.As<Grid>()
                //    .Children.Cast<UIElement>().Single(si=>si.GetType()==typeof(WindowsFormsHost))
            }
            GetData(LogProssesor.CurrentReaded);
        }

        private void GetData(AiToVisualizerWrapper packet)
        {
            if (packet.Model == null) return;
            if (packet.Model.OurRobots != null)
                foreach (var item in packet.Model.OurRobots.Values)
                    mainField.AddAnalizeObject(item);
            if (packet.Model.Opponents != null)
                foreach (var item in packet.Model.Opponents.Values)
                    mainField.AddAnalizeObject(item);
            if (packet.Model.BallState != null)
                mainField.AddAnalizeObject(packet.Model.BallState);
            if (packet.AllBalls != null)
                foreach (var item in packet.AllBalls)
                    mainField.AddAnalizeObject(item);
        }

        private void pointerButton_Checked(object sender, RoutedEventArgs e)
        {
            UncheckeOther("pointerButton", toolGrid);
            mainField.MoveMode = true;
        }

        private void pointerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mainField.MoveMode = false;
        }

        private void mainHost_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void generatModel()
        {
           
            while (run.WaitOne())
            {
                if (DataSender.CurrentWrapper.RecieveMode == ModelRecieveMode.Analizer)
                {
                    DataSender.SendOn.Set();
                    DataSender.CurrentWrapper.SendData.Add("Model");
                    DataSender.CurrentWrapper.RecieveMode = ModelRecieveMode.Analizer;
                    DataSender.CurrentWrapper.SendData.Add("RecieveMode");
                    DataSender.CurrentWrapper.Model = mainField.Model;
                    
                    Thread.Sleep(16);
                }

            }
        }
    }
}
