using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Форма добавления и редактирования данных учетной записи пользователя администратором
/// </summary>
public partial class AddUserPage : Page
{
    private UserAccount _currentUser = new UserAccount();
    private bool _isEditMode = false;

    /// <summary>
    /// Конструктор формы: принимает объект пользователя для редактирования либо null для добавления нового
    /// </summary>
    /// <param name="selectedUser">Редактируемый пользователь или null</param>
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
            _currentUser.Photo = "/res/app_logo.png";
        }

        DataContext = _currentUser;
    }

    /// <summary>
    /// Валидация введенных полей, хеширование пароля и сохранение пользователя в базе данных
    /// </summary>
    /// <param name="sender">Кнопка «Сохранить»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        _currentUser.Login = TBLogin.Text?.Trim() ?? string.Empty;
        _currentUser.FullName = TBFio.Text?.Trim() ?? string.Empty;
        _currentUser.Email = string.IsNullOrWhiteSpace(TBEmail.Text) ? null : TBEmail.Text.Trim();
        _currentUser.Phone = string.IsNullOrWhiteSpace(TBPhone.Text) ? null : TBPhone.Text.Trim();
        _currentUser.Photo = string.IsNullOrWhiteSpace(TBPhoto.Text) ? "/res/app_logo.png" : TBPhoto.Text.Trim();

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
                _currentUser.PasswordHash = SecurityHelper.GetHash(TBPass.Text.Trim());
                db.UserAccounts.Add(_currentUser);
            }
            else
            {
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

    /// <summary>
    /// Очистка всех полей формы создания/редактирования пользователя
    /// </summary>
    /// <param name="sender">Кнопка «Очистить»</param>
    /// <param name="e">Параметры события клика</param>
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
        _currentUser.Photo = "/res/app_logo.png";
    }
}
