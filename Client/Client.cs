using System.Net.Sockets;
using System.Net;
using System.Text;
using ChatApp.Client.Commands.UserCommands;
using ChatApp.Client.Commands.ChatCommands;
using Shared.Models.MessageModel;
using Shared.Models.UserModel;
using Shared.Models.ConnectionInfoModel;
using System.Text.Json;
using Shared.Models.ChatModel;

namespace ChatApp.Client;

public static class Client
{
  public static Socket? client = null;

  public static void Start()
  {
    IPAddress ipAddress = new(new byte[] { 127, 0, 0, 1 });
    IPEndPoint ipEndPoint = new(ipAddress, 25500);

    client = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    client.Connect(ipEndPoint);

    bool isLoggedIn = false;
    string user = "";

    IUserCommands.PrintStartMenu();

    while (!isLoggedIn)
    {
      string? input = Console.ReadLine();

      if (string.IsNullOrWhiteSpace(input))
      {
        Console.WriteLine("Invalid input. Cannot leave field empty.");
        continue;
      }

      SendString(client, input);

      switch (input)
      {
        case "login":
          for (int i = 0; i < 3; i++)
          {
            KeyValuePair<string, string> login = IUserCommands.Login();
            user = login.Key;
            string json = JsonSerializer.Serialize(login);
            SendString(client, json);

            byte[] boolBuffer = new byte[sizeof(bool)];
            _ = client.Receive(boolBuffer);
            bool confirmed = BitConverter.ToBoolean(boolBuffer, 0);

            isLoggedIn = confirmed;

            if (isLoggedIn)
            {
              Console.WriteLine($"You logged in as {login.Key}");
              break;
            }
            else
            {
              Console.WriteLine("Wrong username or password!");
              if (i == 2)
              {
                Console.WriteLine("Too many attempts!");
                return;
              }
            }
          }
          continue;

        case "signup":
          KeyValuePair<string, string> newUser = IUserCommands.Signup();
          string serialized = JsonSerializer.Serialize(newUser);
          SendString(client, serialized);

          byte[] signupBuffer = new byte[sizeof(bool)];
          _ = client.Receive(signupBuffer);
          bool signupSuccessful = BitConverter.ToBoolean(signupBuffer, 0);

          if (signupSuccessful)
          {
            Console.WriteLine($"Registered as {newUser.Key}!");
          }
          else
          {
            Console.WriteLine("Registration failed. Try again!");
          }

          Console.WriteLine("Please login to continue.");
          continue;

        default:
          continue;
      }
    }

    Console.WriteLine("********* You are logged in *********");

    IUserCommands.PrintMenu();

    bool restart = false;

    while (true)
    {
      if (Console.KeyAvailable)
      {
        string? msg = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(msg))
        {
          SendString(client, msg);

          switch (msg)
          {
            case "logout":
              string response = ReceiveString(client);
              Console.WriteLine(response);
              restart = true;
              break;
            case "join":
              string chatName = IChatCommands.JoinChat();
              SendString(client, chatName);

              response = ReceiveString(client);

              List<Message>? history = JsonSerializer.Deserialize<List<Message>>(response);

              if (history?.Count > 0)
              {
                IChatCommands.PrintChatHistory(history, user);
              }
              Console.WriteLine($"You have joined chatroom: {chatName}.");
              IUserCommands.PrintChatRoomMenu();

              continue;
            case "create":
              string newChat = IChatCommands.CreateChat();
              SendString(client, newChat);

              response = ReceiveString(client);
              Console.WriteLine(response);
              IUserCommands.PrintContinueMenu();
              continue;
            case "delete":
              chatName = IChatCommands.DeleteChat();
              SendString(client, chatName);

              response = ReceiveString(client);
              Console.WriteLine(response);
              IUserCommands.PrintContinueMenu();
              continue;
            case "leave":
              Console.Clear();
              response = ReceiveString(client);
              Console.WriteLine(response);
              IUserCommands.PrintContinueMenu();
              continue;
            case "addmember":
              KeyValuePair<string, string> pair = IChatCommands.AddMember();
              string serialized = JsonSerializer.Serialize(pair);
              SendString(client, serialized);

              response = ReceiveString(client);
              Console.WriteLine(response);
              IUserCommands.PrintContinueMenu();
              continue;
            case "removemember":
              pair = IChatCommands.RemoveMember();
              serialized = JsonSerializer.Serialize(pair);
              SendString(client, serialized);

              response = ReceiveString(client);
              Console.WriteLine(response);
              IUserCommands.PrintContinueMenu();
              continue;
            default:
              continue;
          }
        }
      }
      ReceiveMessage();
      if (restart == true)
      {
        Start();
      }
    }
  }

  static void ReceiveMessage()
  {
    if (client?.Available != 0)
    {
      byte[] buffer = new byte[1024];
      int read = client!.Receive(buffer);

      if (read > 0)
      {
        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, read);
        if (!string.IsNullOrWhiteSpace(receivedMessage))
        {
          Message message = JsonSerializer.Deserialize<Message>(receivedMessage)!;
          Console.Write(
              new string(
                  ' ',
                  Console.WindowWidth
                      - message!.MessageText!.Length
                      - message.User!.Name.Length
                      - 2
              )
          );
          Console.WriteLine($"{message.User?.Name}: {message?.MessageText}");
        }
      }
    }
  }

  static string ReceiveString(Socket client)
  {
    byte[] buffer = new byte[10000];
    int length = client.Receive(buffer);
    string json = Encoding.UTF8.GetString(buffer, 0, length);
    return json;
  }

  static void SendString(Socket client, string msg)
  {
    byte[] buffer = Encoding.UTF8.GetBytes(msg, 0, msg.Length);
    client.Send(buffer);
  }
}