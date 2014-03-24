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
using ASPASC.MembershipProvider;

namespace ASPASC.UI.Settings
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public MainWindow mw = null;

        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL(mw.lblApplicationName.Content.ToString());
            }
        }

        private ASPACMembProvider _MP
        {
            get
            {
                return new ASPACMembProvider("");
            }
        }

        #endregion

        public Settings()
        {
            InitializeComponent();
        }

        #region "Checkbox Events"

        private void cbWindowsAuth_Checked(object sender, RoutedEventArgs e)
        {
            tbUserName.IsEnabled = !cbWindowsAuth.IsChecked.GetValueOrDefault();
            tbPassword.IsEnabled = !cbWindowsAuth.IsChecked.GetValueOrDefault();
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

            // Save the database server name and the database name
            // If windows authentication checked, then clear the database username setting
            // else, set the setting with the given username
            if (cbWindowsAuth.IsChecked.GetValueOrDefault())
                _MP.saveSettings(tbDBServer.Text, tbDatabaseName.Text, "", "");
            else
                _MP.saveSettings(tbDBServer.Text, tbDatabaseName.Text, tbUserName.Text, tbPassword.Password);
            
            // Check the connection
            Boolean success = _BL.checkConnection();

            if (success)
            {
                MessageBox.Show("Succesfully connected to database server", "Successful Connection", MessageBoxButton.OK, MessageBoxImage.Information);

                // Persist the settings
                Properties.Settings.Default.Save();

                // Set the window close status
                this.DialogResult = true;

                // Close the window
                this.Close();
            }
            else
                MessageBox.Show("Could not connect to the database server. Please check that the name/instance given is correct and that the aspnetdb is present on the database server.", "Unsuccesful Connection", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

        #region "Helpers"

        private Boolean checkRequiredFields()
        {
            Boolean everythingIsOk = true;

            tbDatabaseName.Background = Brushes.White;
            tbDBServer.Background = Brushes.White;
            tbUserName.Background = Brushes.White;
            tbPassword.Background = Brushes.White;

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

            return everythingIsOk;
        }

        #endregion

        #region "Window Events"

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseServer != "")
                tbDBServer.Text = ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseServer;

            if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseName != "")
                tbDatabaseName.Text = ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseName;

            if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseUserName != "")
                tbUserName.Text = ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseUserName;
            else
                cbWindowsAuth.IsChecked = true;
        }

        #endregion
        
    }
}
