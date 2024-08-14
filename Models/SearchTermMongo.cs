using MongoDB.Bson;

namespace SearchDataCollection.Models;
public class SearchTermMongo: SearchTerm
{
    SearchTermMongo() : base(){
        
    }
    public ObjectId _id;
}
