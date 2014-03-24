using System;
using System.Collections.Generic;
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
using System.IO;

namespace ASPASC.UI.Settings
{
    /// <summary>
    /// Interaction logic for Features_Edit.xaml
    /// </summary>
    public partial class Features_Edit : Window
    {
        private MainWindow _mw = null;

        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL(_mw.lblApplicationName.Content.ToString());
            }
        }

        #endregion

        public Features_Edit(MainWindow mw)
        {
            _mw = mw;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            retrieveFeatureInformation();
        }

        #region "Helpers"

        private void retrieveFeatureInformation()
        {
            List<ASPASCFeature> installed = new List<ASPASCFeature>();
            List<ASPASCFeature> notInstalled = new List<ASPASCFeature>();

            if (_BL.membershipEnabled())
                installed.Add(new ASPASCFeature() { Feature = "Membership", Installed = true });
            else
                notInstalled.Add(new ASPASCFeature() { Feature = "Membership", Installed = false });

            if(_BL.rolesEnabled())
                installed.Add(new ASPASCFeature() { Feature = "Role Manager", Installed = true });
            else
                notInstalled.Add(new ASPASCFeature() { Feature = "Role Manager", Installed = false });

            if (_BL.profileEnabled())
                installed.Add(new ASPASCFeature() { Feature = "Profile", Installed = true });
            else
                notInstalled.Add(new ASPASCFeature() { Feature = "Profile", Installed = false });

            if (_BL.personalizationEnabled())
                installed.Add(new ASPASCFeature() { Feature = "Web Parts Personalization", Installed = true });
            else
                notInstalled.Add(new ASPASCFeature() { Feature = "Web Parts Personalization", Installed = false });

            if (_BL.webEventsEnabled())
                installed.Add(new ASPASCFeature() { Feature = "Web Events", Installed = true });
            else
                notInstalled.Add(new ASPASCFeature() { Feature = "Web Events", Installed = false });

            lvUninstalled.ItemsSource = notInstalled;
            lvInstalled.ItemsSource = installed;
        }

        #endregion

        #region "Button Events"

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lvUninstalled.SelectedIndex >= 0)
            {
                List<ASPASCFeature> installed = ((List<ASPASCFeature>)lvInstalled.ItemsSource);
                List<ASPASCFeature> notInstalled = ((List<ASPASCFeature>)lvUninstalled.ItemsSource).Where(p => p.Feature != ((ASPASCFeature)lvUninstalled.SelectedItem).Feature).ToList(); ;

                installed.Add(((List<ASPASCFeature>)lvUninstalled.ItemsSource).Where(p => p.Feature == ((ASPASCFeature)lvUninstalled.SelectedItem).Feature).FirstOrDefault());
                
                lvInstalled.ItemsSource = installed;
                lvUninstalled.ItemsSource = notInstalled;

                lvInstalled.Items.Refresh();
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lvInstalled.SelectedIndex >= 0)
            {
                List<ASPASCFeature> installed = ((List<ASPASCFeature>)lvInstalled.ItemsSource).Where(p => p.Feature != ((ASPASCFeature)lvInstalled.SelectedItem).Feature).ToList();
                List<ASPASCFeature> notInstalled = ((List<ASPASCFeature>)lvUninstalled.ItemsSource);

                notInstalled.Add(((List<ASPASCFeature>)lvInstalled.ItemsSource).Where(p => p.Feature == ((ASPASCFeature)lvInstalled.SelectedItem).Feature).FirstOrDefault());
                
                lvInstalled.ItemsSource = installed;
                lvUninstalled.ItemsSource = notInstalled;

                lvUninstalled.Items.Refresh();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            String dbNameSwitch = "-d {0}";
            String serverNameSwitch = "-S {0}";
            String userNameSwitch = "-U {0}";
            String passwordSwitch = "-P {0}";
            String windowsAuthSwitch = "-E";
            String addOptionsSwitch = "-A {0}";
            String removeOptionsSwitch = "-R {0}";

            if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseUserName.Length == 0)
            {
                userNameSwitch = "";
                passwordSwitch = "";
            }
            else
            {
                windowsAuthSwitch = "";
                userNameSwitch = String.Format(userNameSwitch, ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseUserName);
                passwordSwitch = String.Format(passwordSwitch, ASPASC.MembershipProvider.Encryption.Encrypt(ASPASC.MembershipProvider.Properties.Settings.Default.DatabasePassword, "^&@NJS&*^Dnas&s8Kas^&8"));
            }

            dbNameSwitch = String.Format(dbNameSwitch, ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseName);
            serverNameSwitch = String.Format(serverNameSwitch, ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseServer);

            List<String> toAdd = new List<String>();
            List<String> toRemove = new List<string>();

            foreach (ASPASCFeature f in lvInstalled.ItemsSource)
            {
                if (!f.Installed)
                    toAdd.Add(f.Feature);
            }

            foreach (ASPASCFeature f in lvUninstalled.ItemsSource)
            {
                if (f.Installed)
                    toRemove.Add(f.Feature);
            }

            if (toAdd.Count == 5)
            {
                addOptionsSwitch = String.Format(addOptionsSwitch, "all");
            }
            else
            {
                addOptionsSwitch = String.Format(addOptionsSwitch,
                    (toAdd.Contains("Membership") ? "m" : "") +
                    (toAdd.Contains("Role Manager") ? "r" : "") +
                    (toAdd.Contains("Profile") ? "p" : "") +
                    (toAdd.Contains("Web Parts Personalization") ? "c" : "") +
                    (toAdd.Contains("Web Events") ? "w" : ""));
            }

            if (toRemove.Count == 5)
            {
                removeOptionsSwitch = String.Format(addOptionsSwitch, "all");
            }
            else
            {
                removeOptionsSwitch = String.Format(removeOptionsSwitch,
                    (toRemove.Contains("Membership") ? "m" : "") +
                    (toRemove.Contains("Role Manager") ? "r" : "") +
                    (toRemove.Contains("Profile") ? "p" : "") +
                    (toRemove.Contains("Web Parts Personalization") ? "c" : "") +
                    (toRemove.Contains("Web Events") ? "w" : ""));
            }

            String windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            String[] frameworkDirs = Directory.GetDirectories(String.Format(@"{0}\Microsoft.NET", windowsPath), "Framework*");

            String[] versionDirs = Directory.GetDirectories(frameworkDirs[frameworkDirs.Length - 1]).OrderBy(p => p).ToArray();

            String processPath = String.Format(@"{0}\aspnet_regsql.exe", versionDirs[versionDirs.Length - 1]);
            String processArguments = String.Format(@"{0} {1} {2} {3} {4} {5} {6}", serverNameSwitch, dbNameSwitch, userNameSwitch, passwordSwitch, windowsAuthSwitch, addOptionsSwitch, removeOptionsSwitch);

            System.Diagnostics.Process.Start(processPath, processArguments);
        }

        #endregion
    }

    class ASPASCFeature
    {
        public String Feature { get; set; }
        public Boolean Installed { get; set; }
    }
}
