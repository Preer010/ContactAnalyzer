using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FatsharkTest.Data;
using Microsoft.Toolkit.Mvvm.Input;

namespace FatsharkTest.ViewModel;

public partial class ContactListingViewModel : ViewModelBase
{
    private Database _database;
    private ObservableCollection<Contact> _contactsListView;
    private int _currentPage;
    private int _totalPages;
    public int PageSize  { get; private set; }
    public int TotalContacts { get; private set; }
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            this._currentPage = value;
            LoadContacts();
            OnPropertyChanged(nameof(CurrentPage));
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set
        {
            this._totalPages = value;
            OnPropertyChanged(nameof(TotalPages));
        }
    }

    
    public ICommand PreviousPageCommand { get; private set; }
    public ICommand NextPageCommand { get; private set; }
    
    public ObservableCollection<Contact> ContactsListView
    {
        get => _contactsListView;
        set
        {
            this._contactsListView = value;
            OnPropertyChanged(nameof(ContactsListView));
        }
    }

    public ContactListingViewModel(Database dataBase)
    {
        _database = dataBase;
        _currentPage = 1;
        PageSize = 100;
        
        TotalContacts = _database.GetTableCount("Contacts");
        PreviousPageCommand = new RelayCommand(PreviousPage, CanGoToPreviousPage);
        NextPageCommand = new RelayCommand(NextPage, CanGoToNextPage);
        _totalPages = CalculateTotalPages();
        LoadContacts();
    }

    private void LoadContacts()
    {
        _contactsListView = new ObservableCollection<Contact>(_database.GetContactPage(CurrentPage, PageSize).ToList());
        OnPropertyChanged(nameof(ContactsListView));
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }

    private void NextPage()
    {
        if (CurrentPage < CalculateTotalPages())
        {
            CurrentPage++;
        }
    }

    private bool CanGoToPreviousPage()
    {
        return CurrentPage > 1;
    }

    private bool CanGoToNextPage()
    {
        return CurrentPage < TotalPages;
    }

    private int CalculateTotalPages()
    {
        return (int)Math.Ceiling((double)TotalContacts / PageSize);
    }
    
    [ICommand]
    private void OnCellEditEnding(DataGridCellEditEndingEventArgs? e)
    {
        if (e.EditingElement is TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
        }

        if (e.Row.Item is Contact editedContact)
        {
            _database.UpdateContact(editedContact);
        }
    }
}