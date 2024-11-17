using Shared.Models.UserModel;
using Shared.Models.ChatModel;
using Shared.Models.MessageModel;
using MongoDB.Driver;

namespace ChatApp.Server.Commands.DatabaseCommands;

public interface IDatabaseCommands
{
    private static IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        string connectionString = "mongodb://127.0.0.1:27017";
        string databaseName = "ChatApp";
        MongoClient client = new(connectionString);
        IMongoDatabase db = client.GetDatabase(databaseName);
        IMongoCollection<T> collection = db.GetCollection<T>(collectionName);
        return collection;
    }

    public static void CreateUser(User user)
    {
        IMongoCollection<User> collection = GetCollection<User>("Users");
        collection.InsertOneAsync(user);
    }

    public static List<User> GetUsers()
    {
        IMongoCollection<User> collection = GetCollection<User>("Users");
        IFindFluent<User, User> users = collection.Find(_ => true);
        return users.ToList();
    }

    public static User? GetUser(User user)
    {
        IMongoCollection<User> collection = GetCollection<User>("Users");
        FilterDefinition<User> filter = Builders<User>.Filter.Eq(u => u.Name, user.Name);
        filter &= Builders<User>.Filter.Eq(u => u.Password, user.Password);
        return collection.Find(filter).FirstOrDefault();
    }

    public static User? GetUserByName(string? name)
    {
        IMongoCollection<User> collection = GetCollection<User>("Users");
        FilterDefinition<User> filter = Builders<User>.Filter.Eq(u => u.Name, name);
        User? foundUser = collection.Find(filter).FirstOrDefault();
        return foundUser;
    }

    public static void CreateChat(Chat chat)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        collection.InsertOneAsync(chat);
    }

    public static void DeleteChat(Chat chat)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Id, chat.Id);
        Chat? foundChat = GetChat(chat);
        if (foundChat != null)
        {
            collection.DeleteOneAsync(filter);
            Console.WriteLine("Chat deleted!");
        }
        else
        {
            Console.WriteLine("No chat like that found");
        }
    }

    public static List<Chat> GetChats()
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        IFindFluent<Chat, Chat> chats = collection.Find(_ => true);
        return chats.ToList();
    }

    public static Chat? GetChat(Chat chat)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Id, chat.Id);
        Chat foundChat = collection.Find(filter).FirstOrDefault();
        return foundChat;
    }

    public static Chat? GetChatByName(string? name)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Name, name);
        Chat? foundChat = collection.Find(filter).FirstOrDefault();
        return foundChat;
    }

    public static List<Chat>? GetChatsWithCurrentUser(User user)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Where(
            c => c.Members.Any(m => m.Name == user!.Name)
        );
        List<Chat> chatRooms = collection.Find(filter).ToList();
        return chatRooms;
    }

    public static void AddMemberToChat(User newMember, Chat chat)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Id, chat.Id);
        UpdateDefinition<Chat> update = Builders<Chat>.Update.Push(c => c.Members, newMember);
        collection.UpdateOneAsync(filter, update);
    }

    public static void RemoveMemberFromChat(User member, Chat chat)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Id, chat.Id);
        UpdateDefinition<Chat> update = Builders<Chat>.Update.Pull(c => c.Members, member);
        collection.UpdateOneAsync(filter, update);
    }

    public static void AddMessageToChat(string chatId, Message message)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
        UpdateDefinition<Chat> update = Builders<Chat>.Update.Push(c => c.Messages, message);

        Chat chat = collection.Find(filter).FirstOrDefault();
        if (chat.Messages.Count >= 30) {
            for (int i = chat.Messages.Count; i >= 30; i--) {
                RemoveOldestMessageFromChat(chatId);
            }
        }
        
        collection.UpdateOneAsync(filter, update);
    }

    public static void RemoveOldestMessageFromChat(string chatId)
    {
        IMongoCollection<Chat> collection = GetCollection<Chat>("ChatRooms");
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
        UpdateDefinition<Chat> update = Builders<Chat>.Update.PopFirst(c => c.Messages);
        collection.UpdateOneAsync(filter, update);
    }
}