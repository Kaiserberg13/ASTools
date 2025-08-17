using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace ASTools;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        if (config.GetSection("general") as ASTools.Srttings.General == null)
        {
            config.Sections.Add("general", new ASTools.Srttings.General
            {
                Language = CultureInfo.CurrentCulture
            });
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("general");
        }
        if (config.GetSection("themes") as ASTools.Srttings.Themes == null)
        {
            config.Sections.Add("themes", new ASTools.Srttings.Themes());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("themes");
        }
    }
}

