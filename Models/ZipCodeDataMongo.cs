using MongoDB.Bson;

namespace SearchDataCollection.Models;

public class ZipCodeDataMongo: ZipCodeData{
   public ObjectId _id {get; set;}
   public ZipCodeDataMongo(): base() {
    _id = new ObjectId();
   }
}
