using System.Windows;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new MainMenuWindow().Show();
        }
    }
}
