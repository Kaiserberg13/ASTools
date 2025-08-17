using System.Configuration;

namespace ASTools.Srttings
{
    public class Themes : ConfigurationSection
    {
        [ConfigurationProperty("currentTheme", DefaultValue = "Windows 11")]
        public string CurrentTheme
        {
            get { return (string)this["currentTheme"]; }
            set { this["currentTheme"] = value; }
        }

        [ConfigurationProperty("light", DefaultValue = false)]
        public bool Light
        {
            get { return (bool)this["light"]; }
            set { this["light"] = value; }
        }

        [ConfigurationProperty("fontSizeProcent", DefaultValue = 1f)]
        public float FontSizeProcent
        {
            get { return (float)this["fontSizeProcent"]; }
            set { this["fontSizeProcent"] = value; }
        }
    }
}
