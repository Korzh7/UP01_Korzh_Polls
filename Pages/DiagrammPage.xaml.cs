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
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
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

    // Шаг 12. Пункты 10-11: Построение диаграммы с учетом как созданных опросов, так и участия
    private void UpdateChart(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized) return;
        if (CmbUser.SelectedItem is not UserAccount selectedUser) return;

        try
        {
            using var db = new PollsDbContext();
            var categories = db.Categories.AsNoTracking().OrderBy(c => c.Name).ToList();

            // Опросы, созданные данным пользователем (автор)
            var userCreatedPolls = db.Polls
                .Where(p => p.UserId == selectedUser.Id)
                .AsNoTracking()
                .ToList();

            // Опросы, в которых пользователь участвовал (респондент)
            var participatedPollIds = db.Responses
                .Where(r => r.UserId == selectedUser.Id)
                .Select(r => r.PollId)
                .Distinct()
                .ToList();

            var userParticipatedPolls = db.Polls
                .Where(p => participatedPollIds.Contains(p.Id))
                .AsNoTracking()
                .ToList();

            string activityMode = (CmbActivity?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Сводка";
            bool showCreated = activityMode.Contains("Сводка") || activityMode.Contains("созданные");
            bool showParticipated = activityMode.Contains("Сводка") || activityMode.Contains("участи");

            string chartType = (CmbDiagram.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Столбчатая (Column)";

            if (chartType.Contains("Круговая"))
            {
                CartesianChartControl.Visibility = Visibility.Collapsed;
                PieChartControl.Visibility = Visibility.Visible;

                var pieSeries = new SeriesCollection();

                if (activityMode.Contains("Сводка"))
                {
                    foreach (var cat in categories)
                    {
                        int createdCount = userCreatedPolls.Count(p => p.CategoryId == cat.Id);
                        if (createdCount > 0)
                        {
                            pieSeries.Add(new LiveCharts.Wpf.PieSeries
                            {
                                Title = $"Создано: {cat.Name}",
                                Values = new ChartValues<int> { createdCount },
                                DataLabels = true
                            });
                        }

                        int partCount = userParticipatedPolls.Count(p => p.CategoryId == cat.Id);
                        if (partCount > 0)
                        {
                            pieSeries.Add(new LiveCharts.Wpf.PieSeries
                            {
                                Title = $"Участие: {cat.Name}",
                                Values = new ChartValues<int> { partCount },
                                DataLabels = true
                            });
                        }
                    }
                }
                else if (activityMode.Contains("созданные"))
                {
                    foreach (var cat in categories)
                    {
                        int count = userCreatedPolls.Count(p => p.CategoryId == cat.Id);
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
                }
                else // Только участие
                {
                    foreach (var cat in categories)
                    {
                        int count = userParticipatedPolls.Count(p => p.CategoryId == cat.Id);
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
                }

                if (pieSeries.Count == 0)
                {
                    pieSeries.Add(new LiveCharts.Wpf.PieSeries
                    {
                        Title = "Нет активности",
                        Values = new ChartValues<int> { 0 },
                        DataLabels = true
                    });
                }

                PieChartControl.Series = pieSeries;
            }
            else
            {
                PieChartControl.Visibility = Visibility.Collapsed;
                CartesianChartControl.Visibility = Visibility.Visible;

                var labels = new List<string>();
                var createdValues = new ChartValues<int>();
                var participatedValues = new ChartValues<int>();

                int maxCount = 1;
                foreach (var cat in categories)
                {
                    labels.Add(cat.Name);
                    int cCount = userCreatedPolls.Count(p => p.CategoryId == cat.Id);
                    int pCount = userParticipatedPolls.Count(p => p.CategoryId == cat.Id);

                    createdValues.Add(cCount);
                    participatedValues.Add(pCount);

                    if (cCount > maxCount) maxCount = cCount;
                    if (pCount > maxCount) maxCount = pCount;
                }

                AxisX.Labels = labels;
                AxisX.LabelsRotation = 15;

                string yTitle = (showCreated && showParticipated)
                    ? "Количество опросов (создание / участие)"
                    : (showCreated ? "Количество созданных опросов (автор)" : "Количество пройденных опросов (участие)");

                CartesianChartControl.AxisY.Clear();
                CartesianChartControl.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = yTitle,
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
                    if (showCreated)
                    {
                        seriesCollection.Add(new LiveCharts.Wpf.LineSeries
                        {
                            Title = $"Создано опросов ({selectedUser.FullName})",
                            Values = createdValues,
                            DataLabels = true
                        });
                    }

                    if (showParticipated)
                    {
                        seriesCollection.Add(new LiveCharts.Wpf.LineSeries
                        {
                            Title = $"Пройдено опросов / Участие ({selectedUser.FullName})",
                            Values = participatedValues,
                            DataLabels = true
                        });
                    }
                }
                else // Столбчатая
                {
                    if (showCreated)
                    {
                        seriesCollection.Add(new LiveCharts.Wpf.ColumnSeries
                        {
                            Title = $"Создано опросов ({selectedUser.FullName})",
                            Values = createdValues,
                            DataLabels = true
                        });
                    }

                    if (showParticipated)
                    {
                        seriesCollection.Add(new LiveCharts.Wpf.ColumnSeries
                        {
                            Title = $"Пройдено опросов / Участие ({selectedUser.FullName})",
                            Values = participatedValues,
                            DataLabels = true
                        });
                    }
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
            var allUsers = db.UserAccounts
                .Include(u => u.Polls).ThenInclude(p => p.Category)
                .Include(u => u.Responses).ThenInclude(r => r.Poll).ThenInclude(p => p.Category)
                .OrderBy(u => u.FullName)
                .ToList();
            var allCategories = db.Categories.OrderBy(c => c.Name).ToList();

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
                titleRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text("ОТЧЕТ ПО АКТИВНОСТИ ПОЛЬЗОВАТЕЛЕЙ: СОЗДАНИЕ И УЧАСТИЕ В ОПРОСАХ"));
                var titleProps = titleRun.AppendChild(new RunProperties());
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "30" });
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

                    // Таблица по категориям (создано + участие)
                    DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                    
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
                        CreateCell("Создано опросов (автор)", true),
                        CreateCell("Пройдено опросов (участие)", true),
                        CreateCell("Всего активности", true)
                    );
                    table.Append(headerRow);

                    // Данные по категориям
                    int userTotalCreated = 0;
                    int userTotalParticipated = 0;

                    foreach (var cat in allCategories)
                    {
                        int createdCount = user.Polls.Count(p => p.CategoryId == cat.Id);
                        int participatedCount = user.Responses.Where(r => r.Poll != null && r.Poll.CategoryId == cat.Id).Select(r => r.PollId).Distinct().Count();
                        int catTotal = createdCount + participatedCount;

                        userTotalCreated += createdCount;
                        userTotalParticipated += participatedCount;

                        TableRow dataRow = new TableRow();
                        dataRow.Append(
                            CreateCell(cat.Name, false),
                            CreateCell(createdCount.ToString(), false),
                            CreateCell(participatedCount.ToString(), false),
                            CreateCell(catTotal.ToString(), false)
                        );
                        table.Append(dataRow);
                    }
                    body.Append(table);

                    // Итоги пользователя
                    var sumPara = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    var sumRun = sumPara.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());
                    string statText = $"\nВсего создано: {userTotalCreated} шт. | Всего пройдено опросов: {userTotalParticipated} шт. | Общая активность: {userTotalCreated + userTotalParticipated} ед.";
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

    // Шаг 14: Выгрузка данных в Microsoft Excel (.xlsx) с отображением как созданных опросов, так и участия
    private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new PollsDbContext();
            var allUsers = db.UserAccounts
                .Include(u => u.Polls).ThenInclude(p => p.Category)
                .Include(u => u.Polls).ThenInclude(p => p.Questions)
                .Include(u => u.Responses).ThenInclude(r => r.Poll).ThenInclude(p => p.Category)
                .Include(u => u.Responses).ThenInclude(r => r.Poll).ThenInclude(p => p.User)
                .Include(u => u.Responses).ThenInclude(r => r.Answers)
                .OrderBy(u => u.FullName)
                .ToList();

            string exportDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(exportDir, "Polls_Report.xlsx");

            using (var workbook = new XLWorkbook())
            {
                // Для каждого пользователя создаем отдельный лист (Шаг 14, Пункт 1)
                for (int i = 0; i < allUsers.Count; i++)
                {
                    var user = allUsers[i];
                    string sheetName = string.IsNullOrWhiteSpace(user.FullName) ? $"User_{i + 1}" : user.FullName;
                    if (sheetName.Length > 28) sheetName = sheetName.Substring(0, 28);

                    var worksheet = workbook.Worksheets.Add(sheetName);
                    int startRowIndex = 1;

                    // Блок 1: Опросы, созданные пользователем
                    var banner1 = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                    banner1.Merge();
                    banner1.Value = $"1. ОПРОСЫ, СОЗДАННЫЕ ПОЛЬЗОВАТЕЛЕМ ({user.FullName})";
                    banner1.Style.Font.Bold = true;
                    banner1.Style.Font.FontColor = XLColor.White;
                    banner1.Style.Fill.BackgroundColor = XLColor.FromArgb(35, 62, 118);
                    startRowIndex++;

                    worksheet.Cell(startRowIndex, 1).Value = "Дата создания";
                    worksheet.Cell(startRowIndex, 2).Value = "Название опроса";
                    worksheet.Cell(startRowIndex, 3).Value = "Категория";
                    worksheet.Cell(startRowIndex, 4).Value = "Статус";
                    worksheet.Cell(startRowIndex, 5).Value = "Кол-во вопросов";

                    var headerRange1 = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                    headerRange1.Style.Font.Bold = true;
                    headerRange1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange1.Style.Fill.BackgroundColor = XLColor.FromArgb(1, 152, 175);
                    headerRange1.Style.Font.FontColor = XLColor.White;

                    startRowIndex++;

                    var userCategories = user.Polls
                        .OrderBy(p => p.CreatedAt)
                        .GroupBy(p => p.Category)
                        .OrderBy(g => g.Key?.Name ?? "");

                    int userPollTotal = 0;

                    foreach (var groupCategory in userCategories)
                    {
                        var groupHeader = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                        groupHeader.Merge();
                        groupHeader.Value = $"Категория: {groupCategory.Key?.Name ?? "Без категории"}";
                        groupHeader.Style.Font.Italic = true;
                        groupHeader.Style.Font.Bold = true;
                        groupHeader.Style.Fill.BackgroundColor = XLColor.FromArgb(234, 235, 237);
                        startRowIndex++;

                        int categoryCount = 0;
                        foreach (var poll in groupCategory)
                        {
                            worksheet.Cell(startRowIndex, 1).Value = poll.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                            worksheet.Cell(startRowIndex, 2).Value = poll.Title;
                            worksheet.Cell(startRowIndex, 3).Value = groupCategory.Key?.Name ?? "";
                            worksheet.Cell(startRowIndex, 4).Value = poll.Status;
                            worksheet.Cell(startRowIndex, 5).Value = poll.Questions.Count;

                            categoryCount++;
                            startRowIndex++;
                        }

                        var sumRange = worksheet.Range(startRowIndex, 1, startRowIndex, 4);
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО ПО КАТЕГОРИИ:";
                        sumRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sumRange.Style.Font.Bold = true;

                        worksheet.Cell(startRowIndex, 5).Value = categoryCount;
                        worksheet.Cell(startRowIndex, 5).Style.Font.Bold = true;

                        userPollTotal += categoryCount;
                        startRowIndex++;
                    }

                    if (userPollTotal == 0)
                    {
                        var emptyRow = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                        emptyRow.Merge();
                        emptyRow.Value = "Пользователь не создавал собственных опросов";
                        emptyRow.Style.Font.Italic = true;
                        startRowIndex++;
                    }

                    startRowIndex += 2;

                    // Блок 2: Опросы, в которых пользователь участвовал
                    var banner2 = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                    banner2.Merge();
                    banner2.Value = $"2. ОПРОСЫ, В КОТОРЫХ ПОЛЬЗОВАТЕЛЬ ПРИНЯЛ УЧАСТИЕ ({user.FullName})";
                    banner2.Style.Font.Bold = true;
                    banner2.Style.Font.FontColor = XLColor.White;
                    banner2.Style.Fill.BackgroundColor = XLColor.FromArgb(37, 101, 105);
                    startRowIndex++;

                    worksheet.Cell(startRowIndex, 1).Value = "Дата участия";
                    worksheet.Cell(startRowIndex, 2).Value = "Название опроса";
                    worksheet.Cell(startRowIndex, 3).Value = "Категория";
                    worksheet.Cell(startRowIndex, 4).Value = "Автор опроса";
                    worksheet.Cell(startRowIndex, 5).Value = "Кол-во ответов";

                    var headerRange2 = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                    headerRange2.Style.Font.Bold = true;
                    headerRange2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange2.Style.Fill.BackgroundColor = XLColor.FromArgb(92, 148, 149);
                    headerRange2.Style.Font.FontColor = XLColor.White;
                    startRowIndex++;

                    int userResponseTotal = 0;
                    foreach (var resp in user.Responses.OrderBy(r => r.CreatedAt))
                    {
                        worksheet.Cell(startRowIndex, 1).Value = resp.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                        worksheet.Cell(startRowIndex, 2).Value = resp.Poll?.Title ?? "—";
                        worksheet.Cell(startRowIndex, 3).Value = resp.Poll?.Category?.Name ?? "—";
                        worksheet.Cell(startRowIndex, 4).Value = resp.Poll?.User?.FullName ?? "—";
                        worksheet.Cell(startRowIndex, 5).Value = resp.Answers.Count;

                        userResponseTotal++;
                        startRowIndex++;
                    }

                    if (userResponseTotal == 0)
                    {
                        var emptyRespRow = worksheet.Range(startRowIndex, 1, startRowIndex, 5);
                        emptyRespRow.Merge();
                        emptyRespRow.Value = "Пользователь пока не принял участие ни в одном опросе";
                        emptyRespRow.Style.Font.Italic = true;
                        startRowIndex++;
                    }
                    else
                    {
                        var sumRespRange = worksheet.Range(startRowIndex, 1, startRowIndex, 4);
                        sumRespRange.Merge();
                        sumRespRange.Value = "ИТОГО ПРОЙДЕНО ОПРОСОВ:";
                        sumRespRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        sumRespRange.Style.Font.Bold = true;

                        worksheet.Cell(startRowIndex, 5).Value = userResponseTotal;
                        worksheet.Cell(startRowIndex, 5).Style.Font.Bold = true;
                        startRowIndex++;
                    }

                    var tableRange = worksheet.Range(1, 1, startRowIndex - 1, 5);
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns().AdjustToContents();
                }

                // Сводный лист "Общий итог" (Шаг 14, Пункт 3)
                var summarySheet = workbook.Worksheets.Add("Общий итог");
                summarySheet.Cell(1, 1).Value = "Пользователь";
                summarySheet.Cell(1, 2).Value = "Логин";
                summarySheet.Cell(1, 3).Value = "Роль";
                summarySheet.Cell(1, 4).Value = "Создано опросов";
                summarySheet.Cell(1, 5).Value = "Пройдено опросов";
                summarySheet.Cell(1, 6).Value = "Общая активность";

                var summaryHeader = summarySheet.Range(1, 1, 1, 6);
                summaryHeader.Style.Font.Bold = true;
                summaryHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                summaryHeader.Style.Fill.BackgroundColor = XLColor.FromArgb(35, 62, 118);
                summaryHeader.Style.Font.FontColor = XLColor.White;

                int sumRow = 2;
                int totalAllCreated = 0;
                int totalAllParticipated = 0;

                foreach (var u in allUsers)
                {
                    int createdCount = u.Polls.Count;
                    int partCount = u.Responses.Select(r => r.PollId).Distinct().Count();
                    int totalAct = createdCount + partCount;

                    summarySheet.Cell(sumRow, 1).Value = u.FullName;
                    summarySheet.Cell(sumRow, 2).Value = u.Login;
                    summarySheet.Cell(sumRow, 3).Value = u.Role;
                    summarySheet.Cell(sumRow, 4).Value = createdCount;
                    summarySheet.Cell(sumRow, 5).Value = partCount;
                    summarySheet.Cell(sumRow, 6).Value = totalAct;

                    totalAllCreated += createdCount;
                    totalAllParticipated += partCount;
                    sumRow++;
                }

                // Итоговая строка
                summarySheet.Cell(sumRow, 1).Value = "ВСЕГО ПО ПЛАТФОРМЕ:";
                var totalLabelRange = summarySheet.Range(sumRow, 1, sumRow, 3);
                totalLabelRange.Merge();
                totalLabelRange.Style.Font.Bold = true;
                totalLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                summarySheet.Cell(sumRow, 4).Value = totalAllCreated;
                summarySheet.Cell(sumRow, 4).Style.Font.Bold = true;
                summarySheet.Cell(sumRow, 5).Value = totalAllParticipated;
                summarySheet.Cell(sumRow, 5).Style.Font.Bold = true;
                summarySheet.Cell(sumRow, 6).Value = totalAllCreated + totalAllParticipated;
                summarySheet.Cell(sumRow, 6).Style.Font.Bold = true;
                summarySheet.Cell(sumRow, 6).Style.Font.FontColor = XLColor.Red;

                var summaryRange = summarySheet.Range(1, 1, sumRow, 6);
                summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
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

    // Экспорт аналитического отчета в PDF (.pdf) через PdfSharpCore
    private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new FileFontResolver();
            }

            using var db = new PollsDbContext();
            var allUsers = db.UserAccounts
                .Include(u => u.Polls).ThenInclude(p => p.Category)
                .Include(u => u.Responses).ThenInclude(r => r.Poll).ThenInclude(p => p.Category)
                .OrderBy(u => u.FullName)
                .ToList();
            var allCategories = db.Categories.OrderBy(c => c.Name).ToList();

            string exportDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(exportDir, "Polls_Report.pdf");

            using (var doc = new PdfDocument())
            {
                doc.Info.Title = "Аналитический отчет по опросам и голосованиям";
                doc.Info.Author = "Корж С.А.";
                doc.Info.Subject = "Сводная аналитика активности пользователей платформы";

                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                page.Orientation = PdfSharpCore.PageOrientation.Portrait;

                var gfx = XGraphics.FromPdfPage(page);

                var titleFont = new XFont("Arial", 13, XFontStyle.Bold);
                var subtitleFont = new XFont("Arial", 9, XFontStyle.Regular);
                var sectionFont = new XFont("Arial", 10.5, XFontStyle.Bold);
                var thFont = new XFont("Arial", 8.5, XFontStyle.Bold);
                var cellFont = new XFont("Arial", 8.5, XFontStyle.Regular);
                var cellBoldFont = new XFont("Arial", 8.5, XFontStyle.Bold);
                var footerFont = new XFont("Arial", 7.5, XFontStyle.Italic);

                var primaryColor = XColor.FromArgb(35, 62, 118);
                var secondaryColor = XColor.FromArgb(1, 152, 175);
                var textColor = XColor.FromArgb(34, 34, 34);
                var headerBgColor = XColor.FromArgb(235, 240, 248);
                var totalBgColor = XColor.FromArgb(245, 247, 250);
                var borderColor = XColor.FromArgb(210, 215, 222);

                var primaryBrush = new XSolidBrush(primaryColor);
                var textBrush = new XSolidBrush(textColor);
                var borderPen = new XPen(borderColor, 0.5);
                var headerPen = new XPen(primaryColor, 1);

                double margin = 36;
                double contentWidth = page.Width - 2 * margin;
                double y = margin;

                void CheckNewPage(double neededHeight)
                {
                    if (y + neededHeight > page.Height - 40)
                    {
                        gfx.DrawString($"Стр. {doc.PageCount} | Отчет сформирован автоматически в ИС «Платформа опросов и голосований»",
                            footerFont, XBrushes.Gray, new XRect(margin, page.Height - 30, contentWidth, 15), XStringFormats.Center);

                        gfx.Dispose();
                        page = doc.AddPage();
                        page.Size = PdfSharpCore.PageSize.A4;
                        page.Orientation = PdfSharpCore.PageOrientation.Portrait;
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                    }
                }

                // Шапка документа
                gfx.DrawRectangle(new XSolidBrush(primaryColor), margin, y, contentWidth, 4);
                y += 12;

                gfx.DrawString("ПЛАТФОРМА ПО ПРОВЕДЕНИЮ ОПРОСОВ И ГОЛОСОВАНИЙ", titleFont, primaryBrush, margin, y);
                y += 18;

                gfx.DrawString($"Сводный аналитический отчет об активности пользователей | Дата: {DateTime.Now:dd.MM.yyyy HH:mm}", subtitleFont, XBrushes.DarkSlateGray, margin, y);
                y += 14;

                gfx.DrawLine(new XPen(secondaryColor, 1), margin, y, margin + contentWidth, y);
                y += 16;

                // Раздел 1: Сводная таблица по всем пользователям
                gfx.DrawString("1. Сводная статистика активности пользователей платформы", sectionFont, primaryBrush, margin, y);
                y += 14;

                double[] colWidths = { 170, 95, 80, 58, 60, 60 };
                string[] colHeaders = { "Пользователь (ФИО)", "Логин", "Роль", "Создано", "Участие", "Всего" };

                double rowHeight = 20;
                gfx.DrawRectangle(new XSolidBrush(headerBgColor), margin, y, contentWidth, rowHeight);
                gfx.DrawRectangle(headerPen, margin, y, contentWidth, rowHeight);

                double curX = margin;
                for (int i = 0; i < colWidths.Length; i++)
                {
                    var format = (i >= 3) ? XStringFormats.Center : XStringFormats.CenterLeft;
                    double padLeft = (i < 3) ? 5 : 0;
                    gfx.DrawString(colHeaders[i], thFont, primaryBrush, new XRect(curX + padLeft, y, colWidths[i] - padLeft, rowHeight), format);
                    if (i > 0)
                    {
                        gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);
                    }
                    curX += colWidths[i];
                }
                y += rowHeight;

                int grandTotalCreated = 0;
                int grandTotalParticipated = 0;

                bool isAlternate = false;
                foreach (var u in allUsers)
                {
                    CheckNewPage(rowHeight);

                    int created = u.Polls.Count;
                    int part = u.Responses.Select(r => r.PollId).Distinct().Count();
                    int total = created + part;

                    grandTotalCreated += created;
                    grandTotalParticipated += part;

                    if (isAlternate)
                    {
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(249, 250, 252)), margin, y, contentWidth, rowHeight);
                    }
                    gfx.DrawRectangle(borderPen, margin, y, contentWidth, rowHeight);

                    string[] rowValues = {
                        u.FullName,
                        u.Login,
                        u.Role,
                        created.ToString(),
                        part.ToString(),
                        total.ToString()
                    };

                    curX = margin;
                    for (int i = 0; i < colWidths.Length; i++)
                    {
                        var format = (i >= 3) ? XStringFormats.Center : XStringFormats.CenterLeft;
                        double padLeft = (i < 3) ? 5 : 0;
                        var font = (i == 5) ? cellBoldFont : cellFont;
                        gfx.DrawString(rowValues[i], font, textBrush, new XRect(curX + padLeft, y, colWidths[i] - padLeft, rowHeight), format);
                        if (i > 0)
                        {
                            gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);
                        }
                        curX += colWidths[i];
                    }

                    y += rowHeight;
                    isAlternate = !isAlternate;
                }

                // Итог по пользователям
                CheckNewPage(rowHeight);
                gfx.DrawRectangle(new XSolidBrush(totalBgColor), margin, y, contentWidth, rowHeight);
                gfx.DrawRectangle(headerPen, margin, y, contentWidth, rowHeight);

                double totalLabelWidth = colWidths[0] + colWidths[1] + colWidths[2];
                gfx.DrawString("ИТОГО ПО ВСЕМ ПОЛЬЗОВАТЕЛЯМ:", cellBoldFont, primaryBrush, new XRect(margin + 5, y, totalLabelWidth - 10, rowHeight), XStringFormats.CenterLeft);

                curX = margin + totalLabelWidth;
                gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);
                gfx.DrawString(grandTotalCreated.ToString(), cellBoldFont, primaryBrush, new XRect(curX, y, colWidths[3], rowHeight), XStringFormats.Center);

                curX += colWidths[3];
                gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);
                gfx.DrawString(grandTotalParticipated.ToString(), cellBoldFont, primaryBrush, new XRect(curX, y, colWidths[4], rowHeight), XStringFormats.Center);

                curX += colWidths[4];
                gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);
                gfx.DrawString((grandTotalCreated + grandTotalParticipated).ToString(), cellBoldFont, new XSolidBrush(XColor.FromArgb(180, 40, 40)), new XRect(curX, y, colWidths[5], rowHeight), XStringFormats.Center);

                y += rowHeight + 20;

                // Раздел 2: Сводная активность по категориям
                CheckNewPage(80);
                gfx.DrawString("2. Распределение опросов по тематическим категориям", sectionFont, primaryBrush, margin, y);
                y += 14;

                double[] catColWidths = { 243, 140, 140 };
                string[] catHeaders = { "Тематическая категория", "Опросов создано", "Участий зафиксировано" };

                gfx.DrawRectangle(new XSolidBrush(headerBgColor), margin, y, contentWidth, rowHeight);
                gfx.DrawRectangle(headerPen, margin, y, contentWidth, rowHeight);

                curX = margin;
                for (int i = 0; i < catColWidths.Length; i++)
                {
                    var format = (i == 0) ? XStringFormats.CenterLeft : XStringFormats.Center;
                    double padLeft = (i == 0) ? 5 : 0;
                    gfx.DrawString(catHeaders[i], thFont, primaryBrush, new XRect(curX + padLeft, y, catColWidths[i] - padLeft, rowHeight), format);
                    if (i > 0)
                    {
                        gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);
                    }
                    curX += catColWidths[i];
                }
                y += rowHeight;

                isAlternate = false;
                foreach (var cat in allCategories)
                {
                    CheckNewPage(rowHeight);

                    int catCreated = db.Polls.Count(p => p.CategoryId == cat.Id);
                    int catPart = db.Responses.Where(r => r.Poll != null && r.Poll.CategoryId == cat.Id).Select(r => r.PollId).Distinct().Count();

                    if (isAlternate)
                    {
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(249, 250, 252)), margin, y, contentWidth, rowHeight);
                    }
                    gfx.DrawRectangle(borderPen, margin, y, contentWidth, rowHeight);

                    curX = margin;
                    gfx.DrawString(cat.Name, cellFont, textBrush, new XRect(curX + 5, y, catColWidths[0] - 5, rowHeight), XStringFormats.CenterLeft);
                    curX += catColWidths[0];
                    gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);

                    gfx.DrawString(catCreated.ToString(), cellFont, textBrush, new XRect(curX, y, catColWidths[1], rowHeight), XStringFormats.Center);
                    curX += catColWidths[1];
                    gfx.DrawLine(borderPen, curX, y, curX, y + rowHeight);

                    gfx.DrawString(catPart.ToString(), cellFont, textBrush, new XRect(curX, y, catColWidths[2], rowHeight), XStringFormats.Center);

                    y += rowHeight;
                    isAlternate = !isAlternate;
                }

                gfx.DrawString($"Стр. {doc.PageCount} | Отчет сформирован автоматически в ИС «Платформа опросов и голосований»",
                    footerFont, XBrushes.Gray, new XRect(margin, page.Height - 30, contentWidth, 15), XStringFormats.Center);

                gfx.Dispose();
                doc.Save(filePath);
            }

            MessageBox.Show($"Аналитический отчет PDF успешно сформирован:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте в PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

