using OxyPlot;

namespace FatsharkTest.ViewModels;

public class ViewModelPlotBase : ViewModelBase
{
    protected PlotModel _dataPlot = new PlotModel();
    protected PlotController _dataController = new PlotController() ;

    public PlotController DataController
    {
        get => _dataController;
        set {
            _dataController = value;
            OnPropertyChanged(nameof(DataController));
        }
    }
    public PlotModel DataPlot
    {
        get => _dataPlot;
        set {
            _dataPlot = value;
            OnPropertyChanged(nameof(DataPlot));
        }
    }
}