using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница регистрации новых пользователей системы с валидацией сложности паролей
/// </summary>
public partial class RegPage : Page
{
    /// <summary>
    /// Конструктор страницы регистрации и установка роли по умолчанию
    /// </summary>
    public RegPage()
    {
        InitializeComponent();
        comboBxRole.SelectedIndex = 0;
    }

    /// <summary>
    /// Управление видимостью подсказки для поля логина
    /// </summary>
    /// <param name="sender">Поле логина</param>
    /// <param name="e">Параметры изменения текста</param>
    private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
    {
        lblLogHitn.Visibility = string.IsNullOrEmpty(txtbxLog.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Управление видимостью подсказки для поля ФИО
    /// </summary>
    /// <param name="sender">Поле ФИО</param>
    /// <param name="e">Параметры изменения текста</param>
    private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
    {
        lblFioHitn.Visibility = string.IsNullOrEmpty(txtbxFIO.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Управление видимостью подсказки для поля первого пароля
    /// </summary>
    /// <param name="sender">Поле ввода пароля</param>
    /// <param name="e">Параметры события</param>
    private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
    {
        lblPassHitn.Visibility = string.IsNullOrEmpty(passBxFrst.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Управление видимостью подсказки для поля подтверждения пароля
    /// </summary>
    /// <param name="sender">Поле повторного ввода пароля</param>
    /// <param name="e">Параметры события</param>
    private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
    {
        lblPassSecHitn.Visibility = string.IsNullOrEmpty(passBxScnd.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Обработка регистрации: проверка уникальности логина, валидация сложности пароля и сохранение в базу
    /// </summary>
    /// <param name="sender">Кнопка «Зарегистрироваться»</param>
    /// <param name="e">Параметры события клика</param>
    private void regButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtbxLog.Text) || 
            string.IsNullOrWhiteSpace(txtbxFIO.Text) || 
            string.IsNullOrEmpty(passBxFrst.Password) || 
            string.IsNullOrEmpty(passBxScnd.Password))
        {
            MessageBox.Show("Заполните все обязательные поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new PollsDbContext();

            var existingUser = db.UserAccounts
                .AsNoTracking()
                .FirstOrDefault(u => u.Login.ToLower() == txtbxLog.Text.Trim().ToLower());

            if (existingUser != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string pwd = passBxFrst.Password;
            if (pwd.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool en = true;
            bool number = false;

            for (int i = 0; i < pwd.Length; i++)
            {
                if (pwd[i] >= '0' && pwd[i] <= '9')
                {
                    number = true;
                }
                else if (!((pwd[i] >= 'A' && pwd[i] <= 'Z') || (pwd[i] >= 'a' && pwd[i] <= 'z')))
                {
                    en = false;
                }
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string trimmedLogin = txtbxLog.Text.Trim();
            string trimmedFio = txtbxFIO.Text.Trim();
            string trimmedPassword = passBxFrst.Password.Trim();

            string selectedRole = ((ComboBoxItem)comboBxRole.SelectedItem)?.Content?.ToString() ?? "User";
            string hashedPassword = SecurityHelper.GetHash(trimmedPassword);

            var newUser = new UserAccount
            {
                Login = trimmedLogin,
                FullName = trimmedFio,
                Email = string.IsNullOrWhiteSpace(txtbxEmail.Text) ? null : txtbxEmail.Text.Trim(),
                Phone = string.IsNullOrWhiteSpace(txtbxPhone.Text) ? null : txtbxPhone.Text.Trim(),
                Role = selectedRole,
                PasswordHash = hashedPassword,
                Photo = "/res/app_logo.png"
            };

            db.UserAccounts.Add(newUser);
            db.SaveChanges();

            MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            txtbxLog.Clear();
            txtbxFIO.Clear();
            txtbxEmail.Clear();
            txtbxPhone.Clear();
            passBxFrst.Clear();
            passBxScnd.Clear();
            comboBxRole.SelectedIndex = 0;

            NavigationService?.Navigate(new AuthPage());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
