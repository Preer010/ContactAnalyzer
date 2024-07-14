using System;
using System.Collections.Generic;
using System.Linq;
using FatsharkTest.Data;
using FatsharkTest.Models;
using OxyPlot;


namespace FatsharkTest.ViewModel;

public class AnalyzedDataViewModel : ViewModelBase
{
    private const int _sampleSize = 10;
    private DataAnalyzer _dataAnalyzer;
    private Database _database;
    
    private Dictionary<string, int> _domains;
    private Dictionary<string, int> _surnames;
    private Dictionary<string, int> _foreNames;
    private Dictionary<string, int> _county;
    private DataVisualizerViewModel _domainVisualizer;
    private DataVisualizerViewModel _forenameVisualizer;
    private DataVisualizerViewModel _surnameVisualizer;
    private DataVisualizerViewModel _CountyVisualizer;
    
    private PostcodeAnalyzer _postcodeAnalyzer;
    private GeoVisualizerViewModel _geoVisualizer;


    public GeoVisualizerViewModel GeoVisualizer
    {
        get => _geoVisualizer;
    }
    
    public DataVisualizerViewModel DomainVisualizer
    {
        get => _domainVisualizer;
    }

    public DataVisualizerViewModel ForenameVisualizer
    {
        get => _forenameVisualizer;
    }

    public DataVisualizerViewModel SurnameVisualizer
    {
        get => _surnameVisualizer;
    }
    public DataVisualizerViewModel CountyVisualizer
    {
        get => _CountyVisualizer;
    }
    public AnalyzedDataViewModel(Database dataBase)
    {
        _postcodeAnalyzer = new PostcodeAnalyzer();
        _database = dataBase;
        
        CreateGeoVisualizer();

        _dataAnalyzer = new DataAnalyzer(_database);

        // Fetch the Top 10 most common values in the sampled data
        _domains = _dataAnalyzer.Domains.Take(_sampleSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _surnames = _dataAnalyzer.Surnames.Take(_sampleSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _foreNames = _dataAnalyzer.ForeNames.Take(_sampleSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _county = _dataAnalyzer.Counties.Take(_sampleSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var domainColor = OxyColor.FromRgb(0x2e, 0x46, 0x69);
        _domainVisualizer = new DataVisualizerViewModel(domainColor);
        _domainVisualizer.SetData(_domains, "Domains");

        var foreName = OxyColor.FromRgb(0xef, 0x84, 0x3c);
        _forenameVisualizer = new DataVisualizerViewModel(foreName);
        _forenameVisualizer.SetData(_foreNames, "Forenames");

        var surnameColor = OxyColor.FromRgb(0x50, 0x3c, 0x60);
        _surnameVisualizer = new DataVisualizerViewModel(surnameColor);
        _surnameVisualizer.SetData(_surnames, "Surnames");       
        
        var countyColor = OxyColor.FromRgb(0x50, 0x3c, 0x60);
        _CountyVisualizer = new DataVisualizerViewModel(countyColor);
        _CountyVisualizer.SetData(_county, "Counties");
    }


    private async void CreateGeoVisualizer()
    {
        _geoVisualizer = new GeoVisualizerViewModel();

        var contacts = _database.GetAllContacts();
        List<string> postcodes = contacts.Select(c => c.Postal).ToList();
        var data = await _postcodeAnalyzer.GetBulkCoordinatesAsync(postcodes);
        
        _geoVisualizer.GeneratePlot(data);
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
