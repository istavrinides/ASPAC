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

namespace ASPASC.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for User_Set_Password.xaml
    /// </summary>
    public partial class User_Set_Password : Window
    {
        private String _userName = "";
        public MainWindow _mw = null;

        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL(_mw.lblApplicationName.Content.ToString());
            }
        }

        #endregion

        public User_Set_Password(String userName, MainWindow mw)
        {
            _userName = userName;
            _mw = mw;
            InitializeComponent();
        }

        #region "Window Events"

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblUsername.Content = _userName;
        }

        #endregion

        #region "Button Events"

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (_mw != null)
            {
                if (!checkRequiredFields())
                {
                    MessageBox.Show("Please enter the required fields highlighed in Red.", "Error", MessageBoxButton.OK);
                    return;
                }

                String generatedPassword = "";

                if (_BL.users_change_password(_userName, tbOldPassword.Password, tbNewPassword.Password, ref generatedPassword))
                {
                    if (cbRandomPassword.IsChecked.GetValueOrDefault())
                    {
                        MessageBox.Show(String.Format("The new password for user {0} is {1}", _userName, generatedPassword), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Succesfully changes password of user {0} to {1}", _userName, tbNewPassword.Password), "Information", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                        this.Close();
                    }
                }
            }
        }

        #endregion

        #region "Checkbox Events"

        private void cbNoOldPassword_CheckedChanged(object sender, RoutedEventArgs e)
        {
            tbOldPassword.IsEnabled = !cbNoOldPassword.IsChecked.GetValueOrDefault();

            if (cbNoOldPassword.IsChecked.GetValueOrDefault())
                tbOldPassword.Clear();
        }

        private void cbRandomPassword_CheckedChanged(object sender, RoutedEventArgs e)
        {
            tbNewPassword.IsEnabled = !cbRandomPassword.IsChecked.GetValueOrDefault();

            if (cbRandomPassword.IsChecked.GetValueOrDefault())
                tbNewPassword.Clear();
        }

        #endregion

        #region "Helpers"

        private Boolean checkRequiredFields()
        {
            Boolean everythingIsOk = true;

            tbNewPassword.Background = Brushes.White;
            tbOldPassword.Background = Brushes.White;

            if (tbNewPassword.Password.Length == 0 && !cbRandomPassword.IsChecked.GetValueOrDefault())
            {
                everythingIsOk = false;
                tbNewPassword.Background = Brushes.Red;
            }

            if (tbOldPassword.Password.Length == 0 && !cbNoOldPassword.IsChecked.GetValueOrDefault())
            {
                everythingIsOk = false;
                tbOldPassword.Background = Brushes.Red;
            }

            return everythingIsOk;
        }

        #endregion
    }
}
