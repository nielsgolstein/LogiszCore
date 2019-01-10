using SitefinityWebApp.Logisz.Core.Configurations.Config;
using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Web.Configuration;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class LogiszPageTitleHandlerConfig : LogiszModuleConfigElement
    {
        public LogiszPageTitleHandlerConfig(ConfigElement parent) : base(parent) { }

        [ConfigurationProperty("Format", DefaultValue = "{0} - {1}", IsRequired = true)]
        [ObjectInfo(typeof(ConfigDescriptions), Description = "Format of the title. {0} = current title, {1} = companyName")]
        public string Format
        {
            get { return (string)this["Format"]; }
            set { this["Format"] = value; }
        }
    }
}