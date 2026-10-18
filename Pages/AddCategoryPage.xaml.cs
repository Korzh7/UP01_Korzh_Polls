using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

/// <summary>
/// Форма создания новой или редактирования существующей категории опросов
/// </summary>
public partial class AddCategoryPage : Page
{
    private Category _currentCategory = new Category();

    /// <summary>
    /// Конструктор формы: принимает объект категории для редактирования либо null для создания новой
    /// </summary>
    /// <param name="selectedCategory">Выбранная категория или null</param>
    public AddCategoryPage(Category? selectedCategory)
    {
        InitializeComponent();

        if (selectedCategory != null)
        {
            _currentCategory = selectedCategory;
            HeaderTextBlock.Text = "РЕДАКТИРОВАНИЕ КАТЕГОРИИ";
        }

        DataContext = _currentCategory;
    }

    /// <summary>
    /// Валидация названия категории и сохранение записи в базу данных
    /// </summary>
    /// <param name="sender">Кнопка «Сохранить»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
    {
        _currentCategory.Name = TBCategoryName.Text?.Trim() ?? string.Empty;

        var errors = new StringBuilder();

        if (string.IsNullOrWhiteSpace(_currentCategory.Name))
            errors.AppendLine("Укажите название категории!");

        if (errors.Length > 0)
        {
            MessageBox.Show(errors.ToString(), "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new PollsDbContext();

            if (_currentCategory.Id == 0)
            {
                db.Categories.Add(_currentCategory);
            }
            else
            {
                var existing = db.Categories.FirstOrDefault(c => c.Id == _currentCategory.Id);
                if (existing != null)
                {
                    existing.Name = _currentCategory.Name;
                }
            }

            db.SaveChanges();
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.GoBack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Очистка текстового поля названия категории
    /// </summary>
    /// <param name="sender">Кнопка «Очистить»</param>
    /// <param name="e">Параметры события клика</param>
    private void ButtonClean_Click(object sender, RoutedEventArgs e)
    {
        TBCategoryName.Text = string.Empty;
        _currentCategory.Name = string.Empty;
    }
}
