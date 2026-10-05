using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;
using Xceed.Words.NET;

namespace UP01_Korzh_Polls.Pages;

public partial class DiagrammPage : Page
{
    public DiagrammPage()
    {
        InitializeComponent();
        LoadInitialData();
    }

    private void LoadInitialData()
    {
        try
        {
            using var db = new PollsDbContext();
            var users = db.UserAccounts.AsNoTracking().ToList();
            CmbUser.ItemsSource = users;
            if (users.Any())
            {
                CmbUser.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 12. Пункты 10-11: Построение диаграммы
    private void UpdateChart(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized) return;
        if (CmbUser.SelectedItem is not UserAccount selectedUser) return;

        try
        {
            using var db = new PollsDbContext();
            var categories = db.Categories.AsNoTracking().ToList();
            var userPolls = db.Polls
                .Where(p => p.UserId == selectedUser.Id)
                .AsNoTracking()
                .ToList();

            string chartType = (CmbDiagram.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Столбчатая (Column)";

            if (chartType.Contains("Круговая"))
            {
                CartesianChartControl.Visibility = Visibility.Collapsed;
                PieChartControl.Visibility = Visibility.Visible;

                var pieSeries = new SeriesCollection();
                foreach (var cat in categories)
                {
                    int count = userPolls.Count(p => p.CategoryId == cat.Id);
                    if (count > 0)
                    {
                        pieSeries.Add(new LiveCharts.Wpf.PieSeries
                        {
                            Title = cat.Name,
                            Values = new ChartValues<int> { count },
                            DataLabels = true
                        });
                    }
                }
                PieChartControl.Series = pieSeries;
            }
            else
            {
                PieChartControl.Visibility = Visibility.Collapsed;
                CartesianChartControl.Visibility = Visibility.Visible;

                var labels = new List<string>();
                var values = new ChartValues<int>();

                foreach (var cat in categories)
                {
                    labels.Add(cat.Name);
                    int count = userPolls.Count(p => p.CategoryId == cat.Id);
                    values.Add(count);
                }

                AxisX.Labels = labels;

                var seriesCollection = new SeriesCollection();
                if (chartType.Contains("Линейная"))
                {
                    seriesCollection.Add(new LiveCharts.Wpf.LineSeries
                    {
                        Title = $"Опросы ({selectedUser.FullName})",
                        Values = values,
                        DataLabels = true
                    });
                }
                else
                {
                    seriesCollection.Add(new LiveCharts.Wpf.ColumnSeries
                    {
                        Title = $"Опросы ({selectedUser.FullName})",
                        Values = values,
                        DataLabels = true
                    });
                }

                CartesianChartControl.Series = seriesCollection;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка обновления графика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 13: Выгрузка данных в Microsoft Word (.docx)
    private void BtnExportWord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new PollsDbContext();
            var allUsers = db.UserAccounts.Include(u => u.Polls).ThenInclude(p => p.Category).ToList();
            var allCategories = db.Categories.ToList();

            string exportDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(exportDir, "Polls_Report.docx");

            using (var doc = DocX.Create(filePath))
            {
                // Верхний колонтитул с текущей датой (Шаг 13, Пункт 4)
                doc.AddHeaders();
                var header = doc.Headers.Odd;
                header.InsertParagraph($"Дата формирования отчета: {DateTime.Now:dd.MM.yyyy HH:mm}")
                      .FontSize(9).Italic().Alignment = Xceed.Document.NET.Alignment.right;

                // Нижний колонтитул с номером страниц (Шаг 13, Пункт 3)
                doc.AddFooters();
                var footer = doc.Footers.Odd;
                var footerPara = footer.InsertParagraph("Страница ");
                footerPara.AppendPageNumber(Xceed.Document.NET.PageNumberFormat.normal);
                footerPara.Append(" из ");
                footerPara.AppendPageCount(Xceed.Document.NET.PageNumberFormat.normal);
                footerPara.Alignment = Xceed.Document.NET.Alignment.center;
                footerPara.FontSize(9);

                // Заголовок документа
                var titlePara = doc.InsertParagraph("ОТЧЕТ ПО АКТИВНОСТИ ПОЛЬЗОВАТЕЛЕЙ И ОПРОСАМ");
                titlePara.FontSize(16).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                titlePara.SpacingAfter(20);

                // Цикл по пользователям (Шаг 13)
                for (int uIdx = 0; uIdx < allUsers.Count; uIdx++)
                {
                    var user = allUsers[uIdx];

                    var userPara = doc.InsertParagraph(user.FullName);
                    userPara.FontSize(14).Bold();
                    userPara.SpacingBefore(10);
                    userPara.SpacingAfter(10);

                    // Таблица по категориям
                    var table = doc.AddTable(allCategories.Count + 1, 2);
                    table.Alignment = Xceed.Document.NET.Alignment.center;
                    table.Rows[0].Cells[0].Paragraphs.First().Append("Тематическая категория").Bold();
                    table.Rows[0].Cells[1].Paragraphs.First().Append("Количество опросов").Bold();

                    for (int i = 0; i < allCategories.Count; i++)
                    {
                        var cat = allCategories[i];
                        int pollCount = user.Polls.Count(p => p.CategoryId == cat.Id);

                        table.Rows[i + 1].Cells[0].Paragraphs.First().Append(cat.Name);
                        table.Rows[i + 1].Cells[1].Paragraphs.First().Append(pollCount.ToString()).Alignment = Xceed.Document.NET.Alignment.center;
                    }

                    doc.InsertTable(table);

                    // Максимальный и минимальный показатель
                    int totalUserPolls = user.Polls.Count;
                    var maxCategory = allCategories
                        .Select(c => new { c.Name, Count = user.Polls.Count(p => p.CategoryId == c.Id) })
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefault();

                    var infoPara = doc.InsertParagraph();
                    infoPara.SpacingBefore(10);
                    if (maxCategory != null && maxCategory.Count > 0)
                    {
                        infoPara.Append($"Наиболее популярная категория: {maxCategory.Name} ({maxCategory.Count} опр.)\n")
                                .Color(Xceed.Drawing.Color.DarkRed).Italic();
                    }
                    infoPara.Append($"Всего создано опросов пользователем: {totalUserPolls} шт.\n")
                            .Color(Xceed.Drawing.Color.DarkGreen).Bold();

                    if (uIdx < allUsers.Count - 1)
                    {
                        doc.InsertSectionPageBreak();
                    }
                }

                doc.Save();
            }

            MessageBox.Show($"Документ Word успешно сформирован и сохранен:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Шаг 14: Выгрузка данных в Microsoft Excel (.xlsx)
    private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new PollsDbContext();
            var allUsers = db.UserAccounts
                .Include(u => u.Polls)
                .ThenInclude(p => p.Category)
                .OrderBy(u => u.FullName)
                .ToList();

            string exportDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(exportDir, "Polls_Report.xlsx");

            using (var workbook = new XLWorkbook())
            {
                int grandTotal = 0;

                // Для каждого пользователя создаем отдельный лист (Шаг 14, Пункт 1)
                for (int i = 0; i < allUsers.Count; i++)
                {
                    var user = allUsers[i];
                    string sheetName = string.IsNullOrWhiteSpace(user.FullName) ? $"User_{i + 1}" : user.FullName;
                    if (sheetName.Length > 28) sheetName = sheetName.Substring(0, 28);

                    var worksheet = workbook.Worksheets.Add(sheetName);
                    int startRowIndex = 1;

                    // Заголовки таблицы
                    worksheet.Cell(startRowIndex, 1).Value = "Дата создания";
                    worksheet.Cell(startRowIndex, 2).Value = "Название опроса";
                    worksheet.Cell(startRowIndex, 3).Value = "Категория";
                    worksheet.Cell(startRowIndex, 4).Value = "Статус";
                    worksheet.Cell(startRowIndex, 5).Value = "Кол-во вопросов";

                    var headerRange = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(1, 152, 175);
                    headerRange.Style.Font.FontColor = XLColor.White;

                    startRowIndex++;

                    // Группировка опросов пользователя по категориям (Шаг 14, Пункт 2)
                    var userCategories = user.Polls
                        .OrderBy(p => p.CreatedAt)
                        .GroupBy(p => p.Category)
                        .OrderBy(g => g.Key.Name);

                    int userPollTotal = 0;

                    foreach (var groupCategory in userCategories)
                    {
                        var groupHeader = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                        groupHeader.Merge();
                        groupHeader.Value = $"Категория: {groupCategory.Key.Name}";
                        groupHeader.Style.Font.Italic = true;
                        groupHeader.Style.Font.Bold = true;
                        groupHeader.Style.Fill.BackgroundColor = XLColor.FromArgb(234, 235, 237);
                        startRowIndex++;

                        int categoryCount = 0;
                        foreach (var poll in groupCategory)
                        {
                            worksheet.Cell(startRowIndex, 1).Value = poll.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                            worksheet.Cell(startRowIndex, 2).Value = poll.Title;
                            worksheet.Cell(startRowIndex, 3).Value = groupCategory.Key.Name;
                            worksheet.Cell(startRowIndex, 4).Value = poll.Status;
                            worksheet.Cell(startRowIndex, 5).Value = 1;

                            categoryCount++;
                            startRowIndex++;
                        }

                        // Итого по категории
                        var sumRange = worksheet.Range(startRowIndex, 1, startRowIndex, 4);
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО ПО КАТЕГОРИИ:";
                        sumRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sumRange.Style.Font.Bold = true;

                        worksheet.Cell(startRowIndex, 5).FormulaA1 = $"=SUM(E{startRowIndex - categoryCount}:E{startRowIndex - 1})";
                        worksheet.Cell(startRowIndex, 5).Style.Font.Bold = true;

                        userPollTotal += categoryCount;
                        startRowIndex++;
                    }

                    // Границы таблицы
                    var tableRange = worksheet.Range(1, 1, startRowIndex - 1, 5);
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns().AdjustToContents();

                    grandTotal += userPollTotal;
                }

                // Сводный лист "Общий итог" (Шаг 14, Пункт 3)
                var summarySheet = workbook.Worksheets.Add("Общий итог");
                summarySheet.Cell(1, 1).Value = "Общий итог опросов в системе:";
                summarySheet.Cell(1, 2).Value = grandTotal;
                summarySheet.Cell(1, 1).Style.Font.Bold = true;
                summarySheet.Cell(1, 2).Style.Font.Bold = true;
                summarySheet.Cell(1, 2).Style.Font.FontColor = XLColor.Red;
                summarySheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }

            MessageBox.Show($"Книга Excel успешно сформирована:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
