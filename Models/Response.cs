namespace SearchDataCollection.Models;
public class Response {
    
    public long total { get; set; }
    public List<SearchTermMongo> rows { get; set; }
    
}