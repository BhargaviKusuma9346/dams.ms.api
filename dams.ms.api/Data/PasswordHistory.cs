using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dams.ms.auth.Data
{
    [Table("PasswordHistory")]
    public partial class PasswordHistory
    {
        [Key]
        public int Id { get; set; }
        public string PasswordHash1 { get; set; }
        public string PasswordHash2 { get; set; }
        public string PasswordHash3 { get; set; }
        public string PasswordHash4 { get; set; }
        public string PasswordHash5 { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDt { get; set; }
        public Guid UserGuid { get; set; }
    }
}
