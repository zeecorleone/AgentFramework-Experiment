

using System.ComponentModel;

namespace AF.Shared.Tools;

public static class Tools
{
    [Description("Gets the current date and time.")] //--> side note: everything cost tokens, so if your tool name is descriptive enough, you might not need this attribute.
    public static DateTime CurrentDateAndTime(TimeType type)
    {
        return type switch
        {
            TimeType.Local => DateTime.Now,
            TimeType.Utc => DateTime.UtcNow,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    [Description("Gets the current time zone.")]
    public static string CurrentTimeZone()
    {
        return TimeZoneInfo.Local.StandardName;
    }
}

public enum TimeType
{
    Local,
    Utc
}
