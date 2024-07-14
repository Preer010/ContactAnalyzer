using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using CsvHelper;
using CsvHelper.Configuration;

namespace FatsharkTest.Data;

using System;
using System.Data.SQLite;

public class Database
{
    private SQLiteConnection _sqLiteConnection;
    private Dictionary<string, int> _domains;
    private Dictionary<string, int> _surnames;
    private Dictionary<string, int> _forenames;
    
    
    public void Initialize()
    {
        GenerateSQL();
        
        string domainQuery = @"
                    SELECT substr(Email, instr(Email, '@') + 1) AS Domain, COUNT(*) AS Count
                    FROM Contacts
                    GROUP BY Domain
                    ORDER BY Count DESC;";
        _domains = QueryCount(domainQuery, "Domain");

        string surnameQuery = @"
            SELECT LastName AS Surname, COUNT(*) AS Count
            FROM Contacts
            GROUP BY Surname
            ORDER BY Count DESC;";

        _surnames = QueryCount(surnameQuery, "Surname");

        string forenameQuery = @"
            SELECT FirstName AS ForeName, COUNT(*) AS Count
            FROM Contacts
            GROUP BY ForeName
            ORDER BY Count DESC;";

        _forenames = QueryCount(forenameQuery, "ForeName");
        
    }

    private void GenerateSQL()
    {
        string connectionString = "Data Source=contacts.db;Version=3";
        _sqLiteConnection = new SQLiteConnection(connectionString);

        try
        {
            _sqLiteConnection.Open();
            {
                string dropTableQuery = "DROP TABLE IF EXISTS Contacts;";
                string createTableQuery = @"
                    CREATE TABLE Contacts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
	                    Email	TEXT,
	                    FirstName	TEXT,
	                    LastName	TEXT,
	                    County	TEXT,
	                    Postal	TEXT
                    );
                ";

                using (SQLiteCommand command = new SQLiteCommand(dropTableQuery, _sqLiteConnection))
                {
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, _sqLiteConnection))
                {
                    command.ExecuteNonQuery();
                }
            }

            {
                string dropTableQuery = "DROP TABLE IF EXISTS GeoLocation;";
                string createGeoQuery = @"
                CREATE TABLE GeoLocation (
	                ID	    INTEGER,
	                Lat	    INTEGER,
	                Long	INTEGER,
	                PRIMARY KEY(ID)
                );
            ";
                using (SQLiteCommand command = new SQLiteCommand(dropTableQuery, _sqLiteConnection))
                {
                    command.ExecuteNonQuery();
                }
                using (SQLiteCommand command = new SQLiteCommand(createGeoQuery, _sqLiteConnection))
                {
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Contacts Table has been created");

            ImportCsvData("uk-500.csv");

            _sqLiteConnection.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            throw;
        }
    }

    private void ImportCsvData(string csvFile)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using (var reader = new StreamReader(csvFile))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<ContactMap>();

            var contacts = csv.GetRecords<Contact>().ToList();

            var transaction = _sqLiteConnection.BeginTransaction();

            foreach (var contact in contacts)
            {
                var insertCommand = new SQLiteCommand(_sqLiteConnection);
                insertCommand.CommandText = @"
                INSERT INTO Contacts (FirstName, LastName,County, Postal, Email)
                VALUES (@FirstName, @LastName, @County, @Postal, @Email);
            ";
                insertCommand.Parameters.AddWithValue("@FirstName", contact.FirstName);
                insertCommand.Parameters.AddWithValue("@LastName", contact.LastName);
                insertCommand.Parameters.AddWithValue("@County", contact.County);
                insertCommand.Parameters.AddWithValue("@Postal", contact.Postal);
                insertCommand.Parameters.AddWithValue("@Email", contact.Email);
                insertCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public Dictionary<string, int> QueryCount(string sqlQuery, string queryType)
    {
        Dictionary<string, int> dataCounts = new Dictionary<string, int>();

        _sqLiteConnection.Open();
        // SQL query to count occurrences of each domain
       

        using (var command = new SQLiteCommand(sqlQuery, _sqLiteConnection))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string type = reader[queryType].ToString();
                    int count = Convert.ToInt32(reader["Count"]);
                    dataCounts[type] = count;
                }
            }
        }

        _sqLiteConnection.Close();
        return dataCounts;
    }
    
    public List<Contact> GetAllContacts()
    {
        List<Contact> contacts = new List<Contact>();
 
        _sqLiteConnection.Open();
        SQLiteCommand cmd = new SQLiteCommand("SELECT FirstName, LastName, County, Postal, Email FROM Contacts", _sqLiteConnection);
        SQLiteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Contact contact = new Contact
            {                
                FirstName = reader.GetString(0),
                LastName = reader.GetString(1),
                County = reader.GetString(2),
                Postal = reader.GetString(3),
                Email = reader.GetString(4),
            };
            contacts.Add(contact);
        }
        _sqLiteConnection.Close();

        return contacts;
    }
}


public class Contact
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string County { get; set; }
    public string Postal { get; set; }
    public string Email { get; set; }

    public override string ToString()
    {
        return $"{this.FirstName} {this.LastName}, {this.Email}";
    }
}

public class ContactMap : ClassMap<Contact>
{
    public ContactMap()
    {
        Map(m => m.FirstName).Name("first_name");
        Map(m => m.LastName).Name("last_name");
        Map(m => m.County).Name("county");
        Map(m => m.Postal).Name("postal");
        Map(m => m.Email).Name("email");
    }
}