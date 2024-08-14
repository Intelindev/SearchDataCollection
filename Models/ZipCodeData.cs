using MongoDB.Bson;

namespace SearchDataCollection.Models;

public class ZipCodeData
{
    public ZipCodeData()
    {
        ZipCode = string.Empty;
        PrimaryCBSA = string.Empty;
        PrimaryCBSAName = string.Empty;
        CBSAType = string.Empty;
        PrimaryCSA = string.Empty;
        PrimaryCSAName = string.Empty;
        City = string.Empty;
        State = string.Empty;
        CBSAPrimaryCity = string.Empty;
        PrimaryCountyName = string.Empty;
        PrimaryCountyFIPSID = string.Empty;
        ZCTAPopulation = string.Empty;
        WithinMultipleCBSAs = string.Empty;; // Inicializando un booleano a false
        SecondaryCBSA1 = string.Empty;
        SecondaryCBSA2 = string.Empty;
        SecondaryCBSA1Name = string.Empty;
        SecondaryCBSA2Name = string.Empty;
    }
    public string ZipCode { get; set; }
    public string PrimaryCBSA { get; set; }
    public string PrimaryCBSAName { get; set; }
    public string CBSAType { get; set; }
    public string PrimaryCSA { get; set; }
    public string PrimaryCSAName { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string CBSAPrimaryCity { get; set; }
    public string PrimaryCountyName { get; set; }
    public string PrimaryCountyFIPSID { get; set; }
    public string ZCTAPopulation { get; set; }
    public string WithinMultipleCBSAs { get; set; } // Asumiendo que este campo es un booleano
    public string SecondaryCBSA1 { get; set; }
    public string SecondaryCBSA2 { get; set; }
    public string SecondaryCBSA1Name { get; set; }
    public string SecondaryCBSA2Name { get; set; }
}
