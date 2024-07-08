using System.Text.RegularExpressions;
namespace HydraKnightBot;

public static class Extensions
{
    public static string GetUserMentionString(long userId, string firstName)
    {
        return  $"<a href='tg://user?id={userId}'>{firstName}</a> ";
    }
    
    public  static TimeSpan ParseDuration(string durationString)
    {
        var match = Regex.Match(durationString, @"^(\d+)([mhd])$");
        if (!match.Success) return TimeSpan.Zero;

        int value = int.Parse(match.Groups[1].Value);
        string unit = match.Groups[2].Value;

        return unit switch
        {
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            "d" => TimeSpan.FromDays(value),
            _ => TimeSpan.Zero
        };
    }
    
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1) return $"{(int)duration.TotalDays} gün";
        if (duration.TotalHours >= 1) return $"{(int)duration.TotalHours} saat";
        return $"{(int)duration.TotalMinutes} dakika";
    }
}