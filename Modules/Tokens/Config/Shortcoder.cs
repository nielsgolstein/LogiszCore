using SitefinityWebApp.Logisz.Core.Configurations.Config;
using System.Configuration;
using Telerik.Sitefinity.Configuration;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class Shortcoder : LogiszModuleConfigElement
    {
        public Shortcoder(ConfigElement parent) : base(parent) { }

        [ConfigurationProperty("SeperatorOpeningTag", DefaultValue = "{{", IsRequired = true)]
        public string SeperatorOpeningTag
        {
            get { return (string)this["SeperatorOpeningTag"]; }
            set { this["SeperatorOpeningTag"] = value; }
        }

        [ConfigurationProperty("SeperatorCloseTag", DefaultValue = "}}", IsRequired = true)]
        public string SeperatorCloseTag
        {
            get { return (string)this["SeperatorCloseTag"]; }
            set { this["SeperatorCloseTag"] = value; }
        }

    }
}