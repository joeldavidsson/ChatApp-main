using ChatApp.Server.Commands.DatabaseCommands;
using Shared.Models.UserModel;
using Shared.Models.ChatModel;

namespace ChatApp.Server.Commands.ChatCommands;

public interface IChatCommands
{
    public static KeyValuePair<Chat?, string> Authorization(string chatName, User? user)
    {
        List<Chat>? availableChats = IDatabaseCommands.GetChatsWithCurrentUser(user!);

        if (availableChats!.Any(c => c.Name == chatName))
        {
            Chat chat = availableChats!.FindAll(c => c.Name == chatName).First();
            return new KeyValuePair<Chat?, string> (chat, $"You joined the chat room '{chat.Name}'");
        }
        else if (availableChats == null)
        {
            return new KeyValuePair<Chat?, string> (null, "You are not a member of any chats");
        }
        else
        {
            return new KeyValuePair<Chat?, string> (null, "There are no chats you can join with that name");
        }
    }

    public static string LeaveChat(Chat? chat)
    {
        if (chat == null) {
            return "You're not in a chat";
        } else {
            return $"You left the chat room '{chat.Name}'!";
        }
    }

    public static string CreateChatCheck(string chatName, User user)
    {
        List<Chat> chats = IDatabaseCommands.GetChats();

        if (chats.Any(c => c.Name == chatName))
        {
            return "Chat with that name already exist";
        }
        else
        {
            Chat chat = new(chatName, user);
            IDatabaseCommands.CreateChat(chat);
            return "New chat created!";
        }
    }

    public static string DeleteChatCheck(string chatName, User? LoggedInUser, Chat? chatRoom) {
        Chat? chat = IDatabaseCommands.GetChatByName(chatName);

        if (chat == null) {
            return $"There is no chat with the name '{chatName}'";
        } else {
            if (chat.Creator.Id == LoggedInUser?.Id) {
                if (chat.Id == chatRoom?.Id) {
                    return "You can't delete the chat room you're in";
                } else {
                    IDatabaseCommands.DeleteChat(chat);
                    return $"The chat room '{chat.Name}' was deleted";                    
                }
            } else {
                return "You can't delete a chat room which you are not the creator of";
            }
        }
    }

    public static string AddMemberCheck(KeyValuePair<string, string> pair, User loggedInUser)
    {
        Chat? chat = IDatabaseCommands.GetChatByName(pair.Key);
        User? newMember = IDatabaseCommands.GetUserByName(pair.Value);
        if (pair.Key == null)
        {
            if (IDatabaseCommands.GetChatByName(pair.Key) == null)
            {
                return "Not a valid chat's name";
            }
            else if (IDatabaseCommands.GetChatByName(pair.Key)?.Creator.Id != loggedInUser.Id)
            {
                return "You can't add a member to a chat room you're not the creator of!";
            }
        }
        else if (pair.Value == null)
        {
            if (IDatabaseCommands.GetUserByName(pair.Value) == null)
            {
                return "A user with that name doesn't exist";
            }
            else if (pair.Value == loggedInUser.Name)
            {
                return "You can't add yourself as a member";
            }
            else if (chat!.Members.Any(m => m.Id == newMember?.Id))
            {
                return "You can't add a user that is already a member!";
            }
        }
        else
        {
            IDatabaseCommands.AddMemberToChat(newMember!, chat!);
            return $"{pair.Value} was added to the chat room '{pair.Key}'";
        }
        return "";
    }

    public static string RemoveMemberCheck(KeyValuePair<string, string> pair, User loggedInUser)
    {
        Chat? chat = IDatabaseCommands.GetChatByName(pair.Value);
        User? member = IDatabaseCommands.GetUserByName(pair.Key);
        if (pair.Value == null)
        {
            if (chat == null)
            {
                return "Not a valid chat's name";
            }
            else if (chat.Creator.Id != loggedInUser.Id)
            {
                return "You can't remove a member from a chat room you're not the creator of!";
            }
        }
        else if (pair.Key == null)
        {
            if (member == null)
            {
                return "Not a valid user's name";
            }
            else if (!chat!.Members.Any(m => m.Name == pair.Key))
            {
                return "Not a valid user's name";
            }
            else if (pair.Key == loggedInUser.Name)
            {
                return "You can't remove yourself from a chat that you are the creator of!";
            }
        } else 
        {
            IDatabaseCommands.RemoveMemberFromChat(member!, chat!);
            return $"{pair.Key} was removed from the chat '{pair.Value}'";
        }
        return "";
    }
}
