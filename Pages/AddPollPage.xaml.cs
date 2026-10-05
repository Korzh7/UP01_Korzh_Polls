using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class AddPollPage : Page
{
    private Poll _currentPoll = new Poll();

    // Шаг 10. Пункт 15: Конструктор для добавления или редактирования опроса
    public AddPollPage(Poll? selectedPoll)
    {
        InitializeComponent();

        try
        {
            using var db = new PollsDbContext();
            CBUser.ItemsSource = db.UserAccounts.AsNoTracking().ToList();
            CBCategory.ItemsSource = db.Categories.AsNoTracking().ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки списков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        if (selectedPoll != null)
        {
            _currentPoll = selectedPoll;
            HeaderTextBlock.Text = "РЕДАКТИРОВАНИЕ ОПРОСА";
            CBStatus.Text = _currentPoll.Status;
        }
        else
        {
            _currentPoll.CreatedAt = DateTime.Now;
            _currentPoll.Status = "Активен";
        }

        DataContext = _currentPoll;
    }

    // Шаг 10. Пункт 15: Сохранение опроса с валидацией
    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        var errors = new StringBuilder();

        if (string.IsNullOrWhiteSpace(_currentPoll.Title))
            errors.AppendLine("Укажите название опроса!");

        if (CBUser.SelectedValue == null)
            errors.AppendLine("Укажите автора опроса!");

        if (CBCategory.SelectedValue == null)
            errors.AppendLine("Укажите категорию опроса!");

        if (errors.Length > 0)
        {
            MessageBox.Show(errors.ToString(), "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new PollsDbContext();

            _currentPoll.UserId = (int)CBUser.SelectedValue;
            _currentPoll.CategoryId = (int)CBCategory.SelectedValue;
            _currentPoll.Status = ((ComboBoxItem)CBStatus.SelectedItem)?.Content?.ToString() ?? "Активен";
            _currentPoll.IsAnonymous = ChbAnonymous.IsChecked == true;

            if (_currentPoll.Id == 0)
            {
                _currentPoll.CreatedAt = DateTime.Now;
                db.Polls.Add(_currentPoll);
            }
            else
            {
                var existing = db.Polls.FirstOrDefault(p => p.Id == _currentPoll.Id);
                if (existing != null)
                {
                    existing.Title = _currentPoll.Title;
                    existing.Description = _currentPoll.Description;
                    existing.UserId = _currentPoll.UserId;
                    existing.CategoryId = _currentPoll.CategoryId;
                    existing.Status = _currentPoll.Status;
                    existing.IsAnonymous = _currentPoll.IsAnonymous;
                }
            }

            db.SaveChanges();
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.GoBack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ButtonClean_Click(object sender, RoutedEventArgs e)
    {
        TBPollTitle.Text = "";
        TBPollDesc.Text = "";
        if (CBUser.Items.Count > 0) CBUser.SelectedIndex = -1;
        if (CBCategory.Items.Count > 0) CBCategory.SelectedIndex = -1;
        CBStatus.SelectedIndex = 0;
        ChbAnonymous.IsChecked = false;
    }
}
