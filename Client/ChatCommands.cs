namespace ChatApp.Client.Commands.ChatCommands;

using Shared.Models.ChatModel;
using Shared.Models.UserModel;
using Shared.Models.MessageModel;
using ChatApp.Client.Commands.UserCommands;

public interface IChatCommands
{

  public static string JoinChat()
  {
    string? chatName = null;
    while (string.IsNullOrWhiteSpace(chatName))
    {
      Console.Clear();
      Console.Write("Chat Room Name: ");
      chatName = Console.ReadLine();
    }
    return chatName;
  }

  public static string CreateChat()
  {
    string? chatName = null;
    while (string.IsNullOrWhiteSpace(chatName))
    {
      Console.Clear();
      Console.Write("Chat room name: ");
      chatName = Console.ReadLine();
    }
    return chatName;
  }

  public static string DeleteChat()
  {
    string? chatName = null;
    while (string.IsNullOrWhiteSpace(chatName))
    {
      Console.Clear();
      Console.Write("Chat room name: ");
      chatName = Console.ReadLine();
    }
    return chatName;
  }

  public static KeyValuePair<string, string> AddMember()
  {
    string? chatName = null;
    Console.Clear();
    while (string.IsNullOrWhiteSpace(chatName))
    {
      Console.Write("Chat room name: ");
      chatName = Console.ReadLine();
      Console.Clear();
    }

    string? memberName = null;
    Console.Clear();
    while (string.IsNullOrWhiteSpace(memberName))
    {
      Console.Write("Members name: ");
      memberName = Console.ReadLine();
      Console.Clear();
    }
    return new KeyValuePair<string, string>(chatName, memberName);
  }

  public static KeyValuePair<string, string> RemoveMember()
  {
    string? chatName = null;
    Console.Clear();
    while (string.IsNullOrWhiteSpace(chatName))
    {
      Console.Write("Chat room name: ");
      chatName = Console.ReadLine();
      Console.Clear();
    }

    string? memberName = null;
    Console.Clear();
    while (string.IsNullOrWhiteSpace(memberName))
    {
      Console.Write("Members name: ");
      memberName = Console.ReadLine();
      Console.Clear();
    }
    return new KeyValuePair<string, string>(memberName, chatName);
  }

  public static void PrintChatHistory(List<Message> list, string loggedInUser)
  {
    foreach (Message message in list)
    {
      if (message.User?.Name == loggedInUser)
      {
        Console.WriteLine($"You: {message.MessageText}");
      }
      else
      {
        Console.Write(
            new string(
                ' ',
                Console.WindowWidth - message.MessageText!.Length - message.User!.Name!.Length - 2
            )
        );
        Console.WriteLine($"{message.User!.Name}: {message.MessageText}");
      }
    }
  }
}