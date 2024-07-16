using System;
using System.Collections.Generic;
using System.Linq;
using FatsharkTest.Utils;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace FatsharkTest.ViewModels;

public class GeoVisualizerViewModel : ViewModelPlotBase
{
    public GeoVisualizerViewModel()
    {
        _dataController.UnbindMouseDown(OxyMouseButton.Left);
        _dataController.UnbindMouseDown(OxyMouseButton.Middle);
    }
    public void GeneratePlot(List<GeoPoint> data)
    {
        _dataPlot = new OxyPlot.PlotModel();
        _dataPlot.Title = "Groups Geographically Close To Each Other";
        
        // Create the clusters with the clustering algorithm
        KMeans alg = new KMeans(data, 7, 10);
        alg.Run();

        List<GeoPoint> points = alg.GeoPoints;
        var clusters = points.GroupBy(p => p.ClusterId);
        clusters = clusters.OrderByDescending(c => c.Count());

        List<OxyColor> clusterColors = ColorThemes.GenerateRandomColors(clusters.Count());
        
        // Create plot series for each cluster
        foreach (IGrouping<int, GeoPoint> cluster in clusters)
        {
            Random random = new Random();
            ScatterSeries scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = clusterColors[cluster.Key],
                Title = $"{cluster.Count()}",
            };

            foreach (GeoPoint point in cluster)
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
        OxyColor foregroundColor = ColorThemes.Foreground;
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
    

}