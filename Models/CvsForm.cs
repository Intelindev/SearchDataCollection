using CsvHelper.Configuration.Attributes;

namespace SearchDataCollection.Models;
public class CsvForm
{
    public CsvForm(){}
    public IFormFile file { get; set; }
    public string SearchTerm { get; set; }
}
