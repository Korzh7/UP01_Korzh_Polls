using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class PollTabPage : Page
{
    public PollTabPage()
    {
        InitializeComponent();
        LoadData();
        this.IsVisibleChanged += Page_IsVisibleChanged;
    }

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

    // Шаг 10. Пункт 13: Автообновление
    private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible)
        {
            LoadData();
        }
    }

    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddPollPage(null));
    }

    // Шаг 10. Пункт 13: Каскадное удаление опросов
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

    private void ButtonEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Poll poll)
        {
            NavigationService?.Navigate(new AddPollPage(poll));
        }
    }
}
