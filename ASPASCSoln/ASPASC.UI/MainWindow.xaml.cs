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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ASPASC.MembershipProvider;
using ASPASC.Model;
using ASPASC.BusinessLayer;
using ASPASC.Utilities;
using System.Text.RegularExpressions;

namespace ASPASC.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public String DatabasePassword = "";

        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL(lblApplicationName.Content.ToString());
            }
        }

        #endregion

        private Guid selectedApplication = Guid.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region "Menu Item Events"

        private void miFile_Click(object sender, RoutedEventArgs e)
        {
            Settings.Settings _win = new Settings.Settings();
            _win.mw = this;

            if (_win.ShowDialog().GetValueOrDefault())
            {
                generateApplicationMenuItem(true);

                showConnectionInformation(configurationChange: true);

                findEnabledFeatures();
            }
        }

        private void miConnect_Click(object sender, RoutedEventArgs e)
        {
            generateApplicationMenuItem(false);

            showConnectionInformation(configurationChange: false);

            findEnabledFeatures();
        }

        private void miCreate_Click(object sender, RoutedEventArgs e)
        {
            Settings.Database_Add _w = new Settings.Database_Add();

            _w.ShowDialog();
        }

        private void miAddApplication_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.Application_Add w = new Dialogs.Application_Add();

            if (w.ShowDialog().GetValueOrDefault())
            {
                generateApplicationMenuItem(true);
            }
        }

        private void miApplicationItem_Click(object sender, RoutedEventArgs e)
        {
            selectedApplication = Guid.Parse(((MenuItem)sender).Tag.ToString());
            lblApplicationName.Content = ((MenuItem)sender).Header;

            selectApplication();

            foreach (var mi in ((MenuItem)mainMenu.Items[1]).Items)
            {
                if (!(mi is Separator))
                {
                    if (((MenuItem)mi).Header.ToString() == lblApplicationName.Content.ToString())
                        ((MenuItem)mi).Icon = new System.Windows.Controls.Image
                            {
                                Source = new BitmapImage(new Uri("Images/selected.png", UriKind.Relative))
                            };
                    else
                        ((MenuItem)mi).Icon = null;
                }
            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            Settings.Features_Edit _w = new Settings.Features_Edit(this);

            _w.ShowDialog();

            findEnabledFeatures();
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            About _w = new About();

            _w.ShowDialog();
        }

        #endregion

        #region "Button Events"

        private void btnDeleteRole_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the selected role?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                String roleName = ((Button)sender).Tag.ToString();

                if (_BL.roles_get_users(roleName).Count() > 0)
                    if (MessageBox.Show("There are users assigned to this role. Remove users from role?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        return;

                if (_BL.roles_delete(roleName))
                {
                    MessageBox.Show("Role deleted succesfully.", "Successful deletion", MessageBoxButton.OK);

                    tviRoles.Items.Clear();

                    List<ASPASCRole> roles = _BL.roles_get();

                    foreach (var role in roles)
                    {
                        generateRoleTreeViewItem(role);
                    }

                    spDetails.Children.Clear();
                }
                else
                    MessageBox.Show("Could not delete role.", "Unsuccessful deletion", MessageBoxButton.OK);

            }
        }

        private void btnSaveUser_Click(object sender, RoutedEventArgs e)
        {
            String userName = ((Button)sender).Tag.ToString();

            String email = ((TextBox)((StackPanel)((StackPanel)((StackPanel)spDetails.Children[2]).Children[1]).Children[1]).Children[1]).Text;
            String comment = ((TextBox)((StackPanel)((StackPanel)((StackPanel)spDetails.Children[2]).Children[1]).Children[2]).Children[1]).Text;
            Boolean approved = ((CheckBox)((StackPanel)((StackPanel)((StackPanel)spDetails.Children[2]).Children[1]).Children[3]).Children[0]).IsChecked.GetValueOrDefault();

            if (_BL.users_change(userName, email, comment, approved))
            {
                MessageBox.Show("Changes saved succesfully.");

                TreeViewItem tvi = new TreeViewItem() { Tag = ">>>>>" + userName };
                miUser_Click(tvi, null);

                tviUsers.Items.Clear();

                List<ASPASCUser> users = _BL.getApplicationUsers();

                foreach (var user in users)
                {
                    generateUserTreeViewItem(user);
                }
            }
            else
            {
                MessageBox.Show("Could not save changes");
            }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the selected user?", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                String userName = ((Button)sender).Tag.ToString();

                if (_BL.user_delete(userName))
                {
                    MessageBox.Show("User deleted succesfully.", "Successful deletion", MessageBoxButton.OK);

                    tviUsers.Items.Clear();

                    List<ASPASCUser> users = _BL.getApplicationUsers();

                    foreach (var user in users)
                    {
                        generateUserTreeViewItem(user);
                    }

                    spDetails.Children.Clear();
                }
                else
                    MessageBox.Show("Could not delete user deleted succesfully.", "Unsuccessful deletion", MessageBoxButton.OK);

            }
        }

        private void btnResetUserPassword_Click(object sender, RoutedEventArgs e)
        {
            String userName = ((Button)sender).Tag.ToString();

            Dialogs.User_Set_Password _w = new Dialogs.User_Set_Password(userName, this);
            _w.ShowDialog();
        }

        void btnEditUserRoles_Click(object sender, RoutedEventArgs e)
        {
            String userName = ((Button)sender).Tag.ToString();

            Dialogs.User_Set_Roles _w = new Dialogs.User_Set_Roles(userName, this);
            _w.ShowDialog();

            TreeViewItem tvi = new TreeViewItem() { Tag = ">>>>>" + userName };
            miUser_Click(tvi, null);
        }

        void btnEditRoleUsers_Click(object sender, RoutedEventArgs e)
        {
            String roleName = ((Button)sender).Tag.ToString();

            Dialogs.Roles_Set_Users _w = new Dialogs.Roles_Set_Users(roleName, this);
            _w.ShowDialog();

            TreeViewItem tvi = new TreeViewItem() { Tag = roleName };
            miRole_Click(tvi, null);
        }

        #endregion

        #region "TreeView Item Events"

        private void miAddNewRole_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.Roles_Add win = new Dialogs.Roles_Add(this);
            win.ShowDialog();

            tviRoles.Items.Clear();

            List<ASPASCRole> roles = _BL.roles_get();

            foreach (var role in roles)
            {
                generateRoleTreeViewItem(role);
            }
        }

        private void miAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.User_Add w = new Dialogs.User_Add(this);

            if (w.ShowDialog().GetValueOrDefault())
            {
                tviUsers.Items.Clear();

                List<ASPASCUser> users = _BL.getApplicationUsers();

                foreach (var user in users)
                {
                    generateUserTreeViewItem(user);
                }
            }
        }

        private void miResetUserPassword_Click(object sender, RoutedEventArgs e)
        {
            MenuItem s = (MenuItem)sender;

            Dialogs.User_Set_Password _win = new Dialogs.User_Set_Password(s.Tag.ToString(), this);
            _win.ShowDialog();
        }

        private void miLockUnlockUser_Click(object sender, RoutedEventArgs e)
        {
            MenuItem s = (MenuItem)sender;

            if (_BL.users_unlock(s.Tag.ToString()))
            {
                MessageBox.Show(String.Format("User {0} unlocked succesfully.", s.Tag.ToString()), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(String.Format("Could not unlock user {0}.", s.Tag.ToString()), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void miUser_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;

            String userName = Regex.Split(tvi.Tag.ToString(), ">>>>>")[1];

            ASPASCUser user = _BL.users_get(userName);

            #region "Panel definitions"

            StackPanel upper = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            StackPanel upper_left = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 280
            };

            StackPanel upper_right = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 280
            };

            StackPanel lower = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Width = 560,
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel lower_left = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 280
            };

            StackPanel lower_right = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 280
            };

            StackPanel lower_roles = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Width = 560,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Grid lower_buttons = new Grid()
            {
                Width = 560,
                Margin = new Thickness(0, 10, 0, 0)
            };

            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(140) });
            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(140) });

            #endregion

            Image _imgUser = new Image()
            {
                Source = new BitmapImage(new Uri(user.LockedOut ? "Images/user_locked_large.png" : (user.Approved ? "Images/user_large.png" : "Images/user_large_notapproved.png"), UriKind.Relative)),
                Width = 280
            };

            #region "Upper Right Panel User Name"

            StackPanel upper_right_username = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblUsernameText = new Label()
            {
                Content = "Username:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5),
                Width = 80
            };
            TextBox _tbUsername = new TextBox()
            {
                Text = user.UserName,
                Margin = new Thickness(5),
                Width = 180,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23,
                IsEnabled = false
            };

            upper_right_username.Children.Add(_lblUsernameText);
            upper_right_username.Children.Add(_tbUsername);

            #endregion

            #region "Upper Right Panel Email"

            StackPanel upper_right_email = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblEmailText = new Label()
            {
                Content = "Email:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5),
                Width = 80
            };
            TextBox _tbEmail = new TextBox()
            {
                Text = user.Email,
                Margin = new Thickness(5),
                Width = 180,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23
            };

            upper_right_email.Children.Add(_lblEmailText);
            upper_right_email.Children.Add(_tbEmail);

            #endregion

            #region "Upper Right Panel Comment"

            StackPanel upper_right_comment = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
            Label _lblCommentText = new Label()
                {
                    Content = "Comment:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5),
                    Width = 80
                };
            TextBox _tbComment = new TextBox()
                {
                    Text = user.Comment,
                    Margin = new Thickness(5),
                    Width = 180,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Height = 100,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap
                };

            upper_right_comment.Children.Add(_lblCommentText);
            upper_right_comment.Children.Add(_tbComment);

            #endregion

            #region "Upper Panel Right Approved"

            StackPanel upper_right_approved = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            CheckBox _cbApproved = new CheckBox()
            {
                Content = "User Is Approved",
                Margin = new Thickness(94, 5, 5, 5),
                Width = 180,
                IsChecked = user.Approved,
                Foreground = user.Approved ? Brushes.Black : Brushes.Red
            };

            upper_right_approved.Children.Add(_cbApproved);

            #endregion

            #region "Upper Panel Right Locked Out"

            StackPanel upper_right_locked = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            CheckBox _cbLocked = new CheckBox()
            {
                Content = "User Is Locked Out",
                Margin = new Thickness(94, 5, 5, 5),
                Width = 180,
                IsChecked = user.LockedOut,
                IsEnabled = user.LockedOut
            };

            upper_right_locked.Children.Add(_cbLocked);

            #endregion

            #region "Lower Panel Left Locked Out Date"

            StackPanel lower_left_lockout_date = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblLockOutDateText = new Label()
            {
                Content = "Last Lockout:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBlock _tbLockoutDate = new TextBlock()
            {
                Text = user.LastLockoutDate.ToString(),
                Margin = new Thickness(5),
                Width = 150,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23
            };

            lower_left_lockout_date.Children.Add(_lblLockOutDateText);
            lower_left_lockout_date.Children.Add(_tbLockoutDate);

            #endregion

            #region "Lower Panel Left Login Date"

            StackPanel lower_left_login_date = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblLoginDateText = new Label()
            {
                Content = "Last Login:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBlock _tbLoginDate = new TextBlock()
            {
                Text = user.LastLoginDate.ToString(),
                Margin = new Thickness(5),
                Width = 150,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23
            };

            lower_left_login_date.Children.Add(_lblLoginDateText);
            lower_left_login_date.Children.Add(_tbLoginDate);

            #endregion

            #region "Lower Panel Left Activity Date"

            StackPanel lower_left_activity_date = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblActivityDateText = new Label()
            {
                Content = "Last Activity:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBlock _tbActivityDate = new TextBlock()
            {
                Text = user.LastActivity.ToString(),
                Margin = new Thickness(5),
                Width = 150,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23
            };

            lower_left_activity_date.Children.Add(_lblActivityDateText);
            lower_left_activity_date.Children.Add(_tbActivityDate);

            #endregion

            #region "Lower Panel Left Locked Out Date"

            StackPanel lower_right_created_date = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblCreatedDateText = new Label()
            {
                Content = "Created Date:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBlock _tbCreatedDate = new TextBlock()
            {
                Text = user.CreatedDate.ToString(),
                Margin = new Thickness(5),
                Width = 150,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23
            };

            lower_right_created_date.Children.Add(_lblCreatedDateText);
            lower_right_created_date.Children.Add(_tbCreatedDate);

            #endregion

            #region "Lower Panel Right Password Change Date"

            StackPanel lower_right_pass_change_date = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblPassChangeDateText = new Label()
            {
                Content = "Pwd Changed:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBlock _tbPassChangeDate = new TextBlock()
            {
                Text = user.LastPasswordChange.ToString(),
                Margin = new Thickness(5),
                Width = 150,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23
            };

            lower_right_pass_change_date.Children.Add(_lblPassChangeDateText);
            lower_right_pass_change_date.Children.Add(_tbPassChangeDate);

            #endregion

            #region "Lower Panel Roles"

            List<ASPASCRole> roles = _BL.user_get_roles(userName);

            Label _lblRolesText = new Label()
            {
                Content = "Roles:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBox _tbRoles = new TextBox()
            {
                Text = String.Join(", ", roles.Select(x => x.Name)),
                Margin = new Thickness(5),
                Width = 400,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 50,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true
            };
            Button _btnRolesEdits = new Button()
            {
                Content = new Image()
                    {
                        Source = new BitmapImage(new Uri("Images/group_edit.png", UriKind.Relative))
                    },
                Width = 30,
                Height = 30,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 0, 0),
                Tag = userName
            };
            _btnRolesEdits.Click += btnEditUserRoles_Click;

            #endregion

            #region "Lower Panel Buttons"

            Button _btnSave = new Button()
            {
                Content = "Save",
                Width = 70,
                Height = 25,
                Tag = user.UserName
            };
            _btnSave.Click += btnSaveUser_Click;

            Button _btnDelete = new Button()
            {
                Content = "Delete",
                Width = 70,
                Height = 25,
                Tag = user.UserName
            };
            _btnDelete.Click += btnDeleteUser_Click;

            Button _btnResetPassword = new Button()
            {
                Content = "Reset Password",
                Width = 110,
                Height = 25,
                Tag = user.UserName
            };
            _btnResetPassword.Click += btnResetUserPassword_Click;

            Grid.SetColumn(_btnSave, 1);
            Grid.SetColumn(_btnDelete, 2);
            Grid.SetColumn(_btnResetPassword, 3);

            #endregion

            upper_left.Children.Add(_imgUser);

            upper_right.Children.Add(upper_right_username);
            upper_right.Children.Add(upper_right_email);
            upper_right.Children.Add(upper_right_comment);
            upper_right.Children.Add(upper_right_approved);
            upper_right.Children.Add(upper_right_locked);

            lower_left.Children.Add(lower_left_lockout_date);
            lower_left.Children.Add(lower_left_login_date);
            lower_left.Children.Add(lower_left_activity_date);

            lower_right.Children.Add(lower_right_created_date);
            lower_right.Children.Add(lower_right_pass_change_date);

            upper.Children.Add(upper_left);
            upper.Children.Add(upper_right);

            lower.Children.Add(lower_left);
            lower.Children.Add(lower_right);

            lower_roles.Children.Add(_lblRolesText);
            lower_roles.Children.Add(_tbRoles);
            lower_roles.Children.Add(_btnRolesEdits);

            lower_buttons.Children.Add(_btnSave);
            lower_buttons.Children.Add(_btnDelete);
            lower_buttons.Children.Add(_btnResetPassword);

            spDetails.Children.Clear();
            spDetails.Children.Add(new TextBlock() { Text = "User Details", FontWeight = FontWeights.Bold, FontSize = 20, Margin = new Thickness(25, 0, 0, 0) });
            spDetails.Children.Add(new Separator());
            spDetails.Children.Add(upper);
            spDetails.Children.Add(lower);
            spDetails.Children.Add(lower_roles);
            spDetails.Children.Add(lower_buttons);

        }

        private void miRole_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;

            String roleName = tvi.Tag.ToString();

            #region "Panel definitions"

            StackPanel upper = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            StackPanel upper_left = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 280
            };

            StackPanel upper_right = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = 280
            };

            StackPanel lower = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Width = 560,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Grid lower_buttons = new Grid()
            {
                Width = 560,
                Margin = new Thickness(0, 10, 0, 0)
            };

            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(160) });
            lower_buttons.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });

            #endregion

            Image _imgRole = new Image()
            {
                Source = new BitmapImage(new Uri("Images/group_large.png", UriKind.Relative)),
                Width = 280
            };

            #region "Upper Right Panel Role Name"

            StackPanel upper_right_rolename = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            Label _lblRolenameText = new Label()
            {
                Content = "Rolename:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5),
                Width = 80
            };
            TextBox _tbRolename = new TextBox()
            {
                Text = roleName,
                Margin = new Thickness(5),
                Width = 180,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 23,
                IsEnabled = false
            };

            upper_right_rolename.Children.Add(_lblRolenameText);
            upper_right_rolename.Children.Add(_tbRolename);

            #endregion

            #region "Lower Panel Users"

            List<ASPASCUser> users = _BL.roles_get_users(roleName);

            Label _lblRolesText = new Label()
            {
                Content = "Users in Role:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 0, 0, 0),
                Width = 100
            };
            TextBox _tbRoles = new TextBox()
            {
                Text = String.Join(", ", users.Select(x => x.UserName)),
                Margin = new Thickness(5),
                Width = 400,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Height = 50,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true
            };
            Button _btnRolesEdits = new Button()
            {
                Content = new Image()
                {
                    Source = new BitmapImage(new Uri("Images/user_add.png", UriKind.Relative))
                },
                Width = 30,
                Height = 30,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 0, 0),
                Tag = roleName
            };
            _btnRolesEdits.Click += btnEditRoleUsers_Click;

            #endregion

            #region "Lower Panel Buttons"

            Button _btnDelete = new Button()
            {
                Content = "Delete",
                Width = 70,
                Height = 25,
                Tag = roleName
            };
            _btnDelete.Click += btnDeleteRole_Click;

            Grid.SetColumn(_btnDelete, 1);

            #endregion

            upper_left.Children.Add(_imgRole);

            upper_right.Children.Add(upper_right_rolename);

            upper.Children.Add(upper_left);
            upper.Children.Add(upper_right);

            lower.Children.Add(_lblRolesText);
            lower.Children.Add(_tbRoles);
            lower.Children.Add(_btnRolesEdits);

            lower_buttons.Children.Add(_btnDelete);

            spDetails.Children.Clear();
            spDetails.Children.Add(new TextBlock() { Text = "Role Details", FontWeight = FontWeights.Bold, FontSize = 20, Margin = new Thickness(25, 0, 0, 0) });
            spDetails.Children.Add(new Separator());
            spDetails.Children.Add(upper);
            spDetails.Children.Add(lower);
            spDetails.Children.Add(lower_buttons);
        }

        #endregion

        #region "Helpers"

        private void generateApplicationMenuItem(Boolean refresh)
        {
            if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseServer != "")
            {
                if (mainMenu.Items.Count == 2 || (mainMenu.Items[1] as MenuItem).Name != "Applications" || refresh)
                {
                    if (refresh && mainMenu.Items.Count > 2)
                        mainMenu.Items.RemoveAt(1);

                    MenuItem mi = new MenuItem();
                    mi.Name = "Applications";
                    mi.Header = "_Applications";

                    List<ASPASC.Model.ASPASCApplication> apps = _BL.getApplications();

                    Boolean firstApp = true;

                    foreach (var app in apps)
                    {
                        MenuItem child = new MenuItem();
                        child.Tag = app.Id;
                        child.Header = app.Name;

                        if (firstApp)
                        {
                            child.Icon = new System.Windows.Controls.Image
                                {
                                    Source = new BitmapImage(new Uri("Images/selected.png", UriKind.Relative))
                                };
                            firstApp = false;
                        }

                        child.Click += miApplicationItem_Click;

                        mi.Items.Add(child);
                    }

                    Separator s = new Separator();
                    mi.Items.Add(s);

                    MenuItem addApplication = new MenuItem()
                    {
                        Name = "addApplication",
                        Header = "Add Application"
                    };
                    addApplication.Click += miAddApplication_Click;
                    mi.Items.Add(addApplication);

                    mainMenu.Items.Insert(1, mi);
                }

                selectApplication();
            }
        }

        private void selectApplication()
        {
            // Get the first application on the list
            if (selectedApplication == Guid.Empty)
            {
                selectedApplication = Guid.Parse(((mainMenu.Items[1] as MenuItem).Items[0] as MenuItem).Tag.ToString());
                lblApplicationName.Content = ((mainMenu.Items[1] as MenuItem).Items[0] as MenuItem).Header;
            }

            tviRoles.Items.Clear();
            tviUsers.Items.Clear();

            List<ASPASCUser> users = _BL.getApplicationUsers();

            foreach (var user in users)
            {
                generateUserTreeViewItem(user);
            }

            if (_BL.rolesEnabled())
            {
                List<ASPASCRole> roles = _BL.roles_get();

                foreach (var role in roles)
                {
                    generateRoleTreeViewItem(role);
                }

                tviRoles.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                tviRoles.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void enableDisableContextMenuItems(Boolean enable)
        {
            miAddNewRole.IsEnabled = enable;
            miAddNewUser.IsEnabled = true;
        }

        private void showConnectionInformation(Boolean configurationChange)
        {
            if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseServer != "")
            {
                miConnect.IsEnabled = true;

                lblDBServer.Content = ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseServer;
                lblDBName.Content = ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseName;

                if (ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseUserName != "")
                {
                    lblUserName.Content = ASPASC.MembershipProvider.Properties.Settings.Default.DatabaseUserName;
                    lblUserNameLabel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    lblUserNameLabel.Visibility = System.Windows.Visibility.Collapsed;
                    lblUserName.Content = "";
                }
            }
            
            Boolean connected = miConnect.Header.ToString() == "_Connect";

            if ((mainMenu.Items[1] as MenuItem).Name == "Applications")
            {
                if (!configurationChange)
                {
                    if (connected)
                    {
                        miConnect.Header = "_Disconnect";
                        miConnect.Icon = new System.Windows.Controls.Image
                        {
                            Source = new BitmapImage(new Uri("Images/disconnect.png", UriKind.Relative))
                        };

                        lblStatus.Content = "Connected";
                        lblStatus.Foreground = Brushes.Green;
                        enableDisableContextMenuItems(true);

                        miEdit.IsEnabled = true;
                    }
                    else if (!connected)
                    {
                        miConnect.Header = "_Connect";
                        miConnect.Icon = new System.Windows.Controls.Image
                        {
                            Source = new BitmapImage(new Uri("Images/connect.png", UriKind.Relative))
                        };

                        if ((mainMenu.Items[1] as MenuItem).Name == "Applications")
                            mainMenu.Items.RemoveAt(1);

                        lblStatus.Content = "Disconnected";
                        lblStatus.Foreground = Brushes.Red;
                        enableDisableContextMenuItems(false);

                        tviRoles.Items.Clear();
                        tviUsers.Items.Clear();

                        miEdit.IsEnabled = false;
                    }
                }
                else
                {
                    if (connected)
                    {
                        miConnect.Header = "_Disconnect";
                        miConnect.Icon = new System.Windows.Controls.Image
                        {
                            Source = new BitmapImage(new Uri("Images/disconnect.png", UriKind.Relative))
                        };

                        lblStatus.Content = "Connected";
                        lblStatus.Foreground = Brushes.Green;
                        enableDisableContextMenuItems(true);

                        miEdit.IsEnabled = true;
                    }
                }
            }
            
        }

        private void generateUserTreeViewItem(ASPASCUser user)
        {
            TreeViewItem tvi = new TreeViewItem();
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            Image img = new Image()
            {
                Source = new BitmapImage(new Uri(user.LockedOut ? "Images/user_locked.png" : (user.Approved ? "Images/user.png" : "Images/user_notapproved.png"), UriKind.Relative))
            };
            Label lbl = new Label()
            {
                Content = user.UserName
            };

            sp.Children.Add(img);
            sp.Children.Add(lbl);

            tvi.Header = sp;
            tvi.Tag = user.Email + ">>>>>" + user.UserName;

            /*
             * Set Password Menu Item
             */
            MenuItem miSetPassword = new MenuItem()
            {
                Header = "Set Password",
                Icon = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri("Images/password.png", UriKind.Relative))
                    },
                Tag = user.UserName
            };

            miSetPassword.Click += miResetUserPassword_Click;



            /*
             * Lock/Unlock Menu Item
             */
            MenuItem miLockUnlock = new MenuItem()
            {
                Header = user.LockedOut ? "Unlock User" : "Lock User",
                Icon = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri("Images/user_locked.png", UriKind.Relative))
                    },
                Tag = user.Email + ">>>>>" + user.UserName

            };

            miLockUnlock.Click += miLockUnlockUser_Click;

            tvi.MouseUp += miUser_Click;
            tvi.ContextMenu = new System.Windows.Controls.ContextMenu();
            tvi.ContextMenu.Items.Add(miSetPassword);
            tvi.ContextMenu.Items.Add(miLockUnlock);


            tviUsers.Items.Add(tvi);
        }

        private void generateRoleTreeViewItem(ASPASCRole role)
        {
            TreeViewItem tvi = new TreeViewItem();
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            Image img = new Image()
            {
                Source = new BitmapImage(new Uri("Images/group.png", UriKind.Relative))
            };
            Label lbl = new Label()
            {
                Content = role.Name
            };

            sp.Children.Add(img);
            sp.Children.Add(lbl);

            tvi.Header = sp;
            tvi.Tag = role.Name;

            tvi.MouseUp += miRole_Click;

            tviRoles.Items.Add(tvi);
        }

        private void findEnabledFeatures()
        {
            Boolean connected = miConnect.Header.ToString() == "_Disconnect";

            if (connected)
            {
                imgProfile.Source = _BL.profileEnabled() ? new BitmapImage(new Uri("Images/ok.png", UriKind.Relative)) : new BitmapImage(new Uri("Images/nok.png", UriKind.Relative));
                imgRole.Source = _BL.rolesEnabled() ? new BitmapImage(new Uri("Images/ok.png", UriKind.Relative)) : new BitmapImage(new Uri("Images/nok.png", UriKind.Relative));
                imgMembership.Source = _BL.membershipEnabled() ? new BitmapImage(new Uri("Images/ok.png", UriKind.Relative)) : new BitmapImage(new Uri("Images/nok.png", UriKind.Relative));
                imgWebEvents.Source = _BL.webEventsEnabled() ? new BitmapImage(new Uri("Images/ok.png", UriKind.Relative)) : new BitmapImage(new Uri("Images/nok.png", UriKind.Relative));
                imgWebParts.Source = _BL.personalizationEnabled() ? new BitmapImage(new Uri("Images/ok.png", UriKind.Relative)) : new BitmapImage(new Uri("Images/nok.png", UriKind.Relative));
            }
            else
            {
                imgProfile.Source = new BitmapImage(new Uri("Images/disabled.png", UriKind.Relative));
                imgRole.Source = new BitmapImage(new Uri("Images/disabled.png", UriKind.Relative));
                imgMembership.Source = new BitmapImage(new Uri("Images/disabled.png", UriKind.Relative));
                imgWebEvents.Source = new BitmapImage(new Uri("Images/disabled.png", UriKind.Relative));
                imgWebParts.Source = new BitmapImage(new Uri("Images/disabled.png", UriKind.Relative));
            }
        }

        #endregion

        #region "Window Events"

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                generateApplicationMenuItem(true);

                showConnectionInformation(configurationChange: false);

                findEnabledFeatures();
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "UI.MainWindow.Loaded");
            }
        }

        #endregion
    }
}
