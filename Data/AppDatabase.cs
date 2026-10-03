using MindEase.Models;
using SQLite;

namespace MindEase.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection? _database;

    private async Task Init()
    {
        if (_database is not null)
            return;

        string dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "mindease_v2.db3");

        _database =
            new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<MoodEntry>();
        await _database.CreateTableAsync<JournalEntry>();
        await _database.CreateTableAsync<DiaryEntry>();
        await _database.CreateTableAsync<Therapist>();
        await _database.CreateTableAsync<Appointment>();

        await SeedTherapistsAsync();
    }

    private async Task SeedTherapistsAsync()
    {
        var existing = await _database!.Table<Therapist>().CountAsync();

        if (existing > 0)
            return;

        var therapists = new List<Therapist>
        {
            new()
            {
                Name = "Dr. Naledi Khumalo",
                Specialty = "Anxiety & Stress",
                Bio = "10+ years helping clients manage anxiety and daily stress.",
                Email = "naledi.khumalo.demo@example.com",
                PhoneNumber = "012 345 6789",
                OfficeAddress = "12 Church Street, Pretoria",
                PriceInPerson = 650,
                PriceOnline = 550,
                PricePhone = 450,
                Latitude = -25.7461,
                Longitude = 28.1881
            },
            new()
            {
                Name = "Dr. Sipho Dlamini",
                Specialty = "Depression",
                Bio = "Focuses on CBT-based approaches for depression and low mood.",
                Email = "sipho.dlamini.demo@example.com",
                PhoneNumber = "011 456 7890",
                OfficeAddress = "45 Jan Smuts Ave, Johannesburg",
                PriceInPerson = 700,
                PriceOnline = 600,
                PricePhone = 500,
                Latitude = -26.2041,
                Longitude = 28.0473
            },
            new()
            {
                Name = "Dr. Aisha Patel",
                Specialty = "Relationships & Family",
                Bio = "Specialises in relationship, family, and communication issues.",
                Email = "aisha.patel.demo@example.com",
                PhoneNumber = "031 567 8901",
                OfficeAddress = "8 Florida Road, Durban",
                PriceInPerson = 600,
                PriceOnline = 500,
                PricePhone = 400,
                Latitude = -29.8587,
                Longitude = 31.0218
            },
            new()
            {
                Name = "Dr. Johan van der Merwe",
                Specialty = "Academic & Work Stress",
                Bio = "Works closely with students and young professionals.",
                Email = "johan.vdmerwe.demo@example.com",
                PhoneNumber = "021 678 9012",
                OfficeAddress = "3 Long Street, Cape Town",
                PriceInPerson = 550,
                PriceOnline = 450,
                PricePhone = 350,
                Latitude = -33.9249,
                Longitude = 18.4241
            }
        };

        foreach (var t in therapists)
            await _database.InsertAsync(t);
    }
    public async Task<User?> GetUserByEmailAsync(
        string email)
    {
        await Init();

        return await _database!
            .Table<User>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveUserAsync(User user)
    {
        await Init();

        return await _database!
            .InsertAsync(user);
    }

    public async Task<int> SaveMoodAsync(
        MoodEntry entry)
    {
        await Init();

        return await _database!
            .InsertAsync(entry);
    }

    public async Task<List<MoodEntry>>
        GetMoodEntriesByUserAsync(int userId)
    {
        await Init();

        return await _database!
            .Table<MoodEntry>()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<int> SaveJournalAsync(
        JournalEntry entry)
    {
        await Init();

        return await _database!
            .InsertAsync(entry);
    }

    public async Task<List<JournalEntry>>
        GetJournalEntriesByUserAsync(int userId)
    {
        await Init();

        return await _database!
            .Table<JournalEntry>()
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.Date)
            .ToListAsync();
    }

    public async Task<int> SaveDiaryEntryAsync(
        DiaryEntry entry)
    {
        await Init();

        return await _database!
            .InsertAsync(entry);
    }

    public async Task<List<DiaryEntry>>
        GetDiaryEntriesByUserAsync(int userId)
    {
        await Init();

        return await _database!
            .Table<DiaryEntry>()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<List<Therapist>>
        GetTherapistsAsync()
    {
        await Init();

        return await _database!
            .Table<Therapist>()
            .ToListAsync();
    }

    public async Task<int> SaveAppointmentAsync(
        Appointment appointment)
    {
        await Init();

        return await _database!
            .InsertAsync(appointment);
    }

    public async Task<List<Appointment>>
        GetAppointmentsByUserAsync(int userId)
    {
        await Init();

        return await _database!
            .Table<Appointment>()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task CancelAppointmentAsync(
        int appointmentId)
    {
        await Init();

        var appointment =
            await _database!
                .Table<Appointment>()
                .Where(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();

        if (appointment is not null)
        {
            appointment.Status = "Cancelled";

            await _database.UpdateAsync(
                appointment);
        }
    }
}