using BCrypt.Net;
using MindEase.Data;
using MindEase.Models;

namespace MindEase.Services;

public class AuthenticationService
{
    private readonly AppDatabase _database;

    public AuthenticationService(AppDatabase database)
    {
        _database = database;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password)
    {
        var existingUser = await _database.GetUserByEmailAsync(email);

        if (existingUser != null)
            return (false, "An account with this email already exists.");

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DateCreated = DateTime.Now
        };

        await _database.SaveUserAsync(user);

        return (true, "Account created successfully.");
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _database.GetUserByEmailAsync(email);

        if (user == null)
            return null;

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        return valid ? user : null;
    }
}