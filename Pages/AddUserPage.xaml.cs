using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class AddUserPage : Page
{
    private UserAccount _currentUser = new UserAccount();
    private bool _isEditMode = false;

    // Шаг 10. Пункт 7: Конструктор для добавления или редактирования
    public AddUserPage(UserAccount? selectedUser)
    {
        InitializeComponent();

        if (selectedUser != null)
        {
            _currentUser = selectedUser;
            _isEditMode = true;
            HeaderTextBlock.Text = "РЕДАКТИРОВАНИЕ ПОЛЬЗОВАТЕЛЯ";
            TBPass.ToolTip = "Оставьте пустым, если не хотите менять пароль";
            cmbRole.Text = _currentUser.Role;
        }
        else
        {
            cmbRole.SelectedIndex = 0;
            _currentUser.Photo = "/res/user1.png";
        }

        DataContext = _currentUser;
    }

    // Шаг 10. Пункт 7: Логика кнопки Сохранить
    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        // Синхронизация данных с полями ввода
        _currentUser.Login = TBLogin.Text?.Trim() ?? string.Empty;
        _currentUser.FullName = TBFio.Text?.Trim() ?? string.Empty;
        _currentUser.Email = string.IsNullOrWhiteSpace(TBEmail.Text) ? null : TBEmail.Text.Trim();
        _currentUser.Phone = string.IsNullOrWhiteSpace(TBPhone.Text) ? null : TBPhone.Text.Trim();
        _currentUser.Photo = string.IsNullOrWhiteSpace(TBPhoto.Text) ? null : TBPhoto.Text.Trim();

        var errors = new StringBuilder();

        if (string.IsNullOrWhiteSpace(_currentUser.Login))
            errors.AppendLine("Укажите логин!");

        if (!_isEditMode && string.IsNullOrWhiteSpace(TBPass.Text))
            errors.AppendLine("Укажите пароль!");

        string role = cmbRole.Text;
        if (string.IsNullOrWhiteSpace(role))
            errors.AppendLine("Выберите роль!");
        else
            _currentUser.Role = role;

        if (string.IsNullOrWhiteSpace(_currentUser.FullName))
            errors.AppendLine("Укажите ФИО!");

        if (errors.Length > 0)
        {
            MessageBox.Show(errors.ToString(), "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new PollsDbContext();

            if (_currentUser.Id == 0)
            {
                // Новый пользователь
                _currentUser.PasswordHash = SecurityHelper.GetHash(TBPass.Text.Trim());
                db.UserAccounts.Add(_currentUser);
            }
            else
            {
                // Редактирование
                var existing = db.UserAccounts.FirstOrDefault(u => u.Id == _currentUser.Id);
                if (existing != null)
                {
                    existing.Login = _currentUser.Login;
                    existing.FullName = _currentUser.FullName;
                    existing.Role = _currentUser.Role;
                    existing.Email = _currentUser.Email;
                    existing.Phone = _currentUser.Phone;
                    existing.Photo = _currentUser.Photo;

                    if (!string.IsNullOrWhiteSpace(TBPass.Text))
                    {
                        existing.PasswordHash = SecurityHelper.GetHash(TBPass.Text.Trim());
                    }
                }
            }

            db.SaveChanges();
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.GoBack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 10. Пункт 7: Логика кнопки Очистить
    private void ButtonClean_Click(object sender, RoutedEventArgs e)
    {
        TBLogin.Text = string.Empty;
        TBPass.Text = string.Empty;
        TBFio.Text = string.Empty;
        TBEmail.Text = string.Empty;
        TBPhone.Text = string.Empty;
        TBPhoto.Text = string.Empty;
        cmbRole.SelectedIndex = 0;

        _currentUser.Login = string.Empty;
        _currentUser.FullName = string.Empty;
        _currentUser.Email = null;
        _currentUser.Phone = null;
        _currentUser.Photo = null;
    }
}
