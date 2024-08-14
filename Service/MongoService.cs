using MongoDB.Bson;
using MongoDB.Driver;
using SearchDataCollection.Models;

namespace SearchDataCollection.Service;
public class MongoService
{
    private IMongoCollection<ZipCodeData> _zipCode;
    private IMongoCollection<ZipCodeDataMongo> _zipCodeMongo;
    private IMongoCollection<SearchTerm> _csvCode;
    private IMongoCollection<SearchTermMongo> _searchTermMongo;

    private MongoClient _client;

    public MongoService(IConfiguration config)
    {
        _client = new MongoClient(config.GetConnectionString("MongoDb"));
    }

    public async Task CreateZipCodeListAsync(List<ZipCodeData> zipcodes)
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _zipCode = database.GetCollection<ZipCodeData>("ZipCodes");
        await _zipCode.InsertManyAsync(zipcodes);
    }

    public async Task<List<ZipCodeDataMongo>> GetAllZipCodes(int offset = 0, int limit = 0)
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _zipCodeMongo = database.GetCollection<ZipCodeDataMongo>("ZipCodes");
        var filter = Builders<ZipCodeDataMongo>.Filter.Empty;
        var zipCodes = await _zipCodeMongo.Find(filter).Skip(offset).Limit(limit).ToListAsync();
        return zipCodes;
    }

    public async Task<bool> existsCvsRecord(SearchTerm cvs)
    {

        var database = _client.GetDatabase("ExcelDataDb");
        _searchTermMongo = database.GetCollection<SearchTermMongo>("Csv");

        var filter = Builders<SearchTermMongo>.Filter.And (
            Builders<SearchTermMongo>.Filter.Eq(x => x.Search_Term, cvs.Search_Term),
            Builders<SearchTermMongo>.Filter.Eq(x => x.Year.Year, cvs.Year.Year),
            Builders<SearchTermMongo>.Filter.Eq(x => x.Year.Month, cvs.Year.Month),
            Builders<SearchTermMongo>.Filter.Eq(x => x.Year.Day, cvs.Year.Day),
            Builders<SearchTermMongo>.Filter.Eq(x => x.PERSONAL_EMAIL, cvs.PERSONAL_EMAIL),
            Builders<SearchTermMongo>.Filter.Eq(x => x.FIRST_NAME, cvs.FIRST_NAME),
            Builders<SearchTermMongo>.Filter.Eq(x => x.LAST_NAME, cvs.LAST_NAME),
            Builders<SearchTermMongo>.Filter.Eq(x => x.PERSONAL_ZIP, cvs.PERSONAL_ZIP),
            Builders<SearchTermMongo>.Filter.Eq(x => x.MOBILE_PHONE, cvs.MOBILE_PHONE)
        );

        var result = await _searchTermMongo.Find(filter).FirstOrDefaultAsync();

        if (result != null)
        {
            return true;
        }
        return false;
    }
    public async Task CreateCsvListAsync(List<SearchTerm> searchTerm)
    {
        var database = _client.GetDatabase("ExcelDataDb");
        var collection = database.GetCollection<SearchTerm>("Csv");

        // Crear el índice único compuesto
        var indexKeys = Builders<SearchTerm>.IndexKeys
            .Ascending(x => x.Search_Term)
            .Ascending(x => x.Year)
            .Ascending(x => x.PERSONAL_EMAIL)
            .Ascending(x => x.FIRST_NAME)
            .Ascending(x => x.LAST_NAME)
            .Ascending(x => x.PERSONAL_ZIP)
            .Ascending(x => x.MOBILE_PHONE);

        var options = new CreateIndexOptions { Unique = true };
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<SearchTerm>(indexKeys, options));

        // Insertar los documentos
        try
        {
            await collection.InsertManyAsync(searchTerm);
        }
        catch (MongoWriteException ex)
        {
            // Manejar la excepción de duplicados
            if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Loggear o tomar alguna acción específica cuando se encuentra un duplicado
                Console.WriteLine("Se encontró un duplicado al insertar el documento.");
            }
            else
            {
                throw; // Re-lanzar la excepción si no es por duplicado
            }
        }
    }

    internal async Task<List<SearchTermMongo>> GetAllSearchTerms()
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _searchTermMongo = database.GetCollection<SearchTermMongo>("Csv");
        var filter = Builders<SearchTermMongo>.Filter.Empty;
        var searchTerms = await _searchTermMongo.Find(filter).ToListAsync();
        return searchTerms;
    }

    public async Task<ZipCodeData> GetZipCodeDataAsync(string zipCode)
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _zipCodeMongo = database.GetCollection<ZipCodeDataMongo>("ZipCodes");

        var filter = Builders<ZipCodeDataMongo>.Filter.Eq(doc => doc.ZipCode, zipCode);

        // Ejecutar la consulta y obtener el primer resultado
        var result = await _zipCodeMongo.Find(filter).FirstOrDefaultAsync();

        if (result == null)
            return new ZipCodeData();

        return MapToZipCodeData(result);
    }

    public static ZipCodeData MapToZipCodeData(ZipCodeDataMongo mongoData)
    {
        return new ZipCodeData
        {
            ZipCode = mongoData.ZipCode,
            PrimaryCBSA = mongoData.PrimaryCBSA,
            PrimaryCBSAName = mongoData.PrimaryCBSAName,
            CBSAType = mongoData.CBSAType,
            PrimaryCSA = mongoData.PrimaryCSA,
            PrimaryCSAName = mongoData.PrimaryCSAName,
            City = mongoData.City,
            State = mongoData.State,
            CBSAPrimaryCity = mongoData.CBSAPrimaryCity,
            PrimaryCountyName = mongoData.PrimaryCountyName,
            PrimaryCountyFIPSID = mongoData.PrimaryCountyFIPSID,
            ZCTAPopulation = mongoData.ZCTAPopulation,
            WithinMultipleCBSAs = mongoData.WithinMultipleCBSAs,
            SecondaryCBSA1 = mongoData.SecondaryCBSA1,
            SecondaryCBSA2 = mongoData.SecondaryCBSA2,
            SecondaryCBSA1Name = mongoData.SecondaryCBSA1Name,
            SecondaryCBSA2Name = mongoData.SecondaryCBSA2Name
        };
    }

    public async Task<Response> GetSearchCodeFromDatabase(Filter filter)
    {
        var filterBuilder = Builders<SearchTermMongo>.Filter;
        var filterExpression = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(filter.PersonalZip))
        {
            filterExpression = filterBuilder.Regex(x => x.PERSONAL_ZIP, new BsonRegularExpression(filter.PersonalZip, "i"));
        }

        if (!string.IsNullOrEmpty(filter.County))
        {
            if (filterExpression != filterBuilder.Empty)
                filterExpression |= filterBuilder.Regex(x => x.PrimaryCountyName, new BsonRegularExpression(filter.County, "i"));

            else
                filterExpression = filterBuilder.Regex(x => x.PrimaryCountyName, new BsonRegularExpression(filter.County, "i"));
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            if (filterExpression != filterBuilder.Empty)
                filterExpression |= filterBuilder.Regex(x => x.Search_Term, new BsonRegularExpression(filter.SearchTerm, "i")); // Case-insensitive search
            else
                filterExpression = filterBuilder.Regex(x => x.Search_Term, new BsonRegularExpression(filter.SearchTerm, "i")); // Case-insensitive search
        }

        if (filter.StartDate.HasValue)
        {
            filterExpression = filterBuilder.And(filterExpression, filterBuilder.Gte(x => x.Year, filter.StartDate.Value));
        }

        if (filter.EndDate.HasValue)
        {
            filterExpression = filterBuilder.And(filterExpression, filterBuilder.Lte(x => x.Year, filter.EndDate.Value));
        }

        var database = _client.GetDatabase("ExcelDataDb");
        _searchTermMongo = database.GetCollection<SearchTermMongo>("Csv");
        var totalRows = _searchTermMongo.CountDocuments(filterExpression);
        var searchTerms = await _searchTermMongo.Find(filterExpression).Skip(filter.offset).Limit(filter.limit).ToListAsync();
        return new Response()
        {
            total = totalRows,
            rows = searchTerms
        };
    }

    internal async Task<long> GetCountSearchTerms()
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _searchTermMongo = database.GetCollection<SearchTermMongo>("Csv");
        var filter = Builders<SearchTermMongo>.Filter.Empty;
        var searchTerms = await _searchTermMongo.CountDocumentsAsync(filter);
        return searchTerms;
    }

    internal async Task<long> GetCountZipCode()
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _zipCodeMongo = database.GetCollection<ZipCodeDataMongo>("ZipCode");
        var filter = Builders<ZipCodeDataMongo>.Filter.Empty;
        var count = await _zipCodeMongo.CountDocumentsAsync(filter);

        return count;
    }

    internal async Task<List<string>> GetAllDistincSearchTerms()
    {
        var database = _client.GetDatabase("ExcelDataDb");
        _searchTermMongo = database.GetCollection<SearchTermMongo>("Csv");
        var filter = Builders<SearchTermMongo>.Filter.Empty;
        var searchTerms = await _searchTermMongo.DistinctAsync(x => x.Search_Term, new BsonDocument());
        return searchTerms.ToList();
    }
}
