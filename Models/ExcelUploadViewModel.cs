namespace SearchDataCollection.Models;
public class ExcelUploadViewModel
{
    public IFormFile File { get; set; }
     public List<ZipCodeDataMongo> ZipCodes { get; set; }
}