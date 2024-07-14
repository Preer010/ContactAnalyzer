using System;
using System.Collections.Generic;
using System.Linq;
using FatsharkTest.Utils;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace FatsharkTest.ViewModel;

public class GeoVisualizerViewModel : ViewModelPlotBase
{
    public GeoVisualizerViewModel()
    {
        _dataController.UnbindMouseDown(OxyMouseButton.Left);
        _dataController.UnbindMouseDown(OxyMouseButton.Middle);
    }
    public void GeneratePlot(List<(double, double)> data)
    {
        _dataPlot = new OxyPlot.PlotModel();
        _dataPlot.Title = "Groups Geographically Close To Each Other";
        
        
        K_Means alg = new K_Means(data, 7, 10);
        alg.Run();
        var points = alg.GeoPoints;

        var clusters = points.GroupBy(p => p.ClusterId);

        clusters = clusters.OrderByDescending(c => c.Count());
        var clusterColors = GenerateRandomColors(clusters.Count());
        
        foreach (var cluster in clusters)
        {
            Random random = new Random();
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = clusterColors[cluster.Key],
                Title = $"{cluster.Count()}",
            };

            foreach (var point in cluster)
            {
                scatterSeries.Points.Add(new ScatterPoint(point.Longitude, point.Latitude, 5, cluster.Count()));
            }
            
            _dataPlot.Series.Add(scatterSeries);
        }
        
        
        SetPlotStyle();
        OnPropertyChanged(nameof(DataPlot));
    }

    private void SetPlotStyle()
    {
        OxyColor foregroundColor = OxyColor.FromRgb(0xa6, 0xa7, 0xb4);
        _dataPlot.TextColor = foregroundColor;

        // Set axes
        _dataPlot.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom, Title = "Longitude",
            AxislineColor = foregroundColor,
           TicklineColor = foregroundColor
        });
        _dataPlot.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left, Title = "Latitude",
            AxislineColor = foregroundColor,
            TicklineColor = foregroundColor,
        });

        _dataPlot.PlotAreaBorderThickness = new OxyThickness(0);
        
        Legend legend = new Legend();
        legend.LegendPosition = LegendPosition.TopRight;
        legend.LegendPlacement = LegendPlacement.Inside;
        legend.TextColor = foregroundColor;
        legend.LegendBorderThickness = 1;
        _dataPlot.Legends.Add(legend);
        _dataPlot.IsLegendVisible = true;
    }
    
    private List<OxyColor> GenerateRandomColors(int count)
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