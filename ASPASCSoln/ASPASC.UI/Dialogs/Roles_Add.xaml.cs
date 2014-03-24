using ASPASC.BusinessLayer;
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

namespace ASPASC.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for Roles_Add.xaml
    /// </summary>
    public partial class Roles_Add : Window
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

        public Roles_Add(MainWindow mw)
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

                String retMsg = "";

                if (_BL.roles_add(tbRoleName.Text, ref retMsg))
                {
                    MessageBox.Show(retMsg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show(retMsg, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region "Helpers"

        private Boolean checkRequiredFields()
        {
            Boolean everythingIsOk = true;

            tbRoleName.Background = Brushes.White;

            if (tbRoleName.Text.Length == 0)
            {
                everythingIsOk = false;
                tbRoleName.Background = Brushes.Red;
            }

            return everythingIsOk;
        }

        #endregion


    }
}
