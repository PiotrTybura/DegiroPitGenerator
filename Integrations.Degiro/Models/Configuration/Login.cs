using System.Collections.Generic;

namespace Integrations.Degiro.Models.Configuration
{
    public class LoginConfiguration
    {
        public string LoginUrl { get; set; }
        public LoginXPaths XPaths { get; set; }
        public string SessionCookieName { get; set; }
    }
}