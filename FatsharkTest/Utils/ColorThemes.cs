using System;
using System.Collections.Generic;
using OxyPlot;

namespace FatsharkTest.Utils;

public static class ColorThemes
{
    public static OxyColor Foreground = OxyColor.FromRgb(0xa6, 0xa7, 0xb4);

    
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