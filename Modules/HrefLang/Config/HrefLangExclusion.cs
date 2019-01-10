using System.Configuration;
using Telerik.Sitefinity.Configuration;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class HrefLangExclusion : ConfigElement
    {
        public HrefLangExclusion(ConfigElement parent) : base(parent) { }


        [ConfigurationProperty("Key", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["Key"]; }
            set { this["Key"] = value; }
        }


    }
}