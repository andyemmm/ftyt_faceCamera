using ftyt.shared.IService;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt.shared.Service
{
    public class LogService : ILogService
    {
        readonly log4net.ILog _log;

        public LogService(string cfg = "log4net.cfg.xml")
        {

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            XmlConfigurator.Configure(new FileInfo(@"Config\" + cfg));
            _log = log4net.LogManager.GetLogger("demo");
            _log.Info("application startup");

        }

        public void Error(string message, Exception exception = null)
        {
            if (_log.IsErrorEnabled) _log.Error(message, exception);
        }

        public void Info(string message)
        {
            if (_log.IsInfoEnabled) _log.Info(message);
        }

        public void Warn(string message)
        {
            if (_log.IsWarnEnabled) _log.Warn(message);
        }
    }
}
