using System.Configuration;
using System.Globalization;

namespace ASTools.Srttings
{
    public class General : ConfigurationSection
    {
        [ConfigurationProperty("toolsDir", DefaultValue ="Data/Tools")]
        public string ToolsDir
        {
            get { return (string)this["toolsDir"]; }
            set { this["toolsDir"] = value; }
        }

        [ConfigurationProperty("guidesDir", DefaultValue = "Data/Guides")]
        public string ThemeDir
        {
            get { return (string)this["guidesDir"]; }
            set { this["guidesDir"] = value; }
        }

        [ConfigurationProperty("themeDir", DefaultValue = "Data/Themes")]
        public string GuidesDir
        {
            get { return (string)this["themeDir"]; }
            set { this["themeDir"] = value; }
        }

        [ConfigurationProperty("language", DefaultValue = "en-US")]
        public CultureInfo Language
        {
            get { return (CultureInfo)this["language"]; }
            set { this["language"] = value; }
        }
    }
}
