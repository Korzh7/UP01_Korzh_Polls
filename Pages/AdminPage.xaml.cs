using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class AdminPage : Page
{
    private UserAccount? _adminUser;

    public AdminPage()
    {
        InitializeComponent();
    }

    public AdminPage(UserAccount admin) : this()
    {
        _adminUser = admin;
    }

    // Шаг 9. Пункт 3: Переходы на страницы по нажатию на кнопки
    private void BtnTab1_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new UsersTabPage());
    }

    private void BtnTab2_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new CategoryTabPage());
    }

    private void BtnTab3_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new PollTabPage());
    }

    private void BtnTab4_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new DiagrammPage());
    }

    private void BtnUserView_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new UserPage(_adminUser ?? new UserAccount()));
    }
}
