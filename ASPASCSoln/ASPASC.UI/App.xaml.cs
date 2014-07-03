using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ASPASC.Utilities;

namespace ASPASC.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException (object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            TextLogger.LogError(e.Exception, "Unhandled Exception Handler");

            MessageBox.Show("An unknown error occured. Please consult the log file.");

            e.Handled = true;
        }
    }
}
