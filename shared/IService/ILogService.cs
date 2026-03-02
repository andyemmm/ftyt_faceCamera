using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt.shared.IService
{
    public interface ILogService
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception exception = null);
    }
}
