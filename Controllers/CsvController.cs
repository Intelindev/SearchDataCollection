using Microsoft.AspNetCore.Mvc;
using System.IO;
using CsvHelper;
using System.Globalization;
using SearchDataCollection.Models;
using SearchDataCollection.Service;
using CsvHelper.Configuration;
using System.Text;

namespace SearchDataCollection.Controllers
{
    [ApiController]
    [Route("Cvs")]
    public class CsvController : Controller
    {
        private  string tempFolderPath;
        private readonly MongoService _mongoDbService;
        private readonly Dictionary<string, ZipCodeData> localCache = new();

        public CsvController(MongoService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "TempCsvFiles");
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> IndexAsync()
        {
            return View(new Filter()
            {
                listOfSearchTerms = await _mongoDbService.GetAllDistincSearchTerms()
            });
        }

        [HttpGet("GetSearchTerms")]
        public async Task<IActionResult> GetSearchTerms(int limit, int offset, string search = "", string personalZip = "", string county = "", string searchTerm = "", DateOnly? dateStart = null, DateOnly? dateEnd = null)
        {

            var filtro = new Filter()
            {
                PersonalZip = personalZip,
                County = county,
                SearchTerm = searchTerm,
                StartDate = dateStart,
                EndDate = dateEnd,
                limit = limit,
                offset = offset
            };

            var searchTerms = await _mongoDbService.GetSearchCodeFromDatabase(filtro);

            return Ok(searchTerms);
        }

        [HttpGet("DownloadCsv")]
        public async Task<IActionResult> DownloadCsv(string personalZip = "", string county = "", string searchTerm = "", DateOnly? dateStart = null, DateOnly? dateEnd = null)
        {
            var filtro = new Filter()
            {
                PersonalZip = personalZip,
                County = county,
                SearchTerm = searchTerm,
                StartDate = dateStart,
                EndDate = dateEnd,
                limit = 0,
                offset = 0
            };

            var searchTerms = await _mongoDbService.GetSearchCodeFromDatabase(filtro);

            // Generar un nombre único para el archivo temporal
            ClearTempFolder();

            // Generar un nombre único para el archivo temporal
            var fileName = Path.Combine(tempFolderPath, Path.GetRandomFileName() + ".csv");
            
            using (var streamWriter = new StreamWriter(fileName))
            using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<SearchTerm>();
                csv.NextRecord();
                foreach (var data in searchTerms.rows)
                {
                    csv.WriteRecord<SearchTerm>(data);
                    csv.NextRecord();
                }
            }

            // Proporcionar un enlace de descarga
            var file = new PhysicalFileResult(fileName, "text/csv");
            file.FileDownloadName = "searchTerms.csv"; // Nombre del archivo para la descarga

            // Eliminar el archivo temporal después de la descarga
            //System.IO.File.Delete(tempFileName);

            return file;
        }

        private void ClearTempFolder()
        {
            if (Directory.Exists(tempFolderPath))
            {
                // Elimina todos los archivos dentro de la carpeta
                foreach (var file in Directory.EnumerateFiles(tempFolderPath))
                {
                    System.IO.File.Delete(file);
                }
            }
            else
            {
                // Crea la carpeta si no existe
                Directory.CreateDirectory(tempFolderPath);
            }
        }


        [HttpGet]
        [Route("UploadCvsFile")]
        public IActionResult UploadCvsFile()
        {
            return View();
        }

        [HttpPost]
        [Route("UploadCvsFile")]
        public async Task<IActionResult> UploadCvsFile(CsvForm csvForm)
        {
            if (csvForm.file == null || csvForm.file.Length == 0)
            {
                return BadRequest("No se ha seleccionado ningún archivo.");
            }

            if (Path.GetExtension(csvForm.file.FileName) != ".csv")
            {
                return BadRequest("El archivo debe tener extensión .csv");
            }
            int count = 0;
            DateTime today = DateTime.Today; // Obtiene la fecha de hoy sin hora
            DateOnly dateOnly = DateOnly.FromDateTime(today);
            try
            {
                using (var reader = new StreamReader(csvForm.file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)

                {
                    HasHeaderRecord = true,
                    Delimiter = ","
                }))
                {
                    List<SearchTerm> searchDataList = new List<SearchTerm>();
                    int batchSize = 1000;

                    while (csv.Read())
                    {
                        var record = csv.GetRecord<CsvRow>();
                        //obtener el county
                        var searchTerm = await MapToSearchTermAsync(record, dateOnly, csvForm.SearchTerm);

                        var existsRecord = await _mongoDbService.existsCvsRecord(searchTerm);

                        if (!existsRecord)
                            searchDataList.Add(searchTerm);
                        else
                            Console.WriteLine($"Duplicado linea {count}");

                        if (searchDataList.Count == batchSize)
                        {
                            await _mongoDbService.CreateCsvListAsync(searchDataList);
                            searchDataList.Clear();
                        }
                        // Procesar el registro
                        //await _mongoDbService.CreateZipCodeAsync(record);
                        count++;
                        if (count == 2586)
                            count = count;
                    }

                    if (searchDataList.Count > 0)
                    {
                        await _mongoDbService.CreateCsvListAsync(searchDataList);
                    }

                    //await _mongoDbService.CreateZipCodeListAsync(records);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al importar el archivo: {ex.Message} en el registro numero {count}");
            }
        }

