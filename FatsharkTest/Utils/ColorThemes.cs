using System;
using System.Collections.Generic;
using OxyPlot;

namespace FatsharkTest.Utils;

public static class ColorThemes
{
    public static OxyColor Foreground = OxyColor.FromRgb(0xa6, 0xa7, 0xb4);
    public static OxyColor BarColor1 = OxyColor.FromRgb(0x2e, 0x46, 0x69);
    public static OxyColor BarColor2 = OxyColor.FromRgb(0x50, 0x3c, 0x60);
    
    public static List<OxyColor> GenerateRandomColors(int count)
    {
        Random random = new Random();
        List<OxyColor> colors = new List<OxyColor>();
        for (int i = 0; i < count; i++)
        {
            colors.Add(OxyColor.FromRgb(
                (byte)random.Next(64, 256),
                (byte)random.Next(64, 256),
                (byte)random.Next(64, 256)));
        }
        return colors;
    }
}