using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Главная панель администратора: переход к разделам управления пользователями, категориями, опросами и диаграммами
/// </summary>
public partial class AdminPage : Page
{
    private UserAccount? _adminUser;

    /// <summary>
    /// Конструктор по умолчанию страницы администратора
    /// </summary>
    public AdminPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Конструктор с сохранением контекста вошедшего администратора
    /// </summary>
    /// <param name="admin">Учетная запись администратора</param>
    public AdminPage(UserAccount admin) : this()
    {
        _adminUser = admin;
    }

    /// <summary>
    /// Переход к таблице управления пользователями системы
    /// </summary>
    /// <param name="sender">Кнопка «Пользователи»</param>
    /// <param name="e">Параметры события</param>
    private void BtnTab1_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new UsersTabPage());
    }

    /// <summary>
    /// Переход к таблице управления категориями опросов
    /// </summary>
    /// <param name="sender">Кнопка «Категории»</param>
    /// <param name="e">Параметры события</param>
    private void BtnTab2_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new CategoryTabPage());
    }

    /// <summary>
    /// Переход к таблице управления опросами
    /// </summary>
    /// <param name="sender">Кнопка «Опросы»</param>
    /// <param name="e">Параметры события</param>
    private void BtnTab3_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new PollTabPage());
    }

    /// <summary>
    /// Переход к разделу аналитических диаграмм и экспорта отчетов
    /// </summary>
    /// <param name="sender">Кнопка «Диаграммы»</param>
    /// <param name="e">Параметры события</param>
    private void BtnTab4_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new DiagrammPage());
    }

    /// <summary>
    /// Переход к пользовательскому режиму просмотра опросов и участников
    /// </summary>
    /// <param name="sender">Кнопка «Режим пользователя»</param>
    /// <param name="e">Параметры события</param>
    private void BtnUserView_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new UserPage(_adminUser ?? new UserAccount()));
    }
}
