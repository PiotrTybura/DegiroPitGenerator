namespace Integrations.Degiro.Models.Configuration
{
    public class DegiroConfiguration
    {
        public Credentials Credentials { get; set; }
        public LoginConfiguration Login { get; set; }
        public RequestsConfiguration Requests { get; set; }
        public Domain Domain { get; set; }
    }
}
