using System;

namespace Dams.ms.auth.Models
{
    public class EmailConfirmBody
    {
        public string Email { get; set; }
        public Guid UserGuid { get; set; }

    }
}
