namespace SearchDataCollection.Models;
public class SearchTerm
{
    public SearchTerm()
    {
        
    }
    public DateOnly Year {get; set;}
    public string Search_Term {get; set;}
    public string FIRST_NAME { get; set; }
    public string LAST_NAME { get; set; }
    public string DIRECT_NUMBER { get; set; }
    public string MOBILE_PHONE { get; set; }
    public string PERSONAL_ADDRESS { get; set; }
    public string PERSONAL_CITY { get; set; }
    public string PERSONAL_PHONE { get; set; }
    public string PERSONAL_STATE { get; set; }
    public string PERSONAL_ZIP { get; set; }
    public string PERSONAL_ZIP4 { get; set; }
    public string SOCIAL_CONNECTIONS { get; set; }
    public string AGE_RANGE { get; set; }
    public string CHILDREN { get; set; }
    public string GENDER { get; set; }
    public string HOMEOWNER { get; set; }
    public string MARRIED { get; set; }
    public string NET_WORTH { get; set; }
    public string INCOME_RANGE { get; set; }
    public string BUSINESS_EMAIL { get; set; }
    public string BUSINESS_EMAIL_VALIDATION_STATUS { get; set; }
    public string PROGRAMMATIC_BUSINESS_EMAILS { get; set; }
    public string BUSINESS_EMAIL_LAST_SEEN { get; set; }
    public string PERSONAL_EMAIL { get; set; }
    public string ADDITIONAL_PERSONAL_EMAILS { get; set; }
    public string PERSONAL_EMAIL_VALIDATION_STATUS { get; set; }
    public string PERSONAL_EMAIL_LAST_SEEN { get; set; }
    public string SHA256_PERSONAL_EMAIL { get; set; }
    public string SHA256_BUSINESS_EMAIL { get; set; }
    public string LAST_UPDATED { get; set; }
    public string COMPANY_ADDRESS { get; set; }
    public string COMPANY_DESCRIPTION { get; set; }
    public string COMPANY_DOMAIN { get; set; }
    public string COMPANY_EMPLOYEE_COUNT { get; set; }
    public string COMPANY_LINKEDIN_URL { get; set; }
    public string COMPANY_NAME { get; set; }
    public string COMPANY_PHONE { get; set; }
    public string COMPANY_REVENUE { get; set; }
    public string COMPANY_SIC { get; set; }
    public string COMPANY_NAICS { get; set; }
    public string COMPANY_CITY { get; set; }
    public string COMPANY_STATE { get; set; }
    public string COMPANY_ZIP { get; set; }
    public string COMPANY_INDUSTRY { get; set; }
    public string COMPANY_LAST_UPDATED { get; set; }
    public string DEPARTMENT { get; set; }
    public string JOB_TITLE { get; set; }
    public string LINKEDIN_URL { get; set; }
    public string PROFESSIONAL_ADDRESS { get; set; }
    public string PROFESSIONAL_ADDRESS_2 { get; set; }
    public string PROFESSIONAL_CITY { get; set; }
    public string PROFESSIONAL_STATE { get; set; }
    public string PROFESSIONAL_ZIP { get; set; }
    public string PROFESSIONAL_ZIP4 { get; set; }
    public string SENIORITY_LEVEL { get; set; }
    public string JOB_TITLE_LAST_UPDATED { get; set; }
    public string SKIPTRACE_MATCH_BY { get; set; }
    public string SKIPTRACE_HASH { get; set; }
    public string SKIPTRACE_FIRST_NAME { get; set; }
    public string SKIPTRACE_LAST_NAME { get; set; }
    public string SKIPTRACE_ADDRESS { get; set; }
    public string SKIPTRACE_CITY { get; set; }
    public string SKIPTRACE_STATE { get; set; }
    public string SKIPTRACE_ZIP { get; set; }
    public string SKIPTRACE_LANDLINE_NUMBERS { get; set; }
    public string SKIPTRACE_WIRELESS_NUMBERS { get; set; }
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