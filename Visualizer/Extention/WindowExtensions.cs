using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;

namespace MRL.SSL.Visualizer.Extentions
{
    public static class WindowExtensions
    {
        public static object ExtractContent(this Window win)
        {
            object content;
            if (win.Content == null)
                content = win.Tag;
            else
            {
                content = win.Tag = win.Content;
            }
            if (content.As<FrameworkElement>().Parent != null)
            {
                content.As<FrameworkElement>().Parent.As<ContentControl>().Content = null;
            }
            return content;
        }

        public static TabItem ShowAsTab(this Window win, TabControl tabControl)
        {
            return ShowAsSingleTab(win, tabControl, Guid.NewGuid().ToString());
        }

        public static TabItem ShowAsSingleTab(this Window win, TabControl tabControl, string uniqueTag)
        {
            TabItem otherInstance = tabControl.Items.Cast<TabItem>().Where(t => t.Tag.ToString() == uniqueTag).SingleOrDefault();
            if (otherInstance == null)
            {
                TabItem ti = new TabItem()
                {
                    Content = win.ExtractContent(),
                    Tag = uniqueTag
                };
                Image img = new Image()
                {
                    Width = 14,
                    Height = 14
                };
                img.SetImageSource("close1.png");

                img.MouseEnter += (s, e) =>
                {
                    img.SetImageSource("close2.png");
                };
                img.MouseLeave += (s, e) =>
                {
                    img.SetImageSource("close1.png");
                };

                img.MouseDown += (s, e) =>
                {
                    if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                    {
                        img.SetImageSource("close3.png");
                    }
                };


                Label lbl = new Label()
                {
                    Name = "lbl"
                };
                lbl.Content = win.Title;

                NameScope ns = new NameScope();
                NameScope.SetNameScope(ti, ns);
                NameScope.SetNameScope(lbl, ns);
                ns.RegisterName("lbl", lbl);

                StackPanel pnlStack = new StackPanel();

                pnlStack.Children.Add(img);
                pnlStack.Children.Add(lbl);
                pnlStack.Orientation = Orientation.Horizontal;

                StackPanel pnlMain = new StackPanel();
                pnlMain.Orientation = Orientation.Vertical;

                pnlMain.Children.Add(pnlStack);
                
                img.MouseLeftButtonUp += (s, e) =>
                {
                    if (Closing != null)
                        Closing(ti, ti.Tag.ToString());
                    tabControl.Items.Remove(ti);
                    if (tabControl.Name == "mainTabControl" && tabControl.Items.Count == 0)
                        tabControl.Visibility = Visibility.Collapsed;
                };
                ti.Header = pnlMain;
                tabControl.Items.Add(ti);
                ti.Focus();
                return ti;
            }
            else
            {

                otherInstance.FindName("lbl").As<Label>().Content = win.Title;
                otherInstance.Focus();
                return otherInstance;
            }
        }

        public static TabItem ShowAsSingleTab(this Window win, TabControl tabControl,out double realSize, string uniqueTag)
        {
            TabItem otherInstance = tabControl.Items.Cast<TabItem>().Where(t => t.Tag.ToString() == uniqueTag).SingleOrDefault();
            if (otherInstance == null)
            {
                realSize = win.Width;
                TabItem ti = new TabItem()
                {
                    
                    Content = win.ExtractContent(),
                    Tag = uniqueTag
                };
                
                
                Image img = new Image()
                {
                    Width = 14,
                    Height = 14
                };
                img.SetImageSource("close1.png");

                img.MouseEnter += (s, e) =>
                {
                    img.SetImageSource("close2.png");
                };
                img.MouseLeave += (s, e) =>
                {
                    img.SetImageSource("close1.png");
                };

                img.MouseDown += (s, e) =>
                {
                    if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                    {
                        img.SetImageSource("close3.png");
                    }
                };


                Label lbl = new Label()
                {
                    Name = "lbl"
                };
                lbl.Content = win.Title;

                NameScope ns = new NameScope();
                NameScope.SetNameScope(ti, ns);
                NameScope.SetNameScope(lbl, ns);
                ns.RegisterName("lbl", lbl);

                StackPanel pnlStack = new StackPanel();

                pnlStack.Children.Add(img);
                pnlStack.Children.Add(lbl);
                pnlStack.Orientation = Orientation.Horizontal;

                StackPanel pnlMain = new StackPanel();
                pnlMain.Orientation = Orientation.Vertical;

                pnlMain.Children.Add(pnlStack);

                img.MouseLeftButtonUp += (s, e) =>
                {
                    tabControl.Items.Remove(ti);
                    if (tabControl.Items.Count == 0)
                        tabControl.Visibility = Visibility.Collapsed;
                };
                ti.Header = pnlMain;
                tabControl.Items.Add(ti);
                ti.Focus();
                return ti;
            }
            else
            {

                otherInstance.FindName("lbl").As<Label>().Content = win.Title;
                otherInstance.Focus();
                realSize = win.Width;
                return otherInstance;
            }
        }

        public static void ShowAsChildOf(this Window win, Panel panel)
        {
            panel.Children.Add(win.ExtractContent() as UIElement);
        }

        public delegate void TabItemClosing(object Sender, string Tag);
        public static event TabItemClosing Closing;
    }
}
