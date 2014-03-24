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
    /// Interaction logic for User_Add.xaml
    /// </summary>
    public partial class User_Add : Window
    {
        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL(_mw.lblApplicationName.Content.ToString());
            }
        }

        #endregion

        private MainWindow _mw = null;

        public User_Add(MainWindow mw)
        {
            InitializeComponent();
            _mw = mw;
        }

        #region "Button Events"

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_mw != null)
            {
                if (!checkRequiredFields())
                {
                    MessageBox.Show("Please enter the required fields highlighed in Red.", "Error", MessageBoxButton.OK);
                    return;
                }

                if(tbPassword.Password != tbPassword2.Password){
                    MessageBox.Show("Passwords do not match.");
                    return;
                }

                String msg = "";

                Boolean success = _BL.users_add(tbUserName.Text, tbPassword.Password, tbEmail.Text, tbPasswordQuestion.Text, tbPasswordAnswer.Text, cbEnabled.IsChecked.GetValueOrDefault(), ref msg);

                MessageBox.Show(msg);

                if (success)
                    this.DialogResult = success;
            }
        }

        #endregion

        #region "Helpers"

        private Boolean checkRequiredFields()
        {
            Boolean everythingIsOk = true;

            tbUserName.Background = Brushes.White;
            tbPassword.Background = Brushes.White;
            tbPassword2.Background = Brushes.White;
            tbEmail.Background = Brushes.White;

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

            if (tbPassword2.Password.Length == 0)
            {
                everythingIsOk = false;
                tbPassword2.Background = Brushes.Red;
            }

            if (tbEmail.Text.Length == 0)
            {
                everythingIsOk = false;
                tbEmail.Background = Brushes.Red;
            }

            return everythingIsOk;
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Boolean passAndQuestReq = _BL.PasswordQuestionAndAnswerEnabled;

            tbPasswordAnswer.IsEnabled = passAndQuestReq;
            tbPasswordQuestion.IsEnabled = passAndQuestReq;
        }
    }
}
