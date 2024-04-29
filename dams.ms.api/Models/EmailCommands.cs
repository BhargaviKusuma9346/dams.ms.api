namespace Dams.ms.auth.Models
{
    public class EmailCommands
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
    }
}
