using Dams.ms.auth.Models;
using Dams.ms.auth.Reflections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dams.ms.auth.Services
{
    public class AuthService
    {
        #region Private Variables
        private readonly IConfigurationRoot _objConfiguration;
        #endregion

        #region Constructor
        public AuthService(IConfigurationRoot objConfiguration)
        {
            _objConfiguration = objConfiguration;
        }
        #endregion
    }
}
