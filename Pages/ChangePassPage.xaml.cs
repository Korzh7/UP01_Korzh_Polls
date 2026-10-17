using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница смены пароля: проверка текущего пароля пользователя и установка нового
/// </summary>
public partial class ChangePassPage : Page
{
    /// <summary>
    /// Конструктор по умолчанию страницы смены пароля
    /// </summary>
    public ChangePassPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Конструктор страницы смены пароля с предварительно установленным логином текущего пользователя
    /// </summary>
    /// <param name="prefilledLogin">Логин активного пользователя</param>
    public ChangePassPage(string? prefilledLogin) : this()
    {
        if (!string.IsNullOrWhiteSpace(prefilledLogin))
        {
            TbLogin.Text = prefilledLogin;
            TbLogin.IsEnabled = false;
        }
    }

    /// <summary>
    /// Обработка сохранения нового пароля: проверка текущего пароля, валидация формата и запись нового хеша
    /// </summary>
    /// <param name="sender">Кнопка «Сохранить пароль»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonSavePassword_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
            string.IsNullOrEmpty(NewPasswordBox.Password) ||
            string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
            string.IsNullOrEmpty(TbLogin.Text))
        {
            MessageBox.Show("Все поля обязательны к заполнению!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new PollsDbContext();

            string hashedCurrentPass = SecurityHelper.GetHash(CurrentPasswordBox.Password);
            var user = db.UserAccounts
                .FirstOrDefault(u => u.Login == TbLogin.Text.Trim() && u.PasswordHash == hashedCurrentPass);

            if (user == null)
            {
                MessageBox.Show("Текущий пароль или логин неверный!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newPwd = NewPasswordBox.Password;
            if (newPwd.Length < 6)
            {
                MessageBox.Show("Новый пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool en = true;
            bool number = false;

            for (int i = 0; i < newPwd.Length; i++)
            {
                if (newPwd[i] >= '0' && newPwd[i] <= '9')
                {
                    number = true;
                }
                else if (!((newPwd[i] >= 'A' && newPwd[i] <= 'Z') || (newPwd[i] >= 'a' && newPwd[i] <= 'z')))
                {
                    en = false;
                }
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку для нового пароля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру в новый пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новый пароль и подтверждение не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            user.PasswordHash = SecurityHelper.GetHash(NewPasswordBox.Password);
            db.SaveChanges();

            MessageBox.Show("Пароль успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AuthPage());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при смене пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
