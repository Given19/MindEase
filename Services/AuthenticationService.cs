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
        if (string.IsNullOrWhiteSpace(firstName))
            return (false, "Please enter your first name.");

        if (string.IsNullOrWhiteSpace(lastName))
            return (false, "Please enter your last name.");

        if (string.IsNullOrWhiteSpace(email))
            return (false, "Please enter your email.");

        if (!email.Contains('@'))
            return (false, "Please enter a valid email address.");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Please enter a password.");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters.");

        email = email.Trim().ToLower();

        var existingUser =
            await _database.GetUserByEmailAsync(email);

        if (existingUser != null)
            return (false, "An account with this email already exists.");

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Password = password,
            CreatedAt = DateTime.Now
        };

        await _database.SaveUserAsync(user);

        return (true, "Account created successfully.");
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        email = email.Trim().ToLower();

        var user = await _database.GetUserByEmailAsync(email);

        if (user == null || user.Password != password)
            return null;

        return user;
    }
}