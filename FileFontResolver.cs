using System;
using System.IO;
using PdfSharpCore.Fonts;

namespace UP01_Korzh_Polls;

/// <summary>
/// Резолвер системных шрифтов для корректной генерации PDF-документов с поддержкой русского языка
/// </summary>
public class FileFontResolver : IFontResolver
{
    /// <summary>
    /// Имя базового шрифта системы по умолчанию
    /// </summary>
    public string DefaultFontName => "Arial";

    /// <summary>
    /// Загрузка байтов файла шрифта из системной директории шрифтов Windows
    /// </summary>
    /// <param name="faceName">Имя файла шрифта</param>
    /// <returns>Массив байтов файла шрифта или null</returns>
    public byte[]? GetFont(string faceName)
    {
        string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string fontPath = Path.Combine(fontsFolder, faceName);
        if (File.Exists(fontPath))
        {
            return File.ReadAllBytes(fontPath);
        }
        string arialPath = Path.Combine(fontsFolder, "arial.ttf");
        if (File.Exists(arialPath))
        {
            return File.ReadAllBytes(arialPath);
        }
        return null;
    }

    /// <summary>
    /// Сопоставление гарнитуры шрифта и его начертания с файлом шрифта TrueType
    /// </summary>
    /// <param name="familyName">Семейство шрифта</param>
    /// <param name="isBold">Флаг полужирного начертания</param>
    /// <param name="isItalic">Флаг курсивного начертания</param>
    /// <returns>Дескриптор шрифта FontResolverInfo</returns>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase) ||
            familyName.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase) ||
            familyName.Equals("Times New Roman", StringComparison.OrdinalIgnoreCase))
        {
            if (isBold && isItalic) return new FontResolverInfo("arialbi.ttf");
            if (isBold) return new FontResolverInfo("arialbd.ttf");
            if (isItalic) return new FontResolverInfo("ariali.ttf");
            return new FontResolverInfo("arial.ttf");
        }
        return new FontResolverInfo("arial.ttf");
    }
}
