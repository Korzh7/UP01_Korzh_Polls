using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница управления опросами: просмотр списка, добавление, редактирование и каскадное удаление
/// </summary>
public partial class PollTabPage : Page
{
    /// <summary>
    /// Конструктор страницы управления опросами
    /// </summary>
    public PollTabPage()
    {
        InitializeComponent();
        LoadData();
        this.IsVisibleChanged += Page_IsVisibleChanged;
    }

    /// <summary>
    /// Загрузка опросов из базы данных с включением связанных данных пользователя и категории
    /// </summary>
    private void LoadData()
    {
        try
        {
            using var db = new PollsDbContext();
            DataGridPoll.ItemsSource = db.Polls
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsNoTracking()
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке опросов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Автоматическое обновление таблицы опросов при возврате на страницу
    /// </summary>
    /// <param name="sender">Текущая страница</param>
    /// <param name="e">Параметры изменения свойства видимости</param>
    private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible)
        {
            LoadData();
        }
    }

    /// <summary>
    /// Переход на форму добавления нового опроса
    /// </summary>
    /// <param name="sender">Кнопка «Добавить»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddPollPage(null));
    }

    /// <summary>
    /// Каскадное удаление выбранных опросов, а также всех связанных с ними вопросов, вариантов и ответов
    /// </summary>
    /// <param name="sender">Кнопка «Удалить»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonDel_Click(object sender, RoutedEventArgs e)
    {
        var pollsForRemoving = DataGridPoll.SelectedItems.Cast<Poll>().ToList();

        if (!pollsForRemoving.Any())
        {
            MessageBox.Show("Выберите хотя бы один опрос для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {pollsForRemoving.Count} элементов? Связанные вопросы и голоса будут удалены каскадно!",
            "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new PollsDbContext();
                var ids = pollsForRemoving.Select(p => p.Id).ToList();
                var itemsToDelete = db.Polls.Where(p => ids.Contains(p.Id)).ToList();

                db.Polls.RemoveRange(itemsToDelete);
                db.SaveChanges();

                MessageBox.Show("Данные успешно удалены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Переход на форму редактирования выбранного опроса
    /// </summary>
    /// <param name="sender">Кнопка редактирования строки опроса</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Poll poll)
        {
            NavigationService?.Navigate(new AddPollPage(poll));
        }
    }
}
