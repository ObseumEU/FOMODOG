using FomoDog.Context.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Riok.Mapperly.Abstractions;

namespace FomoDog.Context.MongoDB.Models
{
    public class Activity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonElement("chat")]
        public string ChatId { get; init; }
        [BsonElement("from")]
        public string From { get; init; }
        [BsonElement("content")]
        public string Content { get; init; }
        [BsonElement("date")]
        public DateTime Date { get; init; }
        [BsonElement("raw_message")]

        [BsonSerializer(typeof(RawMessageToBsonDocumentSerializer))]
        public BsonDocument RawMessage { get; init; }

        public override string ToString()
        {
            return $"From:{From} Date:{Date.ToString("yyyy MMMM d hh:mm:ss")} Content:{Content}";
        }
    }

    public class ActivityMapper
    {
        public Activity ChatActivityToActivity(ChatActivity activity)
        {
            if (activity == null)
                return default;
            var target = new Activity()
            {
                ChatId = activity.ChatId,
                From = activity.From,
                Content = activity.Content,
                Date = activity.Date,
                RawMessage = BsonDocument.Parse(activity.RawMessage),
            };
            return target;
        }

        public ChatActivity ActivityToChatActivity(Activity activity)
        {
            if (activity == null)
                return default;
            var target = new ChatActivity();
            target.ChatId = activity.ChatId;
            target.From = activity.From;
            target.Content = activity.Content;
            target.Date = activity.Date;
            target.RawMessage = activity.RawMessage.ToJson();
            return target;
        }
    }
}