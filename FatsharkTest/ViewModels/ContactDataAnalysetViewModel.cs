using System.ComponentModel.DataAnnotations;
using FatsharkTest.Data;
using FatsharkTest.Models;

namespace FatsharkTest.ViewModel;

public class ContactDataAnalysetViewModel : ViewModelBase
{
    private AnalyzedDataViewModel _analyzedDataViewModel;
    private ContactListingViewModel _ContactListingViewModel;


    public AnalyzedDataViewModel AnalyzedDataViewModel => _analyzedDataViewModel;
    public ContactListingViewModel ContactListingViewModel => _ContactListingViewModel;
    
    private Database _dataBase;
    
    public ContactDataAnalysetViewModel()
    {
        _dataBase = new Database();
        _dataBase.Initialize();
        
        _analyzedDataViewModel = new AnalyzedDataViewModel(_dataBase);
        _ContactListingViewModel = new ContactListingViewModel(_dataBase);
    }
}