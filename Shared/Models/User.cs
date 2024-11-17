using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models.UserModel;

public class User
{ 
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    
    public User(string name, string password)
    {
        this.Name = name;
        this.Password = password;
    }

    public void PrintUser()
    {
        Console.WriteLine($"ID: {Id}");
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Password: {Password}");
    }
}