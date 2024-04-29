using System;
using System.Collections.Generic;
using System.Text;

namespace auto.ms.auth.api.Models
{
    public interface IServiceResponse
    {
        ServiceResponseStatus Status { get; set; }
        string ErrorMessage { get; set; }
    }
}
