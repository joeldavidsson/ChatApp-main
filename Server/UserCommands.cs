namespace ChatApp.Server.Commands.UserCommands;

using ChatApp.Server.Commands.DatabaseCommands;
using Shared.Models.UserModel;

public interface IUserCommands
{
    public static User? Authenticator(string name, string password)
    {
        User user = new(name, password);
        User? matchingUser = IDatabaseCommands.GetUser(user);
        if (matchingUser != null)
        {
            return matchingUser;
        }
        else
        {
            Console.WriteLine("User doesn't exist");
            return null;
        }
    }

    public static bool SignupCheck(KeyValuePair<string, string> user)
    {
        List<User> matchingUsers = IDatabaseCommands.GetUsers();
        if (matchingUsers.Any(u => u.Name == user.Key))
        {
            return false;
        }
        else
        {
            User newUser = new(user.Key, user.Value);
            IDatabaseCommands.CreateUser(newUser);
            return true;
        }
    }

    public static string Logout(User? user)
    {
        if (user == null) {
            return "You are already logged out";
        } else {
            return "You logged out!";
        }
    }
}
