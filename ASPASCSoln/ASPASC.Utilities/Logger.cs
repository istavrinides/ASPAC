using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace ASPASC.Utilities
{
    public static class TextLogger
    {
        public static void LogError(Exception ex, String codeLocation)
        {
            if (!Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\LOGS"))
                Directory.CreateDirectory(Path.GetDirectoryName(Application.ExecutablePath) + @"\LOGS");

            String logFile = String.Format(@"{0}\LOGS\{1}.log", Path.GetDirectoryName(Application.ExecutablePath), DateTime.Now.ToString("yyyyMMdd"));

            using (StreamWriter sw = new StreamWriter(logFile, true))
            {
                StringBuilder entry = new StringBuilder();

                entry.AppendFormat("{0}: ", DateTime.Now.ToString());
                entry.AppendFormat("[{0}] ", codeLocation);
                entry.AppendFormat("{0}", ex.Message);

                sw.WriteLine(entry);

                sw.WriteLine(ex.StackTrace);
            }
        }
    }
}
