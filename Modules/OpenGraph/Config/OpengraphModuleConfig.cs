using System.Configuration;
using Telerik.Sitefinity.Configuration;

namespace SitefinityWebApp.Logisz.Modules.Config
{
    public class OpengraphModuleConfig : ConfigElement
    {
        public OpengraphModuleConfig(ConfigElement parent) : base(parent) { }

        [ConfigurationProperty("Key", DefaultValue = "job", IsRequired = true, IsKey = true)]
        public string Key
        {
            get { return (string)this["Key"]; }
            set { this["Key"] = value; }
        }

        [ConfigurationProperty("ModuleType", DefaultValue = "Telerik.Sitefinity.DynamicTypes.Model.{Module}.{SingleItemName}", IsRequired = true)]
        public string ModuleType
        {
            get { return (string)this["ModuleType"]; }
            set { this["ModuleType"] = value; }
        }

        [ConfigurationProperty("TitlePropertyName", DefaultValue = "lgszTitle", IsRequired = true)]
        public string TitlePropertyName
        {
            get { return (string)this["TitlePropertyName"]; }
            set { this["TitlePropertyName"] = value; }
        }

        [ConfigurationProperty("DescriptionPropertyName", DefaultValue = "lgszDescription", IsRequired = true)]
        public string DescriptionPropertyName
        {
            get { return (string)this["DescriptionPropertyName"]; }
            set { this["DescriptionPropertyName"] = value; }
        }

        [ConfigurationProperty("ImagePropertyName", DefaultValue = "LgszImage", IsRequired = true)]
        public string ImagePropertyName
        {
            get { return (string)this["ImagePropertyName"]; }
            set { this["ImagePropertyName"] = value; }
        }

        [ConfigurationProperty("OgType", DefaultValue = "Vacature", IsRequired = true)]
        public string OgType
        {
            get { return (string)this["OgType"]; }
            set { this["OgType"] = value; }
        }
    }
}