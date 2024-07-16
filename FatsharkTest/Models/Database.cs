using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FatsharkTest.Utils;

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
                if (_sqLiteConnection.State != ConnectionState.Open)
                {
                    _sqLiteConnection.Open();
                }

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

                    if (_sqLiteConnection.State != ConnectionState.Closed)
                    {
                        _sqLiteConnection.Close();
                    }

                    ImportCsvData("uk-500.csv");
                }
                else
                {
                    if (_sqLiteConnection.State != ConnectionState.Closed)
                    {
                        _sqLiteConnection.Close();
                    }

                    if (GetTableCount("Contacts") == 0)
                    {
                        ImportCsvData("uk-500.csv");
                    }
                }
            }
            {
                if (isNewDatabase)
                {
                    if (_sqLiteConnection.State != ConnectionState.Open)
                    {
                        _sqLiteConnection.Open();
                    }

                    string dropTableQuery = "DROP TABLE IF EXISTS GeoLocation;";
                    string createGeoQuery = @"
                    CREATE TABLE GeoLocation (
	                    Postal	        TEXT,
	                    Latitude	    DOUBLE PRECISION,
	                    Longitude	    DOUBLE PRECISION,
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

                    if (_sqLiteConnection.State != ConnectionState.Closed)
                    {
                        _sqLiteConnection.Close();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR: {e.Message}");
            throw;
        }
    }

    public int GetTableCount(string table)
    {
        string checkEmptyTableQuery = $"SELECT COUNT(*) FROM {table};";

        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

        using (SQLiteCommand command = new SQLiteCommand(checkEmptyTableQuery, _sqLiteConnection))
        {
            int count = Convert.ToInt32(command.ExecuteScalar());
            if (_sqLiteConnection.State != ConnectionState.Closed)
            {
                _sqLiteConnection.Close();
            }

            return count;
        }
    }

    private void ImportCsvData(string csvFile)
    {
        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

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

        if (_sqLiteConnection.State != ConnectionState.Closed)
        {
            _sqLiteConnection.Close();
        }
    }

    public Dictionary<string, int> QueryCount(string sqlQuery, string queryType)
    {
        Dictionary<string, int> dataCounts = new Dictionary<string, int>();

        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

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

        if (_sqLiteConnection.State != ConnectionState.Closed)
        {
            _sqLiteConnection.Close();
        }

        return dataCounts;
    }

    [Obsolete("GetAllContacts should not be used on large databases, refer to GetContactPage instead")]
    public List<Contact> GetAllContacts()
    {
        List<Contact> contacts = new List<Contact>();

        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

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

        if (_sqLiteConnection.State != ConnectionState.Closed)
        {
            _sqLiteConnection.Close();
        }

        return contacts;
    }

    public List<Contact> GetContactPage(int pageNumber, int pageSize)
    {
        List<Contact> contacts = new List<Contact>();

        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

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


        if (_sqLiteConnection.State != ConnectionState.Closed)
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
        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

        using (SQLiteCommand command = new SQLiteCommand(updateQuery, _sqLiteConnection))
        {
            command.Parameters.AddWithValue("@FirstName", contact.FirstName);
            command.Parameters.AddWithValue("@LastName", contact.LastName);
            command.Parameters.AddWithValue("@Email", contact.Email);
            command.Parameters.AddWithValue("@County", contact.County);
            command.Parameters.AddWithValue("@Id", contact.Id);

            command.ExecuteNonQuery();
        }

        if (_sqLiteConnection.State != ConnectionState.Closed)
        {
            _sqLiteConnection.Close();
        }
    }

    public void ImportGeoData(List<GeoPoint> geoPoints)
    {
        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

        StringBuilder sb = new StringBuilder();
        List<SQLiteParameter> parameters = new List<SQLiteParameter>();

        sb.Append("INSERT INTO GeoLocation (Postal, Latitude, Longitude) VALUES ");

        for (int i = 0; i < geoPoints.Count; i++)
        {
            GeoPoint geoPoint = geoPoints[i];
            string postalParamName = $"@Postal{i}";
            string latitudeParamName = $"@Latitude{i}";
            string longitudeParamName = $"@Longitude{i}";

            sb.Append($"({postalParamName}, {latitudeParamName}, {longitudeParamName}),");

            parameters.Add(new SQLiteParameter(postalParamName, geoPoint.Postal));
            parameters.Add(new SQLiteParameter(latitudeParamName, geoPoint.Latitude));
            parameters.Add(new SQLiteParameter(longitudeParamName, geoPoint.Longitude));
        }

        sb.Length--; // Remove the last comma
        sb.Append(";");

        string commandText = sb.ToString();


        using (SQLiteCommand insertCommand = new SQLiteCommand(commandText, _sqLiteConnection))
        {
            insertCommand.Parameters.AddRange(parameters.ToArray());
            insertCommand.ExecuteNonQuery();
        }

        if (_sqLiteConnection.State != ConnectionState.Closed)
        {
            _sqLiteConnection.Close();
        }
    }

    // TODO: Fix a page like getter for the geodata
    [Obsolete("Would ideally not be use on a huge database")]
    public List<GeoPoint> GetGeoData()
    {
        List<GeoPoint> geoPoints = new List<GeoPoint>();

        if (_sqLiteConnection.State != ConnectionState.Open)
        {
            _sqLiteConnection.Open();
        }

        SQLiteCommand cmd = new SQLiteCommand("SELECT Postal, Latitude, Longitude FROM GeoLocation", _sqLiteConnection);
        SQLiteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            GeoPoint geoPoint = new GeoPoint(reader.GetDouble(1), reader.GetDouble(2), reader.GetString(0));
            geoPoints.Add(geoPoint);
        }

        if (_sqLiteConnection.State != ConnectionState.Closed)
        {
            _sqLiteConnection.Close();
        }

        return geoPoints;
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