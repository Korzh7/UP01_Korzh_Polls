using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница управления учетными записями пользователей: табличный просмотр, добавление, редактирование и удаление
/// </summary>
public partial class UsersTabPage : Page
{
    /// <summary>
    /// Конструктор страницы пользователей: загрузка данных и подписка на событие отображения
    /// </summary>
    public UsersTabPage()
    {
        InitializeComponent();
        LoadData();
        this.IsVisibleChanged += Page_IsVisibleChanged;
    }

    /// <summary>
    /// Загрузка полного списка пользователей из базы данных в таблицу
    /// </summary>
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

    /// <summary>
    /// Автоматическое обновление данных таблицы при возврате пользователя на страницу
    /// </summary>
    /// <param name="sender">Текущая страница</param>
    /// <param name="e">Параметры изменения свойства видимости</param>
    private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible)
        {
            LoadData();
        }
    }

    /// <summary>
    /// Переход на форму добавления новой учетной записи пользователя
    /// </summary>
    /// <param name="sender">Кнопка «Добавить»</param>
    /// <param name="e">Параметры клика</param>
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddUserPage(null));
    }

    /// <summary>
    /// Каскадное удаление выделенных пользователей из базы данных с подтверждением действия
    /// </summary>
    /// <param name="sender">Кнопка «Удалить»</param>
    /// <param name="e">Параметры клика</param>
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

    /// <summary>
    /// Переход на форму редактирования выбранного в строке таблицы пользователя
    /// </summary>
    /// <param name="sender">Кнопка редактирования строки</param>
    /// <param name="e">Параметры клика</param>
    private void ButtonEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is UserAccount user)
        {
            NavigationService?.Navigate(new AddUserPage(user));
        }
    }
}
