using System;
using System.Collections.Generic;
using System.Text;

namespace auto.ms.auth.api.Models
{
    public enum ServiceResponseStatus
    {
        Unknown = 0,
        Ok = -1,
        InvalidRequest = 1,
        ZeroResults = 2,
        OverQueryLimit = 3,
        RequestDenied = 4,
        NotFound = 5,
        MaxWaypointsExceeded = 6
    }
}
