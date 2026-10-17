using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница авторизации пользователей: ввод учетных данных, валидация капчи и распределение по ролям
/// </summary>
public partial class AuthPage : Page
{
    private int failedAttempts = 0;
    private UserAccount? currentUser;

    /// <summary>
    /// Конструктор страницы авторизации
    /// </summary>
    public AuthPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Обработчик изменения текста логина для динамического отображения подсказки-плейсхолдера
    /// </summary>
    /// <param name="sender">Поле ввода логина</param>
    /// <param name="e">Параметры изменения текста</param>
    private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
    {
        txtHintLogin.Visibility = string.IsNullOrEmpty(TextBoxLogin.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Обработчик изменения пароля для динамического отображения подсказки-плейсхолдера
    /// </summary>
    /// <param name="sender">Поле ввода пароля</param>
    /// <param name="e">Параметры изменения пароля</param>
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        txtHintPass.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Авторизация пользователя: сверка хеша пароля в базе данных и переход в зависимости от роли (Admin/User)
    /// </summary>
    /// <param name="sender">Кнопка «Войти»</param>
    /// <param name="e">Параметры клика</param>
    private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
        {
            MessageBox.Show("Введите логин или пароль!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string inputLogin = TextBoxLogin.Text.Trim();
        string inputPassword = PasswordBox.Password.Trim();

        string hashedPassword = SecurityHelper.GetHash(inputPassword);

        try
        {
            using var db = new PollsDbContext();
            var user = db.UserAccounts
                .AsNoTracking()
                .FirstOrDefault(u => (u.Login.ToLower() == inputLogin.ToLower() || (u.Email != null && u.Email.ToLower() == inputLogin.ToLower())) && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                failedAttempts++;

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

    /// <summary>
    /// Переключение видимости панели капчи и доступности кнопки входа
    /// </summary>
    /// <param name="showCaptcha">Флаг отображения капчи</param>
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

    /// <summary>
    /// Генерация случайной строки символов капчи из 6 элементов
    /// </summary>
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

    /// <summary>
    /// Обновление защитного кода капчи по кнопке
    /// </summary>
    /// <param name="sender">Кнопка «Обновить»</param>
    /// <param name="e">Параметры события</param>
    private void ButtonRefreshCaptcha_Click(object sender, RoutedEventArgs e)
    {
        CaptchaChange();
    }

    /// <summary>
    /// Проверка совпадения введенного защитного кода с сгенерированной капчей
    /// </summary>
    /// <param name="sender">Кнопка подтверждения капчи</param>
    /// <param name="e">Параметры события</param>
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

    /// <summary>
    /// Блокировка системных операций буфера обмена (Copy, Cut, Paste) в поле капчи
    /// </summary>
    /// <param name="sender">Текстовое поле капчи</param>
    /// <param name="e">Параметры перехваченной команды</param>
    private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Переход на экран регистрации новой учетной записи
    /// </summary>
    /// <param name="sender">Кнопка «Регистрация»</param>
    /// <param name="e">Параметры события</param>
    private void ButtonReg_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new RegPage());
    }

    /// <summary>
    /// Переход на экран смены пароля пользователя
    /// </summary>
    /// <param name="sender">Кнопка смены пароля</param>
    /// <param name="e">Параметры события</param>
    private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ChangePassPage());
    }
}
