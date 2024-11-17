namespace ChatApp.Client.Commands.UserCommands;

using Shared.Models.ChatModel;

public interface IUserCommands
{

  public static void PrintStartMenu()
  {
    Console.Clear();
    Console.WriteLine("********* Welcome to Chat App! *********");
    Console.WriteLine("");
    Console.WriteLine("Available commands:");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("login - To login with an existing account");
    Console.WriteLine("signup - To register a new account: ");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("");
    Console.Write("Choose a command: ");
  }

  public static void PrintMenu()
  {
    Console.WriteLine("");
    Console.WriteLine("Available commands:");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("join - Join chat room");
    Console.WriteLine("create - Create chat room");
    Console.WriteLine("addmember - Add member to chat room");
    Console.WriteLine("removemember - Remove member from chat room");
    Console.WriteLine("delete - Delete chat room");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("logout - To logout from your account");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("");
    Console.Write("Choose a command or start chatting by writing a message: ");
  }

  public static void PrintChatRoomMenu()
  {
    Console.WriteLine("");
    Console.WriteLine("Available commands:");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("join - Join another chat room");
    Console.WriteLine("create - Create a new chat room");
    Console.WriteLine("addmember - Add member to chat room");
    Console.WriteLine("removemember - Remove member from chat room");
    Console.WriteLine("delete - Delete chat room");
    Console.WriteLine("leave - Leave chat room");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("logout - To logout from your account");
    Console.WriteLine("-------------------------------------");
    Console.WriteLine("");
    Console.Write("Choose a command or start chatting by writing a message: ");
  }

  public static void PrintContinueMenu()
  {
    Console.Write("Press any key to continue: ");
    Console.ReadLine();
    Console.Clear();
    PrintMenu();
  }


  public static KeyValuePair<string, string> Login()
  {
    string? name = null;
    while (string.IsNullOrWhiteSpace(name))
    {
      Console.Clear();
      Console.Write("Name: ");
      name = Console.ReadLine();
    }

    string? password = null;
    while (string.IsNullOrWhiteSpace(password))
    {
      Console.Clear();
      Console.Write("Password: ");
      password = Console.ReadLine();
    }

    return new KeyValuePair<string, string>(name, password);
  }

  public static KeyValuePair<string, string> Signup()
  {
    string? name = null;
    while (string.IsNullOrWhiteSpace(name))
    {
      Console.Clear();
      Console.Write("Name: ");
      name = Console.ReadLine();
    }

    string? password = null;
    while (string.IsNullOrWhiteSpace(password))
    {
      Console.Clear();
      Console.Write("Password: ");
      password = Console.ReadLine();
    }

    return new KeyValuePair<string, string>(name, password);
  }
}