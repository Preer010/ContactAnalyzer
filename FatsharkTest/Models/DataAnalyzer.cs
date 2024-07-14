using System.Collections.Generic;
using FatsharkTest.Data;

namespace FatsharkTest.Models;

using CountDictionary = Dictionary<string, int>;

public class DataAnalyzer
{
    private Database _database;
    public CountDictionary Domains { get; private set; }
    public CountDictionary Counties { get; private set; }

    public DataAnalyzer(Database database)
    {
        _database = database;

        Domains = DatabaseQueries.CountDomains(_database);
        Counties = DatabaseQueries.CountCounties(_database);
    }
}