        private async Task<ZipCodeData?> GetZipCodeInfoAsync(string zipCode)
        {

            if (zipCode == "")
                return null;
            // Buscar en cache local primero
            if (localCache.TryGetValue(zipCode, out var cachedInfo))
            {
                return cachedInfo;
            }

            // Realizar la consulta a MongoDB
            var result = await _mongoDbService.GetZipCodeDataAsync(zipCode);

            if (result == null)
            {
                return null;
            }
            // Almacenar en cache local
            localCache.Add(zipCode, result);

            return result;
        }
        public async Task<SearchTerm> MapToSearchTermAsync(CsvRow csvRow, DateOnly Year, string Search_Term)
        {
            var zipCodeData = await GetZipCodeInfoAsync(csvRow.PERSONAL_ZIP);

            return new SearchTerm
            {
                Year = Year,
                Search_Term = Search_Term,
                FIRST_NAME = csvRow.FIRST_NAME,
                LAST_NAME = csvRow.LAST_NAME,
                DIRECT_NUMBER = csvRow.DIRECT_NUMBER,
                MOBILE_PHONE = csvRow.MOBILE_PHONE,
                PERSONAL_ADDRESS = csvRow.PERSONAL_ADDRESS,
                PERSONAL_CITY = csvRow.PERSONAL_CITY,
                PERSONAL_PHONE = csvRow.PERSONAL_PHONE,
                PERSONAL_STATE = csvRow.PERSONAL_STATE,
                PERSONAL_ZIP = csvRow.PERSONAL_ZIP,
                PERSONAL_ZIP4 = csvRow.PERSONAL_ZIP4,
                SOCIAL_CONNECTIONS = csvRow.SOCIAL_CONNECTIONS,
                AGE_RANGE = csvRow.AGE_RANGE,
                CHILDREN = csvRow.CHILDREN,
                GENDER = csvRow.GENDER,
                HOMEOWNER = csvRow.HOMEOWNER,
                MARRIED = csvRow.MARRIED,
                NET_WORTH = csvRow.NET_WORTH,
                INCOME_RANGE = csvRow.INCOME_RANGE,
                BUSINESS_EMAIL = csvRow.BUSINESS_EMAIL,
                BUSINESS_EMAIL_VALIDATION_STATUS = csvRow.BUSINESS_EMAIL_VALIDATION_STATUS,
                PROGRAMMATIC_BUSINESS_EMAILS = csvRow.PROGRAMMATIC_BUSINESS_EMAILS,
                BUSINESS_EMAIL_LAST_SEEN = csvRow.BUSINESS_EMAIL_LAST_SEEN,
                PERSONAL_EMAIL = csvRow.PERSONAL_EMAIL,
                ADDITIONAL_PERSONAL_EMAILS = csvRow.ADDITIONAL_PERSONAL_EMAILS,
                PERSONAL_EMAIL_VALIDATION_STATUS = csvRow.PERSONAL_EMAIL_VALIDATION_STATUS,
                PERSONAL_EMAIL_LAST_SEEN = csvRow.PERSONAL_EMAIL_LAST_SEEN,
                SHA256_PERSONAL_EMAIL = csvRow.SHA256_PERSONAL_EMAIL,
                SHA256_BUSINESS_EMAIL = csvRow.SHA256_BUSINESS_EMAIL,
                LAST_UPDATED = csvRow.LAST_UPDATED,
                COMPANY_ADDRESS = csvRow.COMPANY_ADDRESS,
                COMPANY_DESCRIPTION = csvRow.COMPANY_DESCRIPTION,
                COMPANY_DOMAIN = csvRow.COMPANY_DOMAIN,
                COMPANY_EMPLOYEE_COUNT = csvRow.COMPANY_EMPLOYEE_COUNT,
                COMPANY_LINKEDIN_URL = csvRow.COMPANY_LINKEDIN_URL,
                COMPANY_NAME = csvRow.COMPANY_NAME,
                COMPANY_PHONE = csvRow.COMPANY_PHONE,
                COMPANY_REVENUE = csvRow.COMPANY_REVENUE,
                COMPANY_SIC = csvRow.COMPANY_SIC,
                COMPANY_NAICS = csvRow.COMPANY_NAICS,
                COMPANY_CITY = csvRow.COMPANY_CITY,
                COMPANY_STATE = csvRow.COMPANY_STATE,
                COMPANY_ZIP = csvRow.COMPANY_ZIP,
                COMPANY_INDUSTRY = csvRow.COMPANY_INDUSTRY,
                COMPANY_LAST_UPDATED = csvRow.COMPANY_LAST_UPDATED,
                DEPARTMENT = csvRow.DEPARTMENT,
                JOB_TITLE = csvRow.JOB_TITLE,
                LINKEDIN_URL = csvRow.LINKEDIN_URL,
                PROFESSIONAL_ADDRESS = csvRow.PROFESSIONAL_ADDRESS,
                PROFESSIONAL_ADDRESS_2 = csvRow.PROFESSIONAL_ADDRESS_2,
                PROFESSIONAL_CITY = csvRow.PROFESSIONAL_CITY,
                PROFESSIONAL_STATE = csvRow.PROFESSIONAL_STATE,
                PROFESSIONAL_ZIP = csvRow.PROFESSIONAL_ZIP,
                PROFESSIONAL_ZIP4 = csvRow.PROFESSIONAL_ZIP4,
                SENIORITY_LEVEL = csvRow.SENIORITY_LEVEL,
                JOB_TITLE_LAST_UPDATED = csvRow.JOB_TITLE_LAST_UPDATED,
                SKIPTRACE_MATCH_BY = csvRow.SKIPTRACE_MATCH_BY,
                SKIPTRACE_HASH = csvRow.SKIPTRACE_HASH,
                SKIPTRACE_FIRST_NAME = csvRow.SKIPTRACE_FIRST_NAME,
                SKIPTRACE_LAST_NAME = csvRow.SKIPTRACE_LAST_NAME,
                SKIPTRACE_ADDRESS = csvRow.SKIPTRACE_ADDRESS,
                SKIPTRACE_CITY = csvRow.SKIPTRACE_CITY,
                SKIPTRACE_STATE = csvRow.SKIPTRACE_STATE,
                SKIPTRACE_ZIP = csvRow.SKIPTRACE_ZIP,
                SKIPTRACE_LANDLINE_NUMBERS = csvRow.SKIPTRACE_LANDLINE_NUMBERS,
                SKIPTRACE_WIRELESS_NUMBERS = csvRow.SKIPTRACE_WIRELESS_NUMBERS,
                ZipCode = zipCodeData != null ? zipCodeData.ZipCode : "",
                PrimaryCBSA = zipCodeData != null ? zipCodeData.PrimaryCBSA : "",
                PrimaryCBSAName = zipCodeData != null ? zipCodeData.PrimaryCBSAName : "",
                CBSAType = zipCodeData != null ? zipCodeData.CBSAType : "",
                PrimaryCSA = zipCodeData != null ? zipCodeData.PrimaryCSA : "",
                PrimaryCSAName = zipCodeData != null ? zipCodeData.PrimaryCSAName : "",
                City = zipCodeData != null ? zipCodeData.City : "",
                State = zipCodeData != null ? zipCodeData.State : "",
                CBSAPrimaryCity = zipCodeData != null ? zipCodeData.CBSAPrimaryCity : "",
                PrimaryCountyName = zipCodeData != null ? zipCodeData.PrimaryCountyName : "",
                PrimaryCountyFIPSID = zipCodeData != null ? zipCodeData.PrimaryCountyFIPSID : "",
                ZCTAPopulation = zipCodeData != null ? zipCodeData.ZCTAPopulation : "",
                WithinMultipleCBSAs = zipCodeData != null ? zipCodeData.WithinMultipleCBSAs : "",
                SecondaryCBSA1 = zipCodeData != null ? zipCodeData.SecondaryCBSA1 : "",
                SecondaryCBSA2 = zipCodeData != null ? zipCodeData.SecondaryCBSA2 : "",
                SecondaryCBSA1Name = zipCodeData != null ? zipCodeData.SecondaryCBSA1Name : "",
                SecondaryCBSA2Name = zipCodeData != null ? zipCodeData.SecondaryCBSA2Name : ""
            };
        }
    }
}
