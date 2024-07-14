﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using FatsharkTest.Data;

namespace FatsharkTest.ViewModel;

public class ContactListingViewModel : ViewModelBase
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
    }
}