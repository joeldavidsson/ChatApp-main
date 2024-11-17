using System.Text.Json.Serialization;
using Shared.Models.ChatModel;
using Shared.Models.UserModel;

namespace Shared.Models.ConnectionInfoModel;

public class ConnectionInfo {
  public User? LoggedInUser { get; set; } = null;
  public Chat? ChatRoom { get; set; } = null;

  public ConnectionInfo(User? user, Chat? chat) {
    LoggedInUser = user;
    ChatRoom = chat;
  }
}