using System;
using System.IO;
using PdfSharpCore.Fonts;

namespace UP01_Korzh_Polls;

public class FileFontResolver : IFontResolver
{
    public string DefaultFontName => "Arial";

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
