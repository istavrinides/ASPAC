using ASPASC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPASC.MembershipProvider;

namespace ASPASC.BusinessLayer
{
    public class BL
    {
        #region "Variables"

        private String _applicationName = "";

        #endregion

        #region "Properties"

        private ASPACMembProvider _MP
        {
            get
            {
                return new ASPACMembProvider(_applicationName);
            }
        }

        public Boolean PasswordQuestionAndAnswerEnabled
        {
            get
            {
                return _MP.PasswordQuestionAndAnswerEnabled;
            }
        }

        #endregion

        public BL(String applicationName)
        {
            _applicationName = applicationName;
        }

        #region "Database"

        public Boolean checkConnection()
        {
            return _MP.checkConnection();
        }

        public Boolean rolesEnabled()
        {
            return _MP.rolesEnabled();
        }

        public Boolean membershipEnabled()
        {
            return _MP.membershipEnabled();
        }

        public Boolean profileEnabled()
        {
            return _MP.profileEnabled();
        }

        public Boolean webEventsEnabled()
        {
            return _MP.webEventsEnabled();
        }

        public Boolean personalizationEnabled()
        {
            return _MP.personalizationEnabled();
        }

        #endregion

        #region "Applications"

        public List<ASPASCApplication> getApplications()
        {
            return _MP.getApplications();
        }

        public Boolean application_add(String applicationName)
        {
            return _MP.application_add(applicationName);
        }

        public Boolean application_add(String applicationName, String connectionString)
        {
            return _MP.application_add(applicationName, connectionString);
        }

        #endregion

        #region "Users"

        public List<ASPASCUser> getApplicationUsers()
        {
            return _MP.getAllUsers();
        }

        public ASPASCUser users_get(String userName)
        {
            return _MP.users_get(userName);
        }

        public List<ASPASCRole> user_get_roles(String userName)
        {
            return _MP.users_get_roles(userName);
        }

        public Boolean users_add(String userName, String password, String email, String passwordQuestion, String passwordAnswer, Boolean enabled, ref String msg)
        {
            return _MP.users_add(userName, password, email, passwordQuestion, passwordAnswer, enabled, ref msg);
        }

        public Boolean users_change_password(String userName, String oldPassword, String newPassword, ref String generatedPassword)
        {
            return _MP.users_change_password(userName, oldPassword, newPassword, ref generatedPassword);
        }

        public Boolean users_unlock(String userName)
        {
            return _MP.users_unlock(userName);
        }

        public Boolean user_add_to_role(String userName, String roleName)
        {
            return _MP.users_add_to_role(userName, roleName);
        }

        public Boolean user_remove_from_role(String userName, String roleName)
        {
            return _MP.users_remove_from_role(userName, roleName);
        }

        public Boolean user_delete(String userName)
        {
            return _MP.users_delete(userName);
        }

        public Boolean users_change(String userName, String email, String comments, Boolean approved)
        {
            return _MP.users_change(userName, email, comments, approved);
        }

        #endregion

        #region "Roles"

        public Boolean roles_add(String roleName, ref String msg)
        {
            try
            {
                _MP.roles_add(roleName);

                msg = String.Format("Role {0} created for application {1}.", roleName, _applicationName);
                return true;
            }
            catch
            {

                msg = String.Format("The given role name ({0}) already exists for application {1}.", roleName, _applicationName);
                return false;
            }
        }

        public Boolean roles_delete(String roleName)
        {
            return _MP.roles_delete(roleName);
        }

        public List<ASPASCUser> roles_get_users(String roleName)
        {
            return _MP.roles_get_users(roleName);
        }

        public List<ASPASCRole> roles_get()
        {
            return _MP.roles_get();
        }

        #endregion
    }
}
