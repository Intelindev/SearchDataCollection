using System.Data;
using System.Diagnostics;
using ExcelDataReader;

using Microsoft.AspNetCore.Mvc;
using SearchDataCollection.Models;
using SearchDataCollection.Service;
using System.Text;

namespace SearchDataCollection.Controllers;

[ApiController]
[Route("[controller]")]
public class ZipCodeController : Controller
{
    private readonly MongoService _mongoDbService;

    public ZipCodeController(MongoService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index()
    {
        

        var viewModel = new ExcelUploadViewModel
        {
            ZipCodes = null
        };

        return View(viewModel);
    }

    [HttpGet]
    [Route("GetZipCodes")]
    public async Task<IActionResult> GetZipCodes(int offset, int limit)
    {
        // Lógica para obtener los datos de la base de datos con paginación
        var zipCodes = await GetZipCodesFromDatabase(offset, limit); // Implementar este método
        //var total = await _mongoDbService.GetCountZipCode();
        // Crear un objeto de respuesta para Bootstrap Table
        var result = new
        {
            total = 39474,
            rows = zipCodes
        };

        return Json(result);
    }


    [HttpGet]
    [Route("UploadZipCodeFile")]
    public IActionResult UploadZipCodeFile()
    {
        return View();
    }

    [HttpPost]
    [Route("UploadZipCodeFile")]
    public async Task<IActionResult> UploadZipCodeFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha seleccionado ningún archivo.");
        }

        if (Path.GetExtension(file.FileName) != ".xlsx")
        {
            return BadRequest("Solo se permiten archivos XLSX.");
        }

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var stream = file.OpenReadStream())
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {

            // Obtener los datos de la segunda hoja
            var dataSet = reader.AsDataSet();
            var dataTable = dataSet.Tables[1];

            int count = 0;

            List<ZipCodeData> zipCodeDataList = new List<ZipCodeData>();
            int batchSize = 1000; // Ajustar según rendimiento

            foreach (DataRow row in dataTable.Rows)
            {

                var zipCodeData = mapRowToZipCode(row);

                if (count != 0)
                    zipCodeDataList.Add(zipCodeData);
                count++;

                if (zipCodeDataList.Count == batchSize)
                {
                    await _mongoDbService.CreateZipCodeListAsync(zipCodeDataList);
                    zipCodeDataList.Clear();
                }
            }

            // Insertar los registros restantes
            if (zipCodeDataList.Count > 0)
            {
                await _mongoDbService.CreateZipCodeListAsync(zipCodeDataList);
            }
        }

        TempData["Mensaje"] = "Usuario creado exitosamente.";
        return RedirectToAction("Index");
    }

    private ZipCodeData mapRowToZipCode(DataRow row)
    {
        return new ZipCodeData
        {
            ZipCode = row[0].ToString(),
            PrimaryCBSA = row[1].ToString(),
            PrimaryCBSAName = row[2].ToString(),
            CBSAType = row[3].ToString(),
            PrimaryCSA = row[4].ToString(),
            PrimaryCSAName = row[5].ToString(),
            City = row[6].ToString(),
            State = row[7].ToString(),
            CBSAPrimaryCity = row[8].ToString(),
            PrimaryCountyName = row[9].ToString(),
            PrimaryCountyFIPSID = row[10].ToString(),
            ZCTAPopulation = row[11].ToString(),
            WithinMultipleCBSAs = row[12].ToString(),
            SecondaryCBSA1 = row[13].ToString(),
            SecondaryCBSA2 = row[14].ToString(),
            SecondaryCBSA1Name = row[15].ToString(),
            SecondaryCBSA2Name = row[16].ToString(),
        };
    }

    private async Task<List<ZipCodeDataMongo>> GetZipCodesFromDatabase(int offset = 0, int limit = 0)
    {
        var zipCodes = await _mongoDbService.GetAllZipCodes(offset, limit);
        return zipCodes; // Devuelve una lista de ZipCodeData
    }
}