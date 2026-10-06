using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace UP01_Korzh_Polls;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "app_logo.png");
            if (System.IO.File.Exists(iconPath))
            {
                Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }
        }
        catch
        {
            // Иконка опциональна
        }
    }

    // Шаг 2. Пункт 5: Инициализация таймера даты и времени
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = new TimeSpan(0, 0, 1);
        timer.IsEnabled = true;
        timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); };
        timer.Start();

        // Загрузка страницы авторизации (Шаг 4) при старте
        MainFrame.Navigate(new Pages.AuthPage());
    }

    // Шаг 2. Пункт 6: Кнопка «Назад»
    private void ButtonBack_Click(object sender, RoutedEventArgs e)
    {
        if (MainFrame.CanGoBack)
        {
            MainFrame.GoBack();
        }
    }

    // Динамический заголовок страницы и управление видимостью кнопки «Назад»
    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        ButtonBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;

        if (MainFrame.Content is Page page)
        {
            PageTitle.Text = string.IsNullOrWhiteSpace(page.Title) ? "Платформа опросов и голосований" : page.Title;
        }
    }

    // Шаг 2. Пункт 8: Выход из приложения с подтверждением
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

    // Шаг 3. Пункт 5: Переключение двух словарей стилей (Dictionary.xaml и CustomTheme.xaml)
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