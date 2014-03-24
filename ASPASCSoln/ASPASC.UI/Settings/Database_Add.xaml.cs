using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ASPASC.BusinessLayer;

namespace ASPASC.UI.Settings
{
    /// <summary>
    /// Interaction logic for Database_Add.xaml
    /// </summary>
    public partial class Database_Add : Window
    {

        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL("");
            }
        }

        #endregion

        public Database_Add()
        {
            InitializeComponent();
        }

        #region "Checkbox Events"

        private void cbAll_Checked(object sender, RoutedEventArgs e)
        {
            if (cbMembership != null
                && cbPersonal != null
                && cbProfile != null
                && cbRole != null
                && cbWebEvent != null)
            {
                if (cbAll.IsChecked.GetValueOrDefault())
                {
                    cbMembership.IsChecked = false;
                    cbPersonal.IsChecked = false;
                    cbProfile.IsChecked = false;
                    cbRole.IsChecked = false;
                    cbWebEvent.IsChecked = false;
                }

                cbMembership.IsEnabled = !cbAll.IsChecked.GetValueOrDefault();
                cbPersonal.IsEnabled = !cbAll.IsChecked.GetValueOrDefault();
                cbProfile.IsEnabled = !cbAll.IsChecked.GetValueOrDefault();
                cbRole.IsEnabled = !cbAll.IsChecked.GetValueOrDefault();
                cbWebEvent.IsEnabled = !cbAll.IsChecked.GetValueOrDefault();
            }
        }

        private void cbWindowsAuth_Checked(object sender, RoutedEventArgs e)
        {
            tbUserName.IsEnabled = !cbWindowsAuth.IsChecked.GetValueOrDefault();
            tbPassword.IsEnabled = !cbWindowsAuth.IsChecked.GetValueOrDefault();
        }

        #endregion

        #region "Helpers"

        private Boolean checkRequiredFields()
        {
            Boolean everythingIsOk = true;

            tbDatabaseName.Background = Brushes.White;
            tbDBServer.Background = Brushes.White;
            tbApplicationName.Background = Brushes.White;
            tbUserName.Background = Brushes.White;
            tbPassword.Background = Brushes.White;
            cbAll.Foreground = Brushes.Black;
            cbMembership.Foreground = Brushes.Black;
            cbPersonal.Foreground = Brushes.Black;
            cbProfile.Foreground = Brushes.Black;
            cbRole.Foreground = Brushes.Black;
            cbWebEvent.Foreground = Brushes.Black;

            if (tbDatabaseName.Text.Length == 0)
            {
                everythingIsOk = false;
                tbDatabaseName.Background = Brushes.Red;
            }

            if (tbDBServer.Text.Length == 0)
            {
                everythingIsOk = false;
                tbDBServer.Background = Brushes.Red;
            }

            if (tbApplicationName.Text.Length == 0)
            {
                everythingIsOk = false;
                tbApplicationName.Background = Brushes.Red;
            }

            if (!cbWindowsAuth.IsChecked.GetValueOrDefault())
            {
                if (tbUserName.Text.Length == 0)
                {
                    everythingIsOk = false;
                    tbUserName.Background = Brushes.Red;
                }

                if (tbPassword.Password.Length == 0)
                {
                    everythingIsOk = false;
                    tbPassword.Background = Brushes.Red;
                }
            }

            if (!cbAll.IsChecked.GetValueOrDefault() &&
                !cbMembership.IsChecked.GetValueOrDefault() &&
                !cbPersonal.IsChecked.GetValueOrDefault() &&
                !cbProfile.IsChecked.GetValueOrDefault() &&
                !cbRole.IsChecked.GetValueOrDefault() &&
                !cbWebEvent.IsChecked.GetValueOrDefault())
            {
                everythingIsOk = false;
                cbAll.Foreground = Brushes.Red;
                cbMembership.Foreground = Brushes.Red;
                cbPersonal.Foreground = Brushes.Red;
                cbProfile.Foreground = Brushes.Red;
                cbRole.Foreground = Brushes.Red;
                cbWebEvent.Foreground = Brushes.Red;
            }

            return everythingIsOk;
        }

        #endregion

        #region "Button Events"

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!checkRequiredFields())
            {
                MessageBox.Show("Please enter the required fields highlighed in Red.", "Error", MessageBoxButton.OK);
                return;
            }

            String dbNameSwitch = "-d {0}";
            String serverNameSwitch = "-S {0}";
            String userNameSwitch = "-U {0}";
            String passwordSwitch = "-P {0}";
            String windowsAuthSwitch = "-E";
            String optionsSwitch = "-A {0}";

            if (cbWindowsAuth.IsChecked.GetValueOrDefault())
            {
                userNameSwitch = "";
                passwordSwitch = "";
            }
            else
            {
                windowsAuthSwitch = "";
                userNameSwitch = String.Format(userNameSwitch, tbUserName.Text);
                passwordSwitch = String.Format(passwordSwitch, tbPassword.Password);
            }

            dbNameSwitch = String.Format(dbNameSwitch, tbDatabaseName.Text);
            serverNameSwitch = String.Format(serverNameSwitch, tbDBServer.Text);

            if (cbAll.IsChecked.GetValueOrDefault())
            {
                optionsSwitch = String.Format(optionsSwitch, "all");
            }
            else
            {
                optionsSwitch = String.Format(optionsSwitch,
                    (cbMembership.IsChecked.GetValueOrDefault() ? "m" : "") +
                    (cbRole.IsChecked.GetValueOrDefault() ? "r" : "") +
                    (cbProfile.IsChecked.GetValueOrDefault() ? "p" : "") +
                    (cbPersonal.IsChecked.GetValueOrDefault() ? "c" : "") +
                    (cbWebEvent.IsChecked.GetValueOrDefault() ? "w" : ""));
            }

            String windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            String[] frameworkDirs = Directory.GetDirectories(String.Format(@"{0}\Microsoft.NET", windowsPath), "Framework*");

            String[] versionDirs = Directory.GetDirectories(frameworkDirs[frameworkDirs.Length - 1]).OrderBy(p => p).ToArray();

            String processPath = String.Format(@"{0}\aspnet_regsql.exe", versionDirs[versionDirs.Length -1]);
            String processArguments = String.Format(@"{0} {1} {2} {3} {4} {5}", serverNameSwitch, dbNameSwitch, userNameSwitch, passwordSwitch, windowsAuthSwitch, optionsSwitch);

            System.Diagnostics.Process.Start(processPath, processArguments);

            String connectionString = "";

            if (cbWindowsAuth.IsChecked.GetValueOrDefault())
                    connectionString = String.Format("Server={0};Database={1};Trusted_Connection=true", tbDBServer.Text, tbDatabaseName.Text);
                else
                    connectionString = String.Format("Server={0};Database={1};User Id={2};Password={3}", tbDBServer.Text, tbDatabaseName.Text, tbUserName.Text, tbPassword.Password);

            _BL.application_add("/", connectionString);

        }

        #endregion
    }
}
