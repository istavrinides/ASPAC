using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPASC.Model
{
    public class ASPASCUser
    {        
        public String UserName { get; set; }
        public String Email { get; set; }
        public String PasswordQuestion { get; set; }
        public String Comment { get; set; }
        
        public Boolean Approved { get; set; }
        public Boolean LockedOut { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public DateTime LastLockoutDate { get; set; }        
    }
}
