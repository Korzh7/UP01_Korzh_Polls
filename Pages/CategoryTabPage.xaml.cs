using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class CategoryTabPage : Page
{
    public CategoryTabPage()
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
            DataGridCategory.ItemsSource = db.Categories.AsNoTracking().ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 10. Пункт 9: Автообновление
    private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible)
        {
            LoadData();
        }
    }

    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddCategoryPage(null));
    }

    // Шаг 10. Пункт 9: Каскадное удаление категории
    private void ButtonDel_Click(object sender, RoutedEventArgs e)
    {
        var categoryForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

        if (!categoryForRemoving.Any())
        {
            MessageBox.Show("Выберите хотя бы одну категорию для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {categoryForRemoving.Count} элементов? Все связанные опросы также будут удалены!",
            "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new PollsDbContext();
                var ids = categoryForRemoving.Select(c => c.Id).ToList();
                var itemsToDelete = db.Categories.Where(c => ids.Contains(c.Id)).ToList();

                db.Categories.RemoveRange(itemsToDelete);
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
        if ((sender as Button)?.DataContext is Category category)
        {
            NavigationService?.Navigate(new AddCategoryPage(category));
        }
    }
}
