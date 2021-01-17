using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

class MyFontResolver : IFontResolver
{
    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo("Arial");
    }

    public byte[] GetFont(string faceName)
    {
        switch (faceName)
        {
            case "Arial":
                return LoadFontData("TryScanMe.Functions.Fonts.arial.ttf"); ;

            case "Arial#b":
                return LoadFontData("TryScanMe.Functions.Fonts.arialbd.ttf"); ;

            case "Arial#i":
                return LoadFontData("TryScanMe.Functions.Fonts.ariali.ttf");

            case "Arial#bi":
                return LoadFontData("TryScanMe.Functions.Fonts.arialbi.ttf");
        }

        return null;
    }

    /// <summary>
    /// Returns the specified font from an embedded resource.
    /// </summary>
    private byte[] LoadFontData(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Test code to find the names of embedded fonts - put a watch on "ourResources"
        //var ourResources = assembly.GetManifestResourceNames();

        using (Stream stream = assembly.GetManifestResourceStream(name))
        {
            if (stream == null)
                throw new ArgumentException("No resource with name " + name);

            int count = (int)stream.Length;
            byte[] data = new byte[count];
            stream.Read(data, 0, count);
            return data;
        }
    }

    internal static MyFontResolver OurGlobalFontResolver = null;

    /// <summary>
    /// Ensure the font resolver is only applied once (or an exception is thrown)
    /// </summary>
    internal static void Apply()
    {
        if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
        {
            if (OurGlobalFontResolver == null)
                OurGlobalFontResolver = new MyFontResolver();

            GlobalFontSettings.FontResolver = OurGlobalFontResolver;
        }
    }
}