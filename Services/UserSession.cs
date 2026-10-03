using MindEase.Models;

namespace MindEase.Services;

public static class UserSession
{
    public static User? CurrentUser { get; private set; }

    public static bool IsLoggedIn =>
        CurrentUser is not null;

    public static void Login(User user)
    {
        CurrentUser = user;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}