using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MindEase.Models;

namespace MindEase.Services;

public class ApiService
{
    private const string BaseUrl = "https://mindease-backend-c9aq.onrender.com/api";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private void AttachToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            !string.IsNullOrEmpty(UserSession.CurrentUser?.Token)
                ? new AuthenticationHeaderValue("Bearer", UserSession.CurrentUser!.Token)
                : null;
    }

    public async Task<List<CommunityPostDto>> GetCommunityPostsAsync()
    {
        AttachToken();

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CommunityPostDto>>($"{BaseUrl}/community/posts", JsonOptions);
            return result ?? new List<CommunityPostDto>();
        }
        catch
        {
            return new List<CommunityPostDto>();
        }
    }

    public async Task<(bool Success, string Message)> CreateCommunityPostAsync(string content, bool isAnonymous)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/community/posts", new { content, isAnonymous });

            if (!response.IsSuccessStatusCode)
                return (false, "Could not create post.");

            return (true, "Posted.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not reach server: {ex.Message}");
        }
    }

    public async Task<List<CommunityCommentDto>> GetCommentsAsync(int postId)
    {
        AttachToken();

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CommunityCommentDto>>($"{BaseUrl}/community/posts/{postId}/comments", JsonOptions);
            return result ?? new List<CommunityCommentDto>();
        }
        catch
        {
            return new List<CommunityCommentDto>();
        }
    }

    public async Task<(bool Success, string Message)> CreateCommentAsync(int postId, string content, bool isAnonymous)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/community/posts/{postId}/comments",
                new { content, isAnonymous });

            if (!response.IsSuccessStatusCode)
                return (false, "Could not add comment.");

            return (true, "Comment added.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not reach server: {ex.Message}");
        }
    }

    public async Task<bool> ReportPostAsync(int postId)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/community/posts/{postId}/report", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReportCommentAsync(int commentId)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/community/comments/{commentId}/report", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(
        string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            var body = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

            if (!response.IsSuccessStatusCode || body?.User is null)
                return (false, body?.Message ?? "Registration failed.", null);

            return (true, body.Message ?? "Registered successfully.", MapUser(body));
        }
        catch (Exception ex)
        {
            return (false, $"Could not reach server: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(
        string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login", new
            {
                email,
                password
            });

            var body = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

            if (!response.IsSuccessStatusCode || body?.User is null)
                return (false, body?.Message ?? "Invalid email or password.", null);

            return (true, body.Message ?? "Login successful.", MapUser(body));
        }
        catch (Exception ex)
        {
            return (false, $"Could not reach server: {ex.Message}", null);
        }
    }

    private static User MapUser(AuthResponse body)
    {
        return new User
        {
            Id = body.User!.Id,
            FirstName = body.User.FirstName,
            LastName = body.User.LastName,
            Email = body.User.Email,
            Token = body.Token
        };
    }

    public async Task<List<TherapistDto>> GetNearbyTherapistsAsync(double? lat, double? lng)
    {
        AttachToken();

        string url = $"{BaseUrl}/therapists";

        if (lat.HasValue && lng.HasValue)
            url += $"?lat={lat.Value}&lng={lng.Value}";

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TherapistDto>>(url, JsonOptions);
            return result ?? new List<TherapistDto>();
        }
        catch
        {
            return new List<TherapistDto>();
        }
    }

    public async Task<(bool Success, string Message)> BookAppointmentAsync(
        int therapistId, DateTime appointmentDate, string sessionType, double price, string message)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/appointments", new
            {
                therapistId,
                appointmentDate,
                sessionType,
                price,
                message
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
                return (false, errorBody?.Message ?? "Booking failed.");
            }

            return (true, "Appointment requested.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not reach server: {ex.Message}");
        }
    }

    public async Task<List<AppointmentDto>> GetMyAppointmentsAsync()
    {
        AttachToken();

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AppointmentDto>>($"{BaseUrl}/appointments", JsonOptions);
            return result ?? new List<AppointmentDto>();
        }
        catch
        {
            return new List<AppointmentDto>();
        }
    }

    public async Task<bool> CancelAppointmentAsync(int appointmentId)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PutAsync($"{BaseUrl}/appointments/{appointmentId}/cancel", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string Reply)> GetChatReplyAsync(string mood, List<ChatMessageDto> messages)
    {
        AttachToken();

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/chat", new
            {
                mood,
                messages = messages.Select(m => new { role = m.Role, text = m.Text })
            });

            if (!response.IsSuccessStatusCode)
                return (false, "I couldn't get a response right now. Please try again.");

            var body = await response.Content.ReadFromJsonAsync<ChatReplyResponse>(JsonOptions);
            return (true, body?.Reply ?? "Thanks for sharing that.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not reach server: {ex.Message}");
        }
    }

    private class ChatReplyResponse
    {
        public string? Reply { get; set; }
    }

    private class AuthResponse
    {
        public string? Message { get; set; }
        public string? Token { get; set; }
        public AuthUserDto? User { get; set; }
    }

    private class AuthUserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

public class TherapistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OfficeAddress { get; set; } = string.Empty;
    public double PriceInPerson { get; set; }
    public double PriceOnline { get; set; }
    public double PricePhone { get; set; }
    public double? DistanceKm { get; set; }
}

public class AppointmentDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public double Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public TherapistDto? Therapist { get; set; }
}

public class ChatMessageDto
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = string.Empty;
}

public class CommunityPostDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CommentCount { get; set; }
}

public class CommunityCommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}