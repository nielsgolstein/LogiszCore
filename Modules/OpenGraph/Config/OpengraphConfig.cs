/* Logisz Opengraph (Crab)
 * 
 * Custom logisz opengraph supports opengraph for the dynamic module builder
 * All modules are configured in Settings -> Advanced -> Opengraph.
 * 
 * VERSION 1.0.12.
 */

using SitefinityWebApp.Logisz.Core.Configurations.Config;
using System.Configuration;
using Telerik.Sitefinity.Configuration;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class OpengraphConfig : LogiszModuleConfigElement
    {
        public OpengraphConfig(ConfigElement parent) : base(parent) { }

        [ConfigurationProperty("DefaultOGTitle", DefaultValue = "", IsRequired = false)]
        public string DefaultOGTitle
        {
            get { return (string)this["DefaultOGTitle"]; }
            set { this["DefaultOGTitle"] = value; }
        }

        [ConfigurationProperty("DefaultOGDescription", DefaultValue = "", IsRequired = false)]
        public string DefaultOGDescription
        {
            get { return (string)this["DefaultOGDescription"]; }
            set { this["DefaultOGDescription"] = value; }
        }

        [ConfigurationProperty("FbAppId", DefaultValue = "", IsRequired  = true)]
        public string FbAppId
        {
            get { return (string)this["FbAppId"]; }
            set { this["FbAppId"] = value; }
        }

        [ConfigurationProperty("Modules")]
        public ConfigElementDictionary<string, OpengraphModuleConfig> Modules
        {
            get
            {
                return (ConfigElementDictionary<string, OpengraphModuleConfig>) this["Modules"];
            }
        }

    }
}