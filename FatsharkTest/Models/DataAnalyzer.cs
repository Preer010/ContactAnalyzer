using System.Collections.Generic;
using FatsharkTest.Data;

namespace FatsharkTest.Models;

public class DataAnalyzer
{
    private Database _database;
    private Dictionary<string, int> _domains;
    private Dictionary<string, int> _surnames;
    private Dictionary<string, int> _forenames;
    private Dictionary<string, int> _counties;

    public Dictionary<string, int> Domains => _domains;
    public Dictionary<string, int> Surnames => _surnames;
    public Dictionary<string, int> ForeNames => _forenames;
    
    public Dictionary<string, int> Counties => _counties;
    public DataAnalyzer(Database database)
    {
        _database = database;
        
        string domainQuery = @"
                    SELECT substr(Email, instr(Email, '@') + 1) AS Domain, COUNT(*) AS Count
                    FROM Contacts
                    GROUP BY Domain
                    ORDER BY Count DESC;";
        _domains = _database.QueryCount(domainQuery, "Domain");

        string surnameQuery = @"
            SELECT LastName AS Surname, COUNT(*) AS Count
            FROM Contacts
            GROUP BY Surname
            ORDER BY Count DESC;";

        _surnames = _database.QueryCount(surnameQuery, "Surname");

        string forenameQuery = @"
            SELECT FirstName AS ForeName, COUNT(*) AS Count
            FROM Contacts
            GROUP BY ForeName
            ORDER BY Count DESC;";

        _forenames = _database.QueryCount(forenameQuery, "ForeName");        
        
        string countryQuery = @"
            SELECT County AS County, COUNT(*) AS Count
            FROM Contacts
            GROUP BY County
            ORDER BY Count DESC;";

        _counties = _database.QueryCount(countryQuery, "County");
    }
    
}