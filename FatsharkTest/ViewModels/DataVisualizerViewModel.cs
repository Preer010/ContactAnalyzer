using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace FatsharkTest.ViewModel;

public class DataVisualizerViewModel : ViewModelPlotBase
{
    private string _title;
    private BarSeries _barSeries;
    public string Title
    {
        get => _title;
        set  {
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }
    public OxyPlot.PlotModel DataPlot
    {
        get => _dataPlot;
        set {
            _dataPlot = value;
            OnPropertyChanged(nameof(DataPlot));
        }
    }
    public DataVisualizerViewModel(OxyColor BarColor)
    {
        OxyColor axisColor = OxyColor.FromRgb(0xa6, 0xa7, 0xb4);

        _dataPlot.Title = "Most Common Domains";
        _barSeries = new BarSeries();
        _barSeries.IsStacked = false;
        _barSeries.FillColor = BarColor;
        _dataPlot.TitleColor  = axisColor;
        _dataPlot.PlotAreaBorderThickness = new OxyThickness(0);
        
        _dataController.UnbindAll();
    }

    public void SetData(Dictionary<string, int> inData, string inTitle )
    {
        _title = inTitle;
        _dataPlot.Series.Clear();
        _dataPlot.Axes.Clear();
        _barSeries.Items.Clear();
        _dataPlot.Title = $"Most Common {inTitle}";
        var sortedData = inData.Reverse();
        List<string> axisNames = new List<string>();
        
        foreach ((string type, int count) in sortedData)
        {
            _barSeries.Items.Add(new BarItem(count));
            axisNames.Add(type);
        }

        OxyColor axisColor = OxyColor.FromRgb(0xa6, 0xa7, 0xb4);
        
        _dataPlot.Axes.Add(new LinearAxis()
        {
            Position = AxisPosition.Bottom,
            TitleColor = axisColor,
            TextColor = axisColor,
            AxislineColor = axisColor,
            TicklineColor = axisColor,
        });
        _dataPlot.Axes.Add(new CategoryAxis
        {
            IsZoomEnabled = false,
            Position = AxisPosition.Left, 
            TitleColor = axisColor,
            TextColor = axisColor,
            AxislineColor = axisColor,
            TicklineColor = axisColor,
            FontSize = 16,
            Key=$"MostCommon{inTitle}",
            ItemsSource = axisNames
        });
        _dataPlot.Series.Add(_barSeries);
    }
}