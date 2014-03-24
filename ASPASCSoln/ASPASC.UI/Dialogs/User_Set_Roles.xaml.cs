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
using ASPASC.Model;

namespace ASPASC.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for User_Set_Roles.xaml
    /// </summary>
    public partial class User_Set_Roles : Window
    {
        private String _userName = "";
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

        public User_Set_Roles(String userName, MainWindow mw)
        {
            _userName = userName;
            _mw = mw;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblUserName.Content = _userName;
            retrieveRoleInformation();
        }

        #region "Helpers"

        private void retrieveRoleInformation()
        {
            List<ASPASCRole> allRoles = _BL.roles_get();
            List<ASPASCRole> userRoles = _BL.user_get_roles(_userName);

            List<ASPASCRole> availableRoles = allRoles.Where(p => userRoles.Where(q => q.Name == p.Name).Count() == 0).ToList();

            lvAvailable.ItemsSource = availableRoles;
            lvOwned.ItemsSource = userRoles;
        }

        #endregion

        #region "Button Events"

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lvAvailable.SelectedIndex >= 0)
            {
                ASPASCRole role = (ASPASCRole)lvAvailable.SelectedValue;

                if (_BL.user_add_to_role(_userName, role.Name))
                    retrieveRoleInformation();
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lvOwned.SelectedIndex >= 0)
            {
                ASPASCRole role = (ASPASCRole)lvOwned.SelectedValue;

                if (_BL.user_remove_from_role(_userName, role.Name))
                    retrieveRoleInformation();
            }
        }

        #endregion
    }
}
