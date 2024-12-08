using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Services
{
    public interface ILoggingService
    {
        void Debug(string message);
        void Error(string message);
        void Info(string message);
    }
}
