using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace FomoDog.Context.MongoDB.Models
{
    public class RawMessageToBsonDocumentSerializer : IBsonSerializer
    {
        public Type ValueType => typeof(BsonDocument);

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType == BsonType.String)
            {
                var value = context.Reader.ReadString();
                return BsonDocument.Parse(value);
            }
            else
            {
                return BsonSerializer.Deserialize<BsonDocument>(context.Reader);
            }
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            RawBsonDocument bson = new RawBsonDocument(value.ToBson());
            context.Writer.WriteRawBsonDocument(bson.Slice);
        }
    }
}