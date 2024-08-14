
namespace SearchDataCollection.Models;
public class CsvListViewModel
{
    public CsvListViewModel()
    {
    }
    public List<SearchTermMongo>? searchTermsMongo { get; set; }
    public Filter filter{ get; set; }
}