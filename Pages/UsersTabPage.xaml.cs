using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class UsersTabPage : Page
{
    public UsersTabPage()
    {
        InitializeComponent();
        LoadData();
        this.IsVisibleChanged += Page_IsVisibleChanged;
    }

    private void LoadData()
    {
        try
        {
            using var db = new PollsDbContext();
            DataGridUser.ItemsSource = db.UserAccounts.AsNoTracking().ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 10. Пункт 4: Автообновление списка при возврате на страницу
    private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible)
        {
            LoadData();
        }
    }

    // Шаг 10. Пункт 5: Кнопка перехода к добавлению пользователя
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddUserPage(null));
    }

    // Шаг 10. Пункт 5: Каскадное удаление пользователей с подтверждением
    private void ButtonDel_Click(object sender, RoutedEventArgs e)
    {
        var usersForRemoving = DataGridUser.SelectedItems.Cast<UserAccount>().ToList();

        if (!usersForRemoving.Any())
        {
            MessageBox.Show("Выберите хотя бы одного пользователя для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {usersForRemoving.Count} элементов?",
            "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new PollsDbContext();
                var ids = usersForRemoving.Select(u => u.Id).ToList();
                var itemsToDelete = db.UserAccounts.Where(u => ids.Contains(u.Id)).ToList();

                db.UserAccounts.RemoveRange(itemsToDelete);
                db.SaveChanges();

                MessageBox.Show("Данные успешно удалены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Шаг 10. Пункт 5: Кнопка перехода к редактированию
    private void ButtonEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is UserAccount user)
        {
            NavigationService?.Navigate(new AddUserPage(user));
        }
    }
}
