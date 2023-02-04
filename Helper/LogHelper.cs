namespace BomberKnight.Helper;

internal enum LogType
{
    Normal,

    Warning,

    Error,

    Debug
}

internal static class LogHelper
{
    internal static void Write(string message, LogType logType = LogType.Normal)
    {
        switch (logType)
        {
            case LogType.Normal:
                BomberKnight.Instance.Log(message);
                break;
            case LogType.Warning:
                BomberKnight.Instance.LogWarn(message);
                break;
            case LogType.Error:
                BomberKnight.Instance.LogError(message);
                break;
            case LogType.Debug:
                BomberKnight.Instance.LogDebug(message);
                break;
            default:
                break;
        }
    }
}
