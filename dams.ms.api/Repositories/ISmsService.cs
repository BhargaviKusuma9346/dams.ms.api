using Dams.ms.auth.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dams.ms.auth.Repositories
{
    public interface ISmsService
    {
        Task<string> SendTransactionalSMSToPhone(SmsBody body);
    }
}
