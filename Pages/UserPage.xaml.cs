using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class UserPage : Page
{
    private UserAccount? _loggedInUser;

    public UserPage()
    {
        InitializeComponent();
        UpdateUsers();
    }

    public UserPage(UserAccount user) : this()
    {
        _loggedInUser = user;
    }

    // Шаг 8. Пункт 7: Очистка фильтров
    private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
    {
        fioFilterTextBox.Text = "";
        sortComboBox.SelectedIndex = 0;
        onlyAdminCheckBox.IsChecked = false;
        UpdateUsers();
    }

    // Шаг 11: Смена пароля для пользователя
    private void btnChangePassword_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ChangePassPage(_loggedInUser?.Login));
    }

    // Шаг 8. Пункт 8: Обработчики изменений параметров фильтрации
    private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateUsers();
    }

    private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateUsers();
    }

    private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        UpdateUsers();
    }

    private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        UpdateUsers();
    }

    // Шаг 8. Пункт 8: Логика фильтрации и сортировки
    private void UpdateUsers()
    {
        if (!IsInitialized) return;

        try
        {
            using var db = new PollsDbContext();
            List<UserAccount> currentUsers = db.UserAccounts.AsNoTracking().ToList();

            // Фильтрация по ФИО
            if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
            {
                currentUsers = currentUsers
                    .Where(x => x.FullName.ToLower().Contains(fioFilterTextBox.Text.Trim().ToLower()))
                    .ToList();
            }

            // Фильтрация по роли (только администраторы)
            if (onlyAdminCheckBox.IsChecked == true)
            {
                currentUsers = currentUsers
                    .Where(x => string.Equals(x.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Сортировка по возрастанию / убыванию ФИО
            if (sortComboBox.SelectedIndex == 0)
            {
                currentUsers = currentUsers.OrderBy(x => x.FullName).ToList();
            }
            else
            {
                currentUsers = currentUsers.OrderByDescending(x => x.FullName).ToList();
            }

            ListUser.ItemsSource = currentUsers;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
