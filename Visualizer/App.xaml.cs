using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using Enterprise;

namespace MRL.SSL.Visualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (e.Exception.InnerException != null)
                Logger.Write(LogType.Exception, e.Exception.InnerException.Message);
            else
                Logger.Write(LogType.Exception, e.ToString());
            
        }
    }
}
