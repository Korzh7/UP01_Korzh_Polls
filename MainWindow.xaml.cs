using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace UP01_Korzh_Polls;

/// <summary>
/// Главное окно системы: управление глобальной навигацией, темами оформления и системным таймером
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Конструктор главного окна: инициализация компонентов интерфейса и загрузка иконки
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            var uri = new Uri("pack://application:,,,/res/app_logo.png", UriKind.Absolute);
            Icon = System.Windows.Media.Imaging.BitmapFrame.Create(uri);
        }
        catch
        {
            // Иконка опциональна
        }
    }

    /// <summary>
    /// Обработчик загрузки окна: старт системного таймера времени и переход на страницу входа
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Параметры события загрузки</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = new TimeSpan(0, 0, 1);
        timer.IsEnabled = true;
        timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); };
        timer.Start();

        MainFrame.Navigate(new Pages.AuthPage());
    }

    /// <summary>
    /// Переход на один шаг назад по стеку навигации
    /// </summary>
    /// <param name="sender">Кнопка «Назад»</param>
    /// <param name="e">Параметры клика</param>
    private void ButtonBack_Click(object sender, RoutedEventArgs e)
    {
        if (MainFrame.CanGoBack)
        {
            MainFrame.GoBack();
        }
    }

    /// <summary>
    /// Обработка события завершения перехода между страницами: обновление заголовка и видимости кнопки «Назад»
    /// </summary>
    /// <param name="sender">Контейнер навигации MainFrame</param>
    /// <param name="e">Параметры события навигации</param>
    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        ButtonBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;

        if (MainFrame.Content is Page page)
        {
            PageTitle.Text = string.IsNullOrWhiteSpace(page.Title) ? "Платформа опросов и голосований" : page.Title;
        }
    }

    /// <summary>
    /// Перехват закрытия приложения с выводом диалога подтверждения пользователю
    /// </summary>
    /// <param name="sender">Главное окно</param>
    /// <param name="e">Параметры отмены закрытия</param>
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение выхода", 
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
        {
            e.Cancel = true;
        }
        else
        {
            e.Cancel = false;
        }
    }

    /// <summary>
    /// Переключение темы оформления приложения между светлой корпоративной и тёмной
    /// </summary>
    /// <param name="sender">Выпадающий список тем</param>
    /// <param name="e">Параметры выбора элемента</param>
    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized) return;

        string themeFile = ThemeComboBox.SelectedIndex == 0 ? "Dictionary.xaml" : "CustomTheme.xaml";
        try
        {
            var uri = new Uri(themeFile, UriKind.Relative);
            if (Application.LoadComponent(uri) is ResourceDictionary resourceDict)
            {
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при смене темы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}