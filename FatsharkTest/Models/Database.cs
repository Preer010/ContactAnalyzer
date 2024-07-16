﻿using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace FatsharkTest.Data;

using System;
using System.Data.SQLite;
using CountDictionary = Dictionary<string, int>;

public class Database
{
    private SQLiteConnection _sqLiteConnection;

    private CountDictionary _domains;
    private CountDictionary _counties;

    public void Initialize()
    {
        GenerateSQL();

        _domains = DatabaseQueries.CountDomains(this);
        _counties = DatabaseQueries.CountCounties(this);
    }

    private void GenerateSQL()
    {
        string connectionString = "Data Source=contacts.db;Version=3";
        _sqLiteConnection = new SQLiteConnection(connectionString);

        try
        {
            bool isNewDatabase = false;
            {
                _sqLiteConnection.Open();

                string checkTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='Contacts';";
                using (SQLiteCommand command = new SQLiteCommand(checkTableQuery, _sqLiteConnection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        isNewDatabase = true;
                    }
                }

                if (isNewDatabase)
                {
                    string dropTableQuery = "DROP TABLE IF EXISTS Contacts;";
                    string createTableQuery = @"
                    CREATE TABLE Contacts (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
	                    Email	    TEXT UNIQUE,
	                    FirstName	TEXT,
	                    LastName	TEXT,
	                    County	    TEXT,
	                    Postal	    TEXT
                    );";

                    using (SQLiteCommand command = new SQLiteCommand(dropTableQuery, _sqLiteConnection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(createTableQuery, _sqLiteConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                    _sqLiteConnection.Close();

                    ImportCsvData("uk-500.csv");
                }
                else
                {
                    _sqLiteConnection.Close();

                     if (GetTableCount() == 0)
                    {
                        ImportCsvData("uk-500.csv");
                    }

                }
            }
            {
                _sqLiteConnection.Open();
                string dropTableQuery = "DROP TABLE IF EXISTS GeoLocation;";
                string createGeoQuery = @"
                    CREATE TABLE GeoLocation (
	                    Postal	        TEXT,
	                    Latitude	    INTEGER,
	                    Longitude	    INTEGER,
	                    PRIMARY KEY(Postal)
                    );";

                using (SQLiteCommand command = new SQLiteCommand(dropTableQuery, _sqLiteConnection))
                {
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand(createGeoQuery, _sqLiteConnection))
                {
                    command.ExecuteNonQuery();
                }
                _sqLiteConnection.Close();

            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            throw;
        }
    }

    public int GetTableCount()
    {
        string checkEmptyTableQuery = "SELECT COUNT(*) FROM Contacts;";

        _sqLiteConnection.Open();
        using (SQLiteCommand command = new SQLiteCommand(checkEmptyTableQuery, _sqLiteConnection))
        {
            int count = Convert.ToInt32(command.ExecuteScalar());
            _sqLiteConnection.Close();
            return count;
        }
    }

    private void ImportCsvData(string csvFile)
    {
        _sqLiteConnection.Open();
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using (StreamReader reader = new StreamReader(csvFile))
        using (CsvReader csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<ContactMap>();
            List<Contact> contacts = csv.GetRecords<Contact>().ToList();

            const int batchSize = 100;
            using (SQLiteTransaction transaction = _sqLiteConnection.BeginTransaction())
            {
                for (int batchStart = 0; batchStart < contacts.Count; batchStart += batchSize)
                {
                    List<Contact> batchContacts = contacts.Skip(batchStart).Take(batchSize).ToList();
                    SQLiteCommand insertCommand = new SQLiteCommand(_sqLiteConnection);
                    StringBuilder sb = new StringBuilder();

                    sb.Append("INSERT INTO Contacts (FirstName, LastName, County, Postal, Email) VALUES ");

                    for (int i = 0; i < batchContacts.Count; i++)
                    {
                        var contact = batchContacts[i];
                        sb.Append(
                            $"(@FirstName{batchStart + i}, @LastName{batchStart + i}, @County{batchStart + i}, @Postal{batchStart + i}, @Email{batchStart + i}),");

                        insertCommand.Parameters.AddWithValue($"@FirstName{batchStart + i}", contact.FirstName);
                        insertCommand.Parameters.AddWithValue($"@LastName{batchStart + i}", contact.LastName);
                        insertCommand.Parameters.AddWithValue($"@County{batchStart + i}", contact.County);
                        insertCommand.Parameters.AddWithValue($"@Postal{batchStart + i}", contact.Postal);
                        insertCommand.Parameters.AddWithValue($"@Email{batchStart + i}", contact.Email);
                    }

                    sb.Length--;
                    sb.Append(";");

                    insertCommand.CommandText = sb.ToString();
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }
        _sqLiteConnection.Close();

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

    [Obsolete("GetAllContacts should not be used on large databases, refer to GetContactPage instead")]
    public List<Contact> GetAllContacts()
    {
        List<Contact> contacts = new List<Contact>();

        _sqLiteConnection.Open();
        SQLiteCommand cmd = new SQLiteCommand("SELECT Id, FirstName, LastName, County, Postal, Email FROM Contacts",
            _sqLiteConnection);
        SQLiteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Contact contact = new Contact
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                County = reader.GetString(3),
                Postal = reader.GetString(4),
                Email = reader.GetString(5),
            };
            contacts.Add(contact);
        }

        _sqLiteConnection.Close();

        return contacts;
    }

    public List<Contact> GetContactPage(int pageNumber, int pageSize)
    {
        List<Contact> contacts = new List<Contact>();

        try
        {
            _sqLiteConnection.Open();

            string query = @"
                SELECT Id, FirstName, LastName, County, Postal, Email
                FROM Contacts
                ORDER BY Id
                LIMIT @PageSize OFFSET @Offset;
            ";

            int offset = (pageNumber - 1) * pageSize;

            using (SQLiteCommand command = new SQLiteCommand(query, _sqLiteConnection))
            {
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.Parameters.AddWithValue("@Offset", offset);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contact contact = new Contact
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            County = reader.GetString(3),
                            Postal = reader.GetString(4),
                            Email = reader.GetString(5)
                        };
                        contacts.Add(contact);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            throw;
        }
        finally
        {
            _sqLiteConnection.Close();
        }

        return contacts;
    }

    public void UpdateContact(Contact contact)
    {
        string updateQuery = @"
        UPDATE Contacts
        SET FirstName = @FirstName,
            LastName = @LastName,
            Email = @Email,
            County = @County
        WHERE Id = @Id;
    ";
        _sqLiteConnection.Open();
        using (SQLiteCommand command = new SQLiteCommand(updateQuery, _sqLiteConnection))
        {
            command.Parameters.AddWithValue("@FirstName", contact.FirstName);
            command.Parameters.AddWithValue("@LastName", contact.LastName);
            command.Parameters.AddWithValue("@Email", contact.Email);
            command.Parameters.AddWithValue("@County", contact.County);
            command.Parameters.AddWithValue("@Id", contact.Id);

            command.ExecuteNonQuery();
        }

        _sqLiteConnection.Close();
    }
}

public static class DatabaseQueries
{
    public static CountDictionary CountDomains(Database database)
    {
        return database.QueryCount(
            @"
            SELECT substr(Email, instr(Email, '@') + 1) AS Domain, COUNT(*) AS Count
            FROM Contacts
            GROUP BY Domain
            ORDER BY Count DESC;
            ",
            "Domain"
        );
    }

    public static CountDictionary CountCounties(Database database)
    {
        return database.QueryCount(
            @"
            SELECT County AS County, COUNT(*) AS Count
            FROM Contacts
            GROUP BY County
            ORDER BY Count DESC;",
            "County"
        );
    }
}

public class Contact
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string County { get; set; }
    public string Postal { get; set; }

    public string Email { get; set; }
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