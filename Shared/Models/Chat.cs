using Shared.Models.MessageModel;
using Shared.Models.UserModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models.ChatModel;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Name { get; set; }
    public List<Message> Messages { get; set; }
    public List<User> Members { get; set; }
    public User Creator { get; set; }

    public Chat(string name, User creator)
    {
        Name = name;
        Messages = new List<Message>();
        Members = new List<User> { creator };
        Creator = creator;
    }
}
