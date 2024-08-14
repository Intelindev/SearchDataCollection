namespace SearchDataCollection.Models;
public class Filter {
    public Filter(){
        PersonalZip = string.Empty;
        County = string.Empty;
        SearchTerm = string.Empty;
    }
    public string PersonalZip { get; set; } = string.Empty;
    public string County { get; set; }= string.Empty;
    public string SearchTerm { get; set; }= string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? limit { get; set; } = 0; // Default page number
    public int? offset { get; set; } = 0; 
    public List<string>? listOfSearchTerms  { get; set; }
}