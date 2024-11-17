using Shared.Models.UserModel;

namespace Shared.Models.MessageModel;

public class Message
{
    public User? User { get; set; }
    public string? MessageText { get; set; }

    public Message(User user, string messageText)
    {
        this.User = user;
        this.MessageText = messageText;
    }
}