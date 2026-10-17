using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница пользователя: каталог участников, участие в доступных опросах и фильтрация
/// </summary>
public partial class UserPage : Page
{
    private UserAccount? _loggedInUser;

    /// <summary>
    /// Конструктор по умолчанию страницы пользователя: загрузка каталога пользователей и опросов
    /// </summary>
    public UserPage()
    {
        InitializeComponent();
        UpdateUsers();
        LoadPolls();
    }

    /// <summary>
    /// Конструктор с передачей данных текущего авторизованного пользователя
    /// </summary>
    /// <param name="user">Текущий пользователь системы</param>
    public UserPage(UserAccount user) : this()
    {
        _loggedInUser = user;
    }

    /// <summary>
    /// Загрузка перечня активных опросов из базы данных для голосования
    /// </summary>
    private void LoadPolls()
    {
        try
        {
            using var db = new PollsDbContext();
            DataGridPollsForUser.ItemsSource = db.Polls
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.Status == "Активен")
                .AsNoTracking()
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке опросов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Голосование в выбранном опросе: проверка уникальности ответа и фиксация в таблице responses
    /// </summary>
    /// <param name="sender">Кнопка «Проголосовать»</param>
    /// <param name="e">Параметры события клика</param>
    private void btnVotePoll_Click(object sender, RoutedEventArgs e)
    {
        if (DataGridPollsForUser.SelectedItem is not Poll selectedPoll)
        {
            MessageBox.Show("Пожалуйста, выберите опрос из списка, чтобы проголосовать!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new PollsDbContext();

            int userId = _loggedInUser?.Id ?? 2;
            bool alreadyVoted = db.Responses.Any(r => r.PollId == selectedPoll.Id && r.UserId == userId);

            if (alreadyVoted)
            {
                MessageBox.Show($"Вы уже принимали участие в опросе «{selectedPoll.Title}». Повторное голосование невозможно.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var response = new Response
            {
                PollId = selectedPoll.Id,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            db.Responses.Add(response);
            db.SaveChanges();

            MessageBox.Show($"Спасибо! Ваш голос в опросе «{selectedPoll.Title}» успешно учтён и зафиксирован в системе.", "Голос принят", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка голосования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Сброс всех примененных фильтров пользователей к начальным значениям
    /// </summary>
    /// <param name="sender">Кнопка «Очистить фильтры»</param>
    /// <param name="e">Параметры события клика</param>
    private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
    {
        fioFilterTextBox.Text = "";
        sortComboBox.SelectedIndex = 0;
        onlyAdminCheckBox.IsChecked = false;
        UpdateUsers();
    }

    /// <summary>
    /// Переход на экран смены пароля текущего пользователя
    /// </summary>
    /// <param name="sender">Кнопка «Сменить пароль»</param>
    /// <param name="e">Параметры события клика</param>
    private void btnChangePassword_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ChangePassPage(_loggedInUser?.Login));
    }

    /// <summary>
    /// Обработка изменения текста в поле фильтрации по ФИО
    /// </summary>
    /// <param name="sender">Текстовое поле ввода ФИО</param>
    /// <param name="e">Параметры события изменения текста</param>
    private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateUsers();
    }

    /// <summary>
    /// Обработка смены направления сортировки по ФИО
    /// </summary>
    /// <param name="sender">Выпадающий список сортировки</param>
    /// <param name="e">Параметры события изменения выбора</param>
    private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateUsers();
    }

    /// <summary>
    /// Обработка активации фильтрации «Только администраторы»
    /// </summary>
    /// <param name="sender">Флажок «Только администраторы»</param>
    /// <param name="e">Параметры события</param>
    private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        UpdateUsers();
    }

    /// <summary>
    /// Обработка деактивации фильтрации «Только администраторы»
    /// </summary>
    /// <param name="sender">Флажок «Только администраторы»</param>
    /// <param name="e">Параметры события</param>
    private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        UpdateUsers();
    }

    /// <summary>
    /// Применение всех фильтров (по ФИО, роли) и сортировки к списку пользователей
    /// </summary>
    private void UpdateUsers()
    {
        if (!IsInitialized) return;

        try
        {
            using var db = new PollsDbContext();
            List<UserAccount> currentUsers = db.UserAccounts.AsNoTracking().ToList();

            if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
            {
                currentUsers = currentUsers
                    .Where(x => x.FullName.ToLower().Contains(fioFilterTextBox.Text.Trim().ToLower()))
                    .ToList();
            }

            if (onlyAdminCheckBox.IsChecked == true)
            {
                currentUsers = currentUsers
                    .Where(x => string.Equals(x.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

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
