using Dams.ms.auth.Reflections;

namespace Dams.ms.auth.Models
{
    public class EmailTemplatesResponse : BaseEntity
    {
        public string TemplateHtml { get; set; }
        public string ReplaceableVariables { get; set; }
    }
}
