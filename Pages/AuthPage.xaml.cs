using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class AuthPage : Page
{
    // Шаг 4. Пункт 5 & Шаг 5. Пункт 1: Переменные попыток и пользователя
    private int failedAttempts = 0;
    private UserAccount? currentUser;

    public AuthPage()
    {
        InitializeComponent();
    }

    // Подсказки-плейсхолдеры
    private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
    {
        txtHintLogin.Visibility = string.IsNullOrEmpty(TextBoxLogin.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        txtHintPass.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    // Шаг 4. Пункт 6: Авторизация пользователя
    private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
        {
            MessageBox.Show("Введите логин или пароль!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string hashedPassword = SecurityHelper.GetHash(PasswordBox.Password);

        try
        {
            using var db = new PollsDbContext();
            var user = db.UserAccounts
                .AsNoTracking()
                .FirstOrDefault(u => (u.Login == TextBoxLogin.Text || u.Email == TextBoxLogin.Text) && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                failedAttempts++;

                // Шаг 5: 3 неверных ввода — вывод капчи
                if (failedAttempts >= 3)
                {
                    CaptchaSwitch(true);
                    CaptchaChange();
                }
                return;
            }
            else
            {
                currentUser = user;
                MessageBox.Show($"Добро пожаловать, {user.FullName}!", "Успешная авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                failedAttempts = 0;

                // Переход в зависимости от роли (Пользователь / Администратор)
                switch (user.Role)
                {
                    case "Admin":
                        NavigationService?.Navigate(new AdminPage(user));
                        break;
                    case "User":
                    default:
                        NavigationService?.Navigate(new UserPage(user));
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 5. Метод включения / отключения капчи
    public void CaptchaSwitch(bool showCaptcha)
    {
        if (showCaptcha)
        {
            captchaPanel.Visibility = Visibility.Visible;
            ButtonEnter.IsEnabled = false;
        }
        else
        {
            captchaPanel.Visibility = Visibility.Collapsed;
            ButtonEnter.IsEnabled = true;
            captchaInput.Clear();
        }
    }

    // Шаг 5. Код генерации капчи
    public void CaptchaChange()
    {
        string allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z,1,2,3,4,5,6,7,8,9,0";
        string[] ar = allowchar.Split(',');
        string pwd = "";
        var r = new Random();

        for (int i = 0; i < 6; i++)
        {
            pwd += ar[r.Next(0, ar.Length)];
        }

        captcha.Text = pwd;
    }

    private void ButtonRefreshCaptcha_Click(object sender, RoutedEventArgs e)
    {
        CaptchaChange();
    }

    // Шаг 5. Подтверждение капчи
    private void submitCaptcha_Click(object sender, RoutedEventArgs e)
    {
        if (string.Equals(captchaInput.Text.Trim(), captcha.Text.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Капча введена успешно, можете продолжить авторизацию!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            CaptchaSwitch(false);
            failedAttempts = 0;
        }
        else
        {
            MessageBox.Show("Неверно введена капча!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            CaptchaChange();
            captchaInput.Clear();
        }
    }

    // Шаг 5. Запрет копирования / вырезания / вставки капчи
    private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
        {
            e.Handled = true;
        }
    }

    // Шаг 4. Пункт 7: Переходы на страницы регистрации и смены пароля
    private void ButtonReg_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new RegPage());
    }

    private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ChangePassPage());
    }
}
