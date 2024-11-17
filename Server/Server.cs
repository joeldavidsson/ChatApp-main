namespace ChatApp.Server;

using System.Net;
using ChatApp.Server.Commands.UserCommands;
using ChatApp.Server.Commands.ChatCommands;
using ChatApp.Server.Commands.DatabaseCommands;
using System.Net.Sockets;
using Shared.Models.UserModel;
using Shared.Models.ChatModel;
using Shared.Models.MessageModel;
using Shared.Models.ConnectionInfoModel;
using System.Text.Json;
using System.Text;

public static class Server
{
    static Socket? server;

    static Dictionary<Socket, ConnectionInfo> clients = new();

    public static void Start()
    {
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint ipEndPoint = new(ipAddress, 25500);

        server = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(ipEndPoint);
        server.Listen();

        Console.WriteLine("Wait for connection...");

        while (true)
        {
            if (server.Poll(10, SelectMode.SelectRead))
            {
                Socket client = server.Accept();
                KeyValuePair<Socket, ConnectionInfo> connection = new(client, new ConnectionInfo(null, null));

                while (connection.Value.LoggedInUser == null)
                {
                    string msg = ReceiveString(client);

                    switch (msg)
                    {
                        case "login":
                            while (true)
                            {
                                if (client.Poll(10, SelectMode.SelectRead) && client.Available == 0)
                                {
                                    string json = JsonSerializer.Serialize(new Message(connection.Value.LoggedInUser!, "Disconnected!"));
                                    Console.WriteLine($"{connection.Value.LoggedInUser?.Name} disconnect!");
                                    byte[] buffer = Encoding.UTF8.GetBytes(json, 0, json.Length);

                                    client.Close();
                                    clients.Remove(client);
                                    ServerBroadcastMessage(buffer, buffer.Length);
                                    break;
                                }
                                else
                                {
                                    bool authenticated = false;
                                    string json = ReceiveString(client);
                                    KeyValuePair<string, string> login = JsonSerializer.Deserialize<KeyValuePair<string, string>>(json);
                                    User? user = IUserCommands.Authenticator(login.Key, login.Value);
                                    if (user != null)
                                    {
                                        authenticated = true;
                                        connection.Value.LoggedInUser = user;
                                        clients.Add(connection.Key, connection.Value);
                                        Console.WriteLine($"{user.Name} connected!");
                                        BroadcastMessage(connection, "Connected!");

                                        byte[] loginBuffer = BitConverter.GetBytes(authenticated);
                                        client.Send(loginBuffer);
                                        break;
                                    }
                                    else
                                    {
                                        byte[] loginBuffer = BitConverter.GetBytes(authenticated);
                                        client.Send(loginBuffer);
                                        continue;
                                    }
                                }
                            }

                            continue;
                        case "signup":
                            string received = ReceiveString(connection.Key);
                            KeyValuePair<string, string> serialized = JsonSerializer.Deserialize<KeyValuePair<string, string>>(received);
                            bool signupSuccessful = IUserCommands.SignupCheck(serialized);
                            byte[] signupBuffer = BitConverter.GetBytes(signupSuccessful);
                            client.Send(signupBuffer);
                            continue;
                        default:
                            continue;
                    }
                }
            }

            if (clients.Count == 0)
            {
                continue;
            }

            foreach (KeyValuePair<Socket, ConnectionInfo> client in clients)
            {
                if (client.Key.Poll(10, SelectMode.SelectRead))
                {
                    if (client.Key.Available == 0)
                    {
                        string json = JsonSerializer.Serialize(new Message(client.Value.LoggedInUser!, "Disconnected!"));
                        Console.WriteLine($"{client.Value.LoggedInUser?.Name} disconnect!");
                        byte[] buffer = Encoding.UTF8.GetBytes(json, 0, json.Length);

                        client.Key!.Close();
                        clients.Remove(client.Key!);
                        ServerBroadcastMessage(buffer, buffer.Length);
                        continue;
                    }

                    string msg = ReceiveString(client.Key);
                    if (msg.Length > 0)
                    {
                        switch (msg)
                        {
                            case "logout":
                                string response = IUserCommands.Logout(client.Value.LoggedInUser);
                                SendString(client.Key, response);
                                if (client.Value.LoggedInUser != null)
                                {
                                    clients.Remove(client.Key);
                                }
                                continue;
                            case "join":
                                string chatName = ReceiveString(client.Key);
                                KeyValuePair<Chat?, string> pair = IChatCommands.Authorization(chatName, client.Value.LoggedInUser);
                                ConnectionInfo connectionInfo = new(client.Value.LoggedInUser, pair.Key);
                                List<Message>? history = pair.Key?.Messages;
                                string historyJson = JsonSerializer.Serialize(history);
                                SendString(client.Key, historyJson);
                                client.Value.ChatRoom = pair.Key;

                                continue;
                            case "leave":
                                response = IChatCommands.LeaveChat(client.Value.ChatRoom);
                                client.Value.ChatRoom = null;
                                SendString(client.Key, response);
                                continue;
                            case "create":
                                string res = ReceiveString(client.Key);
                                response = IChatCommands.CreateChatCheck(res, client.Value.LoggedInUser!);
                                SendString(client.Key, response);
                                continue;
                            case "delete":
                                res = ReceiveString(client.Key);
                                response = IChatCommands.DeleteChatCheck(res, client.Value.LoggedInUser, client.Value.ChatRoom);

                                SendString(client.Key, response);
                                continue;
                            case "addmember":
                                string received = ReceiveString(client.Key);
                                KeyValuePair<string, string> serialized = JsonSerializer.Deserialize<KeyValuePair<string, string>>(received);
                                response = IChatCommands.AddMemberCheck(serialized, client.Value.LoggedInUser!);
                                SendString(client.Key, response);
                                continue;
                            case "removemember":
                                received = ReceiveString(client.Key);
                                serialized = JsonSerializer.Deserialize<KeyValuePair<string, string>>(received);
                                response = IChatCommands.RemoveMemberCheck(serialized, client.Value.LoggedInUser!);
                                SendString(client.Key, response);
                                continue;
                            default:
                                BroadcastMessage(client, msg);
                                continue;
                        }
                    }
                }
            }
        }
    }

    static string ReceiveString(Socket client)
    {
        byte[] buffer = new byte[10000];
        int read = client.Receive(buffer);
        string msg = Encoding.UTF8.GetString(buffer, 0, read);
        return msg;
    }

    static void SendString(Socket client, string msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg, 0, msg.Length);
        client.Send(buffer);
    }

    static void BroadcastMessage(KeyValuePair<Socket, ConnectionInfo> client, string msg)
    {
        foreach (KeyValuePair<Socket, ConnectionInfo> pair in clients)
        {
            if (
                pair.Key != client.Key
                && (
                    pair.Value.ChatRoom == client.Value.ChatRoom
                    || pair.Value.ChatRoom?.Id == client.Value.ChatRoom?.Id
                )
            )
            {
                Message message = new(client.Value.LoggedInUser!, msg);
                string json = JsonSerializer.Serialize(message);
                SendString(pair.Key, json);
            }
        }
        if (client.Value.ChatRoom != null)
        {
            Message message = new(client.Value.LoggedInUser!, msg);
            IDatabaseCommands.AddMessageToChat(client.Value.ChatRoom?.Id!, message!);
        }
    }

    static void ServerBroadcastMessage(byte[] buffer, int read)
    {
        foreach (KeyValuePair<Socket, ConnectionInfo> pair in clients)
        {
            pair.Key.Send(buffer, read, SocketFlags.None);
        }
    }
}