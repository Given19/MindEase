using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace MindEase.Services;

public static class NotificationService
{
    private const int DailyCheckInId = 1001;

    public static async Task<bool> RequestPermissionAsync()
    {
#if ANDROID || IOS || MACCATALYST

        try
        {
            return await LocalNotificationCenter.Current
                .RequestNotificationPermission();
        }
        catch
        {
            return false;
        }

#else

        // Windows does not use Plugin.LocalNotification
        // in this project.
        return false;

#endif
    }

    public static async Task ScheduleDailyCheckInAsync(
        int hour = 20,
        int minute = 0)
    {
#if ANDROID || IOS || MACCATALYST

        try
        {
            var now = DateTimeOffset.Now;

            var firstOccurrence =
                new DateTimeOffset(
                    now.Year,
                    now.Month,
                    now.Day,
                    hour,
                    minute,
                    0,
                    now.Offset);

            if (firstOccurrence <= now)
            {
                firstOccurrence =
                    firstOccurrence.AddDays(1);
            }

            var notification =
                new NotificationRequest
                {
                    NotificationId =
                        DailyCheckInId,

                    Title =
                        "MindEase 🌿",

                    Description =
                        "How are you feeling today? " +
                        "Take a moment to check in.",

                    Schedule =
                        new NotificationRequestSchedule
                        {
                            NotifyTime =
                                firstOccurrence,

                            RepeatType =
                                NotificationRepeat.Daily
                        }
                };

            await LocalNotificationCenter.Current
                .Show(notification);
        }
        catch
        {
            // Notifications should never crash
            // the MindEase application.
        }

#else

        await Task.CompletedTask;

#endif
    }

    public static void CancelDailyCheckIn()
    {
#if ANDROID || IOS || MACCATALYST

        try
        {
            LocalNotificationCenter.Current
                .Cancel(DailyCheckInId);
        }
        catch
        {
            // Ignore notification cancellation errors.
        }

#endif
    }
}