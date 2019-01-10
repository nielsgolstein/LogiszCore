using SitefinityWebApp.Logisz.Core.Configurations.Config;
using System.Collections.Generic;
using System.Configuration;
using Telerik.Sitefinity.Configuration;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class HrefLangConfig : LogiszModuleConfigElement
    {
        public HrefLangConfig(ConfigElement parent) : base(parent) { }


        [ConfigurationProperty("IncludeCurrentLanguage", DefaultValue = false, IsRequired = true)]
        public bool IncludeCurrentLanguage
        {
            get { return (bool)this["IncludeCurrentLanguage"]; }
            set { this["IncludeCurrentLanguage"] = value; }
        }


        [ConfigurationProperty("ExcludedPages")]
        public ConfigElementDictionary<string, HrefLangExclusion> ExcludedPages
        {
            get
            {
                return (ConfigElementDictionary<string, HrefLangExclusion>)this["ExcludedPages"];
            }
        }

        /// <summary>
        /// Checks if a page is excluded or not
        /// </summary>
        /// <param name="ExcludedName">Name of the page which is excluded</param>
        /// <returns>True or false</returns>
        public bool PageIsExcluded(string ExcludedName)
        {
            ExcludedName = ExcludedName.Replace("/", string.Empty);

            return ExcludedPages.GetValueOrNull(ExcludedName.ToLower()) != null || ExcludedPages.GetValueOrNull(ExcludedName) != null;
        }


    }
}