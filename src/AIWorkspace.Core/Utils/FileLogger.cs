namespace AIWorkspace.Core.Utils;

public static class FileLogger
{
    private static readonly string LogDir;
    private static readonly object LockObj = new();

    static FileLogger()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        LogDir = Path.Combine(appData, "AIWorkspace", "logs");
        Directory.CreateDirectory(LogDir);
    }

    private static string GetLogFilePath()
    {
        return Path.Combine(LogDir, $"app-{DateTime.Now:yyyyMMdd}.log");
    }

    private static void WriteLine(string level, string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (LockObj)
        {
            try
            {
                File.AppendAllText(GetLogFilePath(), line + Environment.NewLine);
            }
            catch { /* Silently fail to avoid recursion */ }
        }
    }

    public static void Info(string message) => WriteLine("INFO", message);
    public static void Warning(string message) => WriteLine("WARN", message);
    public static void Error(string message) => WriteLine("ERROR", message);
    public static void Error(Exception ex) => WriteLine("ERROR", $"{ex.Message}\n{ex.StackTrace}");
}
