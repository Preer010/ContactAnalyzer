using FatsharkTest.Data;

namespace FatsharkTest.ViewModel;

public class ContactDataAnalysetViewModel : ViewModelBase
{
    
    public AnalyzedDataViewModel AnalyzedDataViewModel { get; private set; }
    public ContactListingViewModel ContactListingViewModel { get; private set; }
    
    private Database _dataBase;
    
    public ContactDataAnalysetViewModel()
    {
        _dataBase = new Database();
        _dataBase.Initialize();
        
        AnalyzedDataViewModel = new AnalyzedDataViewModel(_dataBase);
        ContactListingViewModel = new ContactListingViewModel(_dataBase);
    }
}