using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class RegPage : Page
{
    public RegPage()
    {
        InitializeComponent();
        // По умолчанию выбрана роль "User" (Шаг 6, Пункт 3)
        comboBxRole.SelectedIndex = 0;
    }

    // Обработка плейсхолдеров
    private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
    {
        lblLogHitn.Visibility = string.IsNullOrEmpty(txtbxLog.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
    {
        lblFioHitn.Visibility = string.IsNullOrEmpty(txtbxFIO.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
    {
        lblPassHitn.Visibility = string.IsNullOrEmpty(passBxFrst.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
    {
        lblPassSecHitn.Visibility = string.IsNullOrEmpty(passBxScnd.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    // Шаг 6. Пункт 6: Обработка нажатия на кнопку «Зарегистрироваться»
    private void regButton_Click(object sender, RoutedEventArgs e)
    {
        // 6.1. Проверка заполнения полей
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

            // 6.2. Проверка наличия в базе данных логина пользователя
            var existingUser = db.UserAccounts
                .AsNoTracking()
                .FirstOrDefault(u => u.Login.ToLower() == txtbxLog.Text.Trim().ToLower());

            if (existingUser != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 6.3. Проверка формата пароля согласно методичке:
            // 1) 6 или более символов;
            // 2) только английская раскладка;
            // 3) хотя бы одна цифра.
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

            // 6.4 (7.4 в нумерации методички): Проверка на совпадение двух паролей
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string trimmedLogin = txtbxLog.Text.Trim();
            string trimmedFio = txtbxFIO.Text.Trim();
            string trimmedPassword = passBxFrst.Password.Trim();

            // 6.5 (7.5 в нумерации методички): Создание и сохранение пользователя в БД
            string selectedRole = ((ComboBoxItem)comboBxRole.SelectedItem)?.Content?.ToString() ?? "User";
            
            // Хеширование пароля (Шаг 7)
            string hashedPassword = SecurityHelper.GetHash(trimmedPassword);

            var newUser = new UserAccount
            {
                Login = trimmedLogin,
                FullName = trimmedFio,
                Email = string.IsNullOrWhiteSpace(txtbxEmail.Text) ? null : txtbxEmail.Text.Trim(),
                Phone = string.IsNullOrWhiteSpace(txtbxPhone.Text) ? null : txtbxPhone.Text.Trim(),
                Role = selectedRole,
                PasswordHash = hashedPassword,
                Photo = "/res/user1.png"
            };

            db.UserAccounts.Add(newUser);
            db.SaveChanges();

            MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Очистка полей и переход к авторизации
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
