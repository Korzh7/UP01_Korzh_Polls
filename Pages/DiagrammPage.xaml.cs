using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using UP01_Korzh_Polls.Models;

namespace UP01_Korzh_Polls.Pages;

public partial class DiagrammPage : System.Windows.Controls.Page
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

                int maxCount = 1;
                foreach (var cat in categories)
                {
                    labels.Add(cat.Name);
                    int count = userPolls.Count(p => p.CategoryId == cat.Id);
                    values.Add(count);
                    if (count > maxCount) maxCount = count;
                }

                AxisX.Labels = labels;
                AxisX.LabelsRotation = 15; // аккуратный наклон, чтобы длинные названия категорий не налезали друг на друга

                // Настройка оси Y для целочисленных значений (0, 1, 2 вместо 0.30000000000000004)
                CartesianChartControl.AxisY.Clear();
                CartesianChartControl.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Количество созданных опросов",
                    Foreground = (System.Windows.Media.Brush)FindResource("TextColor"),
                    MinValue = 0,
                    MaxValue = Math.Max(maxCount + 1, 3),
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelFormatter = val => val.ToString("N0")
                });

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

    // Шаг 13: Выгрузка данных в Microsoft Word (.docx) через DocumentFormat.OpenXml
    private void BtnExportWord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new PollsDbContext();
            var allUsers = db.UserAccounts.Include(u => u.Polls).ThenInclude(p => p.Category).ToList();
            var allCategories = db.Categories.ToList();

            string exportDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(exportDir, "Polls_Report.docx");

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Заголовок документа
                var titlePara = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                var titleRun = titlePara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                titleRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text("ОТЧЕТ ПО АКТИВНОСТИ ПОЛЬЗОВАТЕЛЕЙ И ОПРОСАМ"));
                var titleProps = titleRun.AppendChild(new RunProperties());
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "32" }); // 16pt
                titleProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = "233E76" });

                var datePara = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                var dateRun = datePara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                dateRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}"));
                var dateProps = dateRun.AppendChild(new RunProperties());
                dateProps.AppendChild(new Italic());
                dateProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "20" });

                // Цикл по пользователям (Шаг 13)
                foreach (var user in allUsers)
                {
                    // Имя пользователя
                    var uPara = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    var uRun = uPara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                    uRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text($"Пользователь: {user.FullName} (Роль: {user.Role})"));
                    var uProps = uRun.AppendChild(new RunProperties());
                    uProps.AppendChild(new Bold());
                    uProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "26" });
                    uProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = "0198AF" });

                    // Таблица по категориям
                    DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                    
                    // Стиль границы таблицы
                    TableProperties tblProp = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                        )
                    );
                    table.AppendChild(tblProp);

                    // Шапка таблицы
                    TableRow headerRow = new TableRow();
                    headerRow.Append(
                        CreateCell("Тематическая категория", true),
                        CreateCell("Количество созданных опросов", true)
                    );
                    table.Append(headerRow);

                    // Данные по категориям
                    foreach (var cat in allCategories)
                    {
                        int pollCount = user.Polls.Count(p => p.CategoryId == cat.Id);
                        TableRow dataRow = new TableRow();
                        dataRow.Append(
                            CreateCell(cat.Name, false),
                            CreateCell(pollCount.ToString(), false)
                        );
                        table.Append(dataRow);
                    }
                    body.Append(table);

                    // Итоги пользователя
                    int totalUserPolls = user.Polls.Count;
                    var maxCategory = allCategories
                        .Select(c => new { c.Name, Count = user.Polls.Count(p => p.CategoryId == c.Id) })
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefault();

                    var sumPara = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    var sumRun = sumPara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                    string statText = $"\nВсего создано опросов: {totalUserPolls} шт.";
                    if (maxCategory != null && maxCategory.Count > 0)
                    {
                        statText += $" | Самая активная категория: «{maxCategory.Name}» ({maxCategory.Count} опр.)";
                    }
                    sumRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(statText));
                    var sumProps = sumRun.AppendChild(new RunProperties());
                    sumProps.AppendChild(new Bold());
                    sumProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "22" });
                    sumProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = "256569" });

                    // Разделитель
                    var emptyPara = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    emptyPara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(" ")));
                }

                mainPart.Document.Save();
            }

            MessageBox.Show($"Документ Word успешно сформирован и сохранен:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private TableCell CreateCell(string text, bool isHeader)
    {
        TableCell cell = new TableCell();
        var p = cell.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
        var r = p.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
        r.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(text));

        var rp = r.AppendChild(new RunProperties());
        rp.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = isHeader ? "22" : "20" });
        if (isHeader)
        {
            rp.AppendChild(new Bold());
            rp.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = "233E76" });
        }
        return cell;
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
            string filePath = System.IO.Path.Combine(exportDir, "Polls_Report.xlsx");

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
