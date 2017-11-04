using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;


namespace MRL.SSL.Visualizer.Extentions
{
    public static class CommonExtensions
    {

        public static void SetImageSource(this Image img, string path)
        {
            string asmName = Assembly.GetExecutingAssembly().GetName().Name;
            string uriString = string.Format("pack://application:,,,/{0};component/Images/{1}", asmName, path);
            ImageSource imgSrc = new BitmapImage(new Uri(uriString));
            if (img == null)
            {
                img = new Image();
            }
            img.Source = imgSrc;
        }

        public static void EnsureVisible(this TreeViewItem node)
        {
            if (node != null)
            {
                node.IsExpanded = true;
                EnsureVisible(node.Parent as TreeViewItem);
            }
        }

        /// <summary>
        /// Updates the underlying data object, without needing to leave the bound controls in a dependency object.
        /// </summary>
        /// <remarks>Normally the underlying data source won't change in a dependency object untill you leave the bound control or you have to set UpdateSourceTrigger property of the binding to "PropertyChanged" value so that it updates the underlying data object on changes of the bound property of your control. 
        /// Calling this method updates the data source without needing to leave the control. 
        /// It useful specially when you use shortcut keys for updating the data source rather than pressing a button.</remarks>
        /// <param x:Name="parent">The parent object which owns the bound control; usually your form.</param>
        /// <example>this.EdnEdit();</example>
        public static void EndEdit(this DependencyObject parent)
        {
            LocalValueEnumerator localValues = parent.GetLocalValueEnumerator();
            while (localValues.MoveNext())
            {
                LocalValueEntry entry = localValues.Current;
                if (System.Windows.Data.BindingOperations.IsDataBound(parent, entry.Property))
                {
                    System.Windows.Data.BindingExpression binding = System.Windows.Data.BindingOperations.GetBindingExpression(parent, entry.Property);
                    if (binding != null)
                    {
                        binding.UpdateSource();
                    }
                }
            }

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                EndEdit(child);
            }
        }

        /// <summary>
        /// Returns a substitute value if the source is null and the source otherwise.
        /// </summary>
        /// <typeparam x:Name="T"></typeparam>
        /// <param x:Name="source">The object being tested</param>
        /// <param x:Name="substitue">A replacement for the null object</param>
        /// <returns>Substitute value if the source is null and the source otherwise</returns>
        /// <example>
        /// String str = MyString.IsNull("-");
        /// Button btn = NullButton.IsNull(new Button());
        /// </example>
        public static T IsNull<T>(this T source, T substitue) where T : class
        {
            if (source == null)
            {
                return substitue;
            }
            else
            {
                return source;
            }
        }


        /// <summary>
        /// Determines if the instance is between the given values including the bounds. 
        /// </summary>
        /// <example>
        /// if( "hello".IsBetween("example1", "sample2") ) { ... }
        /// if( someInt.IsBetween(5,6) ){...}
        /// </example>
        public static bool IsBetween<T>(this IComparable<T> instance, T value1, T value2)
        {
            return instance.CompareTo(value1) >= 0 && instance.CompareTo(value2) <= 0;
        }

        public static T As<T>(this object objectToCast)
        {
            if (objectToCast == null)
                return default(T);
            else
            {
                return (T)objectToCast;
            }

        }


        public static MRL.SSL.CommonClasses.MathLibrary.Position2D ToPosition(this System.Drawing.Point Pixel, MRL.SSL.GameDefinitions.FieldOrientation ShowMode, SlimDX.Matrix3x2? transform)
        {
            double X = Pixel.X;
            double Y = Pixel.Y;
            if (ShowMode == MRL.SSL.GameDefinitions.FieldOrientation.Verticaly)
            {
                X = (X - transform.Value.M31) / transform.Value.M21;
                Y = (Y - transform.Value.M32) / transform.Value.M12;
                return new MRL.SSL.CommonClasses.MathLibrary.Position2D(Y, X);
            }
            else
            {
                X = (X - transform.Value.M31) / transform.Value.M11;
                Y = (Y - transform.Value.M32) / transform.Value.M22;
                return new MRL.SSL.CommonClasses.MathLibrary.Position2D(X, Y);
            }

        }


        public static MRL.SSL.CommonClasses.MathLibrary.Position2D ToPosition(this System.Drawing.Point Pixel, SlimDX.Matrix3x2? transform)
        {
            double X = Pixel.X;
            double Y = Pixel.Y;

            X = (X - transform.Value.M31) / transform.Value.M21;
            Y = (Y - transform.Value.M32) / transform.Value.M12;

            return new MRL.SSL.CommonClasses.MathLibrary.Position2D(Y, X);
        }

        public static T Clone<T>(this T source) where T : new()
        {
            T newT = new T();
            foreach (var p in typeof(T).GetProperties())
            {
                if (p.CanWrite)
                {
                    p.SetValue(newT, p.GetValue(source, null), null);
                }
            }
            return newT;
        }

        public static void CopyTo<T1, T2>(this T1 source, T2 destObject) where T2 : T1
        {
            foreach (var p in typeof(T1).GetProperties())
            {
                if (p.CanWrite)
                {
                    p.SetValue(destObject, p.GetValue(source, null), null);
                }
            }
        }

        public static double ToDouble(this string source)
        {
            double result;
            if (double.TryParse(source, out result))
                return result;
            return double.NaN;

        }

        public static int ToInt(this string source)
        {
            int result;
            if (int.TryParse(source, out result))
                return result;
            return int.MinValue;

        }

        public static float ToFloat(this string source)
        {
            float result;
            if (float.TryParse(source, out result))
                return result;
            return float.NaN;

        }

        public static List<char> ToList(this string source)
        {
            List<char> res = new List<char>();
            foreach (var item in source)
                res.Add(item);
            return res;
        }
        /// <summary>
        /// Retrievs the object defined in the ItemTemplate of the Given ItemsControl for the given item of it.
        /// </summary>
        /// <param name="item">The item to retrieve the template control for</param>
        /// <remarks>
        /// Assume that you have a ListBox with CheckBox as ItemControl. When you enumerate ListBox.Items you just get the bound entities.
        /// Using this method you can get the checkBox for ecah item.
        /// </remarks>
        public static DependencyObject GetTemplateContent(this ItemsControl ic, object item)
        {
            return VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(ic.ItemContainerGenerator.ContainerFromItem(item), 0), 0), 0);
        }

    }
}
