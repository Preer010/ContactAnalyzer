using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        List<Contact> contacts = dataBase.GetAllContacts();
        _contactsListView = new ObservableCollection<Contact>(contacts);
        OnPropertyChanged(nameof(ContactsListView));
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