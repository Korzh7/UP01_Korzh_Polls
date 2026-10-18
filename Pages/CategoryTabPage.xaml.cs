using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Страница управления категориями опросов: табличный просмотр, добавление, изменение и каскадное удаление
/// </summary>
public partial class CategoryTabPage : Page
{
    /// <summary>
    /// Конструктор страницы категорий опросов
    /// </summary>
    public CategoryTabPage()
    {
        InitializeComponent();
        LoadData();
        this.IsVisibleChanged += Page_IsVisibleChanged;
    }

    /// <summary>
    /// Загрузка полного списка категорий из базы данных в таблицу
    /// </summary>
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

    /// <summary>
    /// Автоматическое обновление данных категорий при повторном отображении страницы
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
    /// Переход на форму добавления новой категории
    /// </summary>
    /// <param name="sender">Кнопка «Добавить»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddCategoryPage(null));
    }

    /// <summary>
    /// Каскадное удаление выбранных категорий и всех связанных с ними опросов
    /// </summary>
    /// <param name="sender">Кнопка «Удалить»</param>
    /// <param name="e">Параметры события клика</param>
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

    /// <summary>
    /// Переход на форму редактирования выбранной категории
    /// </summary>
    /// <param name="sender">Кнопка «Редактировать»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Category category)
        {
            NavigationService?.Navigate(new AddCategoryPage(category));
        }
    }
}
