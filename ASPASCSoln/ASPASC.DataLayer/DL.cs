using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

using ASPASC.Model;
using ASPASC.Utilities;

namespace ASPASC.DataLayer
{
    public class DL
    {
        #region "Variables"
        
        private String _dbServer;
        private String _dbDatabase;
        private String _dbUserName;
        private String _dbUserPassword;

        private Boolean _userWinAuth = false;
        
        #endregion

        #region "Properties"

        private String SQLConnectionString
        {
            get
            {
                if (_userWinAuth)
                    return String.Format("Server={0};Database={1};Trusted_Connection=true", _dbServer, _dbDatabase);
                else
                    return String.Format("Server={0};Database={1};User Id={2};Password={3}", _dbServer, _dbDatabase, _dbUserName, _dbUserPassword);
            }
        }

        #endregion

        #region "Constructors"
        
        public DL(String databaseServerName, String databaseName)
        {
            _dbServer = databaseServerName;
            _dbDatabase = databaseName;
            _userWinAuth = true;
        }

        public DL(String databaseServerName, String databaseName, String databaseUserName, String databaseUserPassword)
        {
            _dbServer = databaseServerName;
            _dbDatabase = databaseName;
            _dbUserName = databaseUserName;
            _dbUserPassword = databaseUserPassword;
        }

        #endregion

        #region "Check connectivity"

        public Boolean checkConnection()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    return _dc.DatabaseExists();
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.checkConnection");
                    return false;
                }
            }
        }

        #endregion

        #region "Check for features"

        public Boolean rolesEnabled()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    var result = _dc.ExecuteQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'aspnet_Roles'").FirstOrDefault();

                    return result == 1;
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.rolesEnabled");
                    return false;
                }
            }
        }

        public Boolean membershipEnabled()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    var result = _dc.ExecuteQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'aspnet_Membership'").FirstOrDefault();

                    return result == 1;
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.membershipEnabled");
                    return false;
                }
            }
        }

        public Boolean personalizationEnabled()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    var result = _dc.ExecuteQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'aspnet_PersonalizationPerUser'").FirstOrDefault();

                    return result == 1;
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.personalizationEnabled");
                    return false;
                }
            }
        }

        public Boolean webEventsEnabled()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    var result = _dc.ExecuteQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'aspnet_WebEvent_Events'").FirstOrDefault();

                    return result == 1;
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.webEventsEnabled");
                    return false;
                }
            }
        }

        public Boolean profileEnabled()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    var result = _dc.ExecuteQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE name = 'aspnet_Profile'").FirstOrDefault();

                    return result == 1;
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.profileEnabled");
                    return false;
                }
            }
        }

        #endregion

        #region "Applications"

        public List<ASPASCApplication> getApplications()
        {
            using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
            {
                try
                {
                    var retVal = from p in _dc.vw_aspnet_Applications
                                 select new ASPASCApplication()
                                 {
                                     Description = p.Description,
                                     Id = p.ApplicationId,
                                     Name = p.ApplicationName
                                 };

                    return retVal.ToList();
                }
                catch (Exception ex)
                {
                    TextLogger.LogError(ex, "DL.getApplications");
                    return new List<ASPASCApplication>();
                }
            }
        }

        public Boolean application_add(String applicationName)
        {
            try
            {
                using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(SQLConnectionString))
                {
                    Guid? newAppGuid = null;

                    _dc.aspnet_Applications_CreateApplication(applicationName, ref newAppGuid);

                    return true;
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "DL.application_add(1)");
                return false;
            }
        }

        public Boolean application_add(String applicationName, String connectionString)
        {
            try
            {
                using (ASPASCDCDataContext _dc = new ASPASCDCDataContext(connectionString))
                {
                    Guid? newAppGuid = null;

                    _dc.aspnet_Applications_CreateApplication(applicationName, ref newAppGuid);

                    return true;
                }
            }
            catch (Exception ex)
            {
                TextLogger.LogError(ex, "DL.application_add(2)");
                return false;
            }
        }

        #endregion
    }
}
