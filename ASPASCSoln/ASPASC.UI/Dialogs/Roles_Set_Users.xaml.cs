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
    /// Interaction logic for Roles_Set_Users.xaml
    /// </summary>
    public partial class Roles_Set_Users : Window
    {
        private String _roleName = "";
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

        public Roles_Set_Users(String roleName, MainWindow mw)
        {
            _roleName = roleName;
            _mw = mw;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            retrieveRoleInformation();
            lblRoleName.Content = _roleName;
        }

        #region "Helpers"

        private void retrieveRoleInformation()
        {
            List<ASPASCUser> allUsers = _BL.getApplicationUsers();
            List<ASPASCUser> roleUsers = _BL.roles_get_users(_roleName);

            List<ASPASCUser> availableUsers = allUsers.Where(p => roleUsers.Where(q => q.UserName == p.UserName).Count() == 0).ToList();

            lvAvailable.ItemsSource = availableUsers;
            lvOwned.ItemsSource = roleUsers;
        }

        #endregion

        #region "Button Events"

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lvAvailable.SelectedIndex >= 0)
            {
                ASPASCUser user = (ASPASCUser)lvAvailable.SelectedValue;

                if (_BL.user_add_to_role(user.UserName, _roleName))
                    retrieveRoleInformation();
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lvOwned.SelectedIndex >= 0)
            {
                ASPASCUser user = (ASPASCUser)lvOwned.SelectedValue;

                if (_BL.user_remove_from_role(user.UserName, _roleName))
                    retrieveRoleInformation();
            }
        }

        #endregion
    }
}
