﻿using System;
using System.Collections.Generic;
using System.Linq;
using FatsharkTest.Data;
using FatsharkTest.Models;
using FatsharkTest.Utils;
using OxyPlot;


namespace FatsharkTest.ViewModel;

public class AnalyzedDataViewModel : ViewModelBase
{
    private const int _sampleSize = 10;
    private DataAnalyzer _dataAnalyzer;
    private Database _database;
    
    private Dictionary<string, int> _domains;
    private Dictionary<string, int> _county;
    
    public DataVisualizerViewModel DomainVisualizer { get; private set; }
    public DataVisualizerViewModel CountyVisualizer { get; private set; }
    
    private PostcodeAnalyzer _postcodeAnalyzer;
    public GeoVisualizerViewModel GeoVisualizer { get; private set; }


    public AnalyzedDataViewModel(Database dataBase)
    {
        _postcodeAnalyzer = new PostcodeAnalyzer();
        _database = dataBase;
        
        CreateGeoVisualizer();

        _dataAnalyzer = new DataAnalyzer(_database);

        // Fetch the Top 10 most common values in the sampled data
        _domains = _dataAnalyzer.Domains.Take(_sampleSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _county = _dataAnalyzer.Counties.Take(_sampleSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        DomainVisualizer = new DataVisualizerViewModel(ColorThemes.BarColor1);
        DomainVisualizer.SetData(_domains, "Domains");

        CountyVisualizer = new DataVisualizerViewModel(ColorThemes.BarColor2);
        CountyVisualizer.SetData(_county, "Counties");
    }


    private async void CreateGeoVisualizer()
    {
        GeoVisualizer = new GeoVisualizerViewModel();

        List<Contact> contacts = _database.GetAllContacts();
        List<string> postcodes = contacts.Select(c => c.Postal).ToList();
        List<(double, double)> data = await _postcodeAnalyzer.GetBulkCoordinatesAsync(postcodes);
        
        GeoVisualizer.GeneratePlot(data);
    }
}
