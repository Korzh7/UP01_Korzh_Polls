using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace UP01_Korzh_Polls;

/// <summary>
/// Главный класс приложения и точка входа в жизненный цикл WPF
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Инициализация при запуске приложения: настройка русской локализации и кодировки UTF-8
    /// </summary>
    /// <param name="e">Параметры запуска приложения</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var culture = new CultureInfo("ru-RU");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        base.OnStartup(e);
    }
}
