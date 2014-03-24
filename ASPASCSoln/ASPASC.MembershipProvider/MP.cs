using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.Profile;

using ASPASC.Model;
using ASPASC.DataLayer;
using ASPASC.Utilities;

namespace ASPASC.MembershipProvider
{
    public class ASPASCMP : SqlMembershipProvider
    {
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            try
            {
                base.Initialize(name, config);

                // Update the private connection string field in the base class.
                string connectionString =
                    Properties.Settings.Default.DatabaseUserName.Length == 0 ?
                        String.Format("data source={0};Initial Catalog={1};Integrated Security=True;",
                            Properties.Settings.Default.DatabaseServer,
                            Properties.Settings.Default.DatabaseName) :
                        String.Format("Server={0};Database={1};User Id={2};Password={3}",
                            Properties.Settings.Default.DatabaseServer,
                            Properties.Settings.Default.DatabaseName,
                            Properties.Settings.Default.DatabaseUserName,
                            Encryption.Decrypt(Properties.Settings.Default.DatabasePassword, "^&@NJS&*^Dnas&s8Kas^&8"));
                // Set private property of Membership provider.  
                FieldInfo connectionStringField = GetType().BaseType.GetField("_sqlConnectionString", BindingFlags.Instance | BindingFlags.NonPublic);
                connectionStringField.SetValue(this, connectionString);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.InitializeSQLMembershipProvider");
            }
        }
    }

    public class ASPASCRP : SqlRoleProvider
    {
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            try
            {
                base.Initialize(name, config);

                // Update the private connection string field in the base class.
                string connectionString =
                    Properties.Settings.Default.DatabaseUserName.Length == 0 ?
                        String.Format("data source={0};Initial Catalog={1};Integrated Security=True;",
                            Properties.Settings.Default.DatabaseServer,
                            Properties.Settings.Default.DatabaseName) :
                        String.Format("Server={0};Database={1};User Id={2};Password={3}",
                            Properties.Settings.Default.DatabaseServer,
                            Properties.Settings.Default.DatabaseName,
                            Properties.Settings.Default.DatabaseUserName,
                            Encryption.Decrypt(Properties.Settings.Default.DatabasePassword, "^&@NJS&*^Dnas&s8Kas^&8"));
                // Set private property of Membership provider.  
                FieldInfo connectionStringField = GetType().BaseType.GetField("_sqlConnectionString", BindingFlags.Instance | BindingFlags.NonPublic);
                connectionStringField.SetValue(this, connectionString);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.InitializeSQLRoleProvider");
            }
        }
    }

    public static class Encryption
    {
        public static string Encrypt(string input, string key)
        {
            try
            {
                byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.Encrypt");
                return "";
            }
        }

        public static string Decrypt(string input, string key)
        {
            try
            {
                byte[] inputArray = Convert.FromBase64String(input);
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.Decrypt");
                return "";
            }
        }
    }

    public class ASPACMembProvider
    {
        #region "Properties"

        private DL _DL
        {
            get
            {
                if (Properties.Settings.Default.DatabaseUserName.Length == 0)
                    return new DL(Properties.Settings.Default.DatabaseServer,
                                  Properties.Settings.Default.DatabaseName);
                else
                    return new DL(Properties.Settings.Default.DatabaseServer,
                                  Properties.Settings.Default.DatabaseName,
                                  Properties.Settings.Default.DatabaseUserName,
                                  Encryption.Decrypt(Properties.Settings.Default.DatabasePassword, "^&@NJS&*^Dnas&s8Kas^&8"));
            }
        }

        public Boolean PasswordQuestionAndAnswerEnabled
        {
            get
            {
                return Membership.RequiresQuestionAndAnswer;
            }
        }

        #endregion

        public ASPACMembProvider(String applicationName)
        {
            if (applicationName.Length > 0 && !applicationName.StartsWith("["))
            {
                if (membershipEnabled())
                    Membership.ApplicationName = applicationName;

                if (rolesEnabled())
                    Roles.ApplicationName = applicationName;
            }
        }

        #region "Settings"

        public void saveSettings(String dbServer, String dbName, String dbUserName, String dbPassword)
        {
            try
            {
                Properties.Settings.Default.DatabaseServer = dbServer;
                Properties.Settings.Default.DatabaseName = dbName;
                Properties.Settings.Default.DatabaseUserName = dbUserName;
                Properties.Settings.Default.DatabasePassword = dbPassword.Length > 0 ? Encryption.Encrypt(dbPassword, "^&@NJS&*^Dnas&s8Kas^&8") : "";

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.SaveSettings");
            }
        }

        public Boolean checkConnection()
        {
            return _DL.checkConnection();
        }

        public Boolean rolesEnabled()
        {
            return _DL.rolesEnabled();
        }

        public Boolean membershipEnabled()
        {
            return _DL.membershipEnabled();
        }

        public Boolean profileEnabled()
        {
            return _DL.profileEnabled();
        }

        public Boolean webEventsEnabled()
        {
            return _DL.webEventsEnabled();
        }

        public Boolean personalizationEnabled()
        {
            return _DL.personalizationEnabled();
        }

        #endregion

        #region "Applications"

        public List<ASPASCApplication> getApplications()
        {
            return _DL.getApplications();
        }

        public Boolean application_add(String applicationName)
        {
            return _DL.application_add(applicationName);
        }

        public Boolean application_add(String applicationName, String connectionString)
        {
            return _DL.application_add(applicationName, connectionString);
        }

        #endregion

        #region "Users"

        public List<ASPASCUser> getAllUsers()
        {
            List<ASPASCUser> retVal = new List<ASPASCUser>();

            try
            {
                MembershipUserCollection users = Membership.GetAllUsers();


                foreach (MembershipUser u in users)
                {
                    retVal.Add(new ASPASCUser()
                        {
                            Approved = u.IsApproved,
                            Comment = u.Comment,
                            CreatedDate = u.CreationDate,
                            Email = u.Email,
                            LastActivity = u.LastActivityDate,
                            LastLockoutDate = u.LastLockoutDate,
                            LastLoginDate = u.LastLoginDate,
                            LastPasswordChange = u.LastPasswordChangedDate,
                            LockedOut = u.IsLockedOut,
                            PasswordQuestion = u.PasswordQuestion,
                            UserName = u.UserName
                        });
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.getAllUsers");
            }

            return retVal;
        }

        public ASPASCUser users_get(String userName)
        {
            try
            {
                MembershipUser u = Membership.GetUser(userName);

                ASPASCUser retVal = new ASPASCUser()
                {
                    Approved = u.IsApproved,
                    Comment = u.Comment,
                    CreatedDate = u.CreationDate,
                    Email = u.Email,
                    LastActivity = u.LastActivityDate,
                    LastLockoutDate = u.LastLockoutDate,
                    LastLoginDate = u.LastLoginDate,
                    LastPasswordChange = u.LastPasswordChangedDate,
                    LockedOut = u.IsLockedOut,
                    PasswordQuestion = u.PasswordQuestion,
                    UserName = u.UserName
                };

                return retVal;
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_get");
                return new ASPASCUser();
            }
        }

        public List<ASPASCRole> users_get_roles(String userName)
        {
            List<ASPASCRole> retVal = new List<ASPASCRole>();

            try
            {
                foreach (String role in Roles.GetRolesForUser(userName))
                {
                    retVal.Add(new ASPASCRole()
                        {
                            Name = role
                        });
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_get_roles");
            }

            return retVal;
        }

        public Boolean users_add_to_role(String userName, String roleName)
        {
            try
            {
                Roles.AddUserToRole(userName, roleName);

                return true;
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_add_to_role");
                return false;
            }
        }

        public Boolean users_remove_from_role(String userName, String roleName)
        {
            try
            {
                Roles.RemoveUserFromRole(userName, roleName);

                return true;
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_remove_from_role");
                return false;
            }
        }

        public Boolean users_add(String userName, String password, String email, String passwordQuestion, String passwordAnswer, Boolean enabled, ref String msg)
        {
            MembershipCreateStatus status = MembershipCreateStatus.Success;

            try
            {
                if (Membership.RequiresQuestionAndAnswer)
                    Membership.CreateUser(userName, password, email, passwordQuestion, passwordAnswer, enabled, out status);
                else
                {
                    try
                    {
                        Membership.CreateUser(userName, password, email);
                    }
                    catch (MembershipCreateUserException ex)
                    {
                        status = ex.StatusCode;
                    }
                }

                switch (status)
                {
                    case MembershipCreateStatus.DuplicateEmail:
                        msg = String.Format("User {0} was not created. The given email ({1}) has already been registered to another user.", userName, email);
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        msg = String.Format("User {0} was not created. The given username already exists.", userName);
                        break;
                    case MembershipCreateStatus.InvalidAnswer:
                        msg = String.Format("User {0} was not created. Invalid answer to the password question.", userName);
                        break;
                    case MembershipCreateStatus.InvalidEmail:
                        msg = String.Format("User {0} was not created. Invalid email ({1}).", userName, email);
                        break;
                    case MembershipCreateStatus.InvalidPassword:
                        msg = String.Format("User {0} was not created. Invalid password. Please check complexity rules.", userName);
                        break;
                    case MembershipCreateStatus.InvalidQuestion:
                        msg = String.Format("User {0} was not created. Invalid password question.", userName);
                        break;
                    case MembershipCreateStatus.InvalidUserName:
                        msg = String.Format("User {0} was not created. Invalid username.", userName);
                        break;
                    case MembershipCreateStatus.ProviderError:
                        msg = String.Format("User {0} was not created. Failed to create user in designated provider. Please check settings and try again.", userName);
                        break;
                    case MembershipCreateStatus.Success:
                        msg = String.Format("User {0} was created succesfully.", userName);
                        break;
                    case MembershipCreateStatus.UserRejected:
                        msg = String.Format("User {0} was not created. Rejected.", userName);
                        break;
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_add");
            }

            return status == MembershipCreateStatus.Success;
        }

        public Boolean users_change_password(String userName, String oldPassword, String newPassword, ref String generatedPassword)
        {
            try
            {
                MembershipUser user = Membership.GetUser(userName);

                if (user != null)
                {
                    if (oldPassword.Length == 0)
                    {
                        if (newPassword.Length == 0)
                        {
                            oldPassword = user.ResetPassword();
                            generatedPassword = oldPassword;
                        }
                        else
                        {
                            oldPassword = user.ResetPassword();

                            Membership.UpdateUser(user);

                            user.ChangePassword(oldPassword, newPassword);
                        }
                    }
                    else
                    {
                        if (newPassword.Length > 0)
                            user.ChangePassword(oldPassword, newPassword);
                        else
                        {
                            newPassword = user.ResetPassword();
                            generatedPassword = newPassword;
                        }
                    }

                    Membership.UpdateUser(user);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_change_password");
                return false;
            }
        }

        public Boolean users_unlock(String userName)
        {
            try
            {
                MembershipUser user = Membership.GetUser(userName);

                if (user != null)
                {
                    user.UnlockUser();

                    Membership.UpdateUser(user);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_unlock");
                return false;
            }
        }

        public Boolean users_delete(String userName)
        {
            try
            {
                return Membership.DeleteUser(userName);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_delete");
                return false;
            }
        }

        public Boolean users_change(String userName, String email, String comments, Boolean approved)
        {
            try
            {
                MembershipUser user = Membership.GetUser(userName);

                user.Email = email;
                user.Comment = comments;
                user.IsApproved = approved;

                Membership.UpdateUser(user);

                return true;
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.users_change");
                return false;
            }
        }

        #endregion

        #region "Roles"

        public void roles_add(string roleName)
        {
            try
            {
                Roles.CreateRole(roleName);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.roles_add");
            }
        }

        public Boolean roles_delete(String roleName)
        {
            try
            {
                return Roles.DeleteRole(roleName, false);
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.roles_delete");
                return false;
            }
        }

        public List<ASPASCUser> roles_get_users(String roleName)
        {
            List<ASPASCUser> retVal = new List<ASPASCUser>();

            try
            {
                foreach (string user in Roles.GetUsersInRole(roleName))
                {
                    retVal.Add(new ASPASCUser()
                        {
                            UserName = user
                        });
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.roles_get_users");
            }

            return retVal;
        }

        public List<ASPASCRole> roles_get()
        {
            List<ASPASCRole> retVal = new List<ASPASCRole>();

            try
            {
                String[] roles = Roles.GetAllRoles();

                foreach (String r in roles)
                {
                    retVal.Add(new ASPASCRole()
                        {
                            Name = r
                        });
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "MP.roles_get");
            }

            return retVal;
        }

        #endregion
    }
}
