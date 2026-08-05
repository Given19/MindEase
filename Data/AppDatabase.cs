using SQLite;
using MindEase.Models;

namespace MindEase.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection? _database;

    public async Task Init()
    {
        if (_database != null)
            return;

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "MindEase.db");

        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<MoodEntry>();
        await _database.CreateTableAsync<JournalEntry>();
    }

    // USERS
    public async Task<List<User>> GetUsersAsync()
    {
        await Init();
        return await _database!.Table<User>().ToListAsync();
    }

    public async Task<int> SaveUserAsync(User user)
    {
        await Init();
        return await _database!.InsertAsync(user);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await Init();
        return await _database!.Table<User>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    // MOODS
    public async Task<int> SaveMoodAsync(MoodEntry mood)
    {
        await Init();
        return await _database!.InsertAsync(mood);
    }

    // JOURNAL
    public async Task<int> SaveJournalAsync(JournalEntry journal)
    {
        await Init();
        return await _database!.InsertAsync(journal);
    }
}