using System.Text.RegularExpressions;

namespace AIWorkspace.Core.Services;

public interface IPromptStrategy
{
    string? Label { get; }
    bool Match(string input);
    string BuildPrompt(string content);
}

public class PromptEngine
{
    private readonly List<IPromptStrategy> _strategies = new()
    {
        new JavaExceptionStrategy(),
        new PythonTracebackStrategy(),
        new LinuxLogStrategy(),
        new SqlErrorStrategy(),
        new NginxErrorStrategy(),
        new KubernetesErrorStrategy(),
        new GenericTechStrategy()
    };

    public string Enhance(string userInput, string? fileContent = null)
    {
        var content = userInput;
        if (!string.IsNullOrEmpty(fileContent))
            content += "\n\n" + fileContent;

        var strategy = _strategies.FirstOrDefault(s => s.Match(content))
            ?? _strategies.Last();

        return strategy.BuildPrompt(content);
    }

    public string? GetStrategyLabel(string userInput, string? fileContent = null)
    {
        var content = userInput;
        if (!string.IsNullOrEmpty(fileContent))
            content += "\n\n" + fileContent;

        var strategy = _strategies.FirstOrDefault(s => s.Match(content));
        return strategy?.Label;
    }
}

public class JavaExceptionStrategy : IPromptStrategy
{
    public string? Label => "Java Exception";

    private static readonly Regex Regex = new(
        @"\b(\w+Exception|\w+Error)\b|at\s+[\w\.]+\.[\w<>]+\(.*?\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool Match(string input) => Regex.IsMatch(input);

    public string BuildPrompt(string content) =>
        $"以下是一段 Java 异常日志/报错信息，请分析根因并给出修复建议：\n\n```\n{content}\n```\n\n" +
        "请按以下格式回答：\n1. 异常概述\n2. 根因分析\n3. 修复建议\n4. 预防措施";
}

public class PythonTracebackStrategy : IPromptStrategy
{
    public string? Label => "Python Error";

    private static readonly Regex Regex = new(
        @"Traceback \(most recent call last\)|\b(Traceback|File\s+""[^""]+"""",\s*line\s*\d+)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool Match(string input) => Regex.IsMatch(input);

    public string BuildPrompt(string content) =>
        $"以下是一段 Python 报错信息（Traceback），请分析错误原因并给出修复建议：\n\n```python\n{content}\n```\n\n" +
        "请按以下格式回答：\n1. 错误概述\n2. 出错位置分析\n3. 修复代码\n4. 预防措施";
}

public class LinuxLogStrategy : IPromptStrategy
{
    public string? Label => "Linux Log";

    private static readonly Regex Regex = new(
        @"(kernel panic|segfault|oom-killer|systemd\[.*?\]|ssh\[.*?\]|cron\[.*?\]|kernel:.*?\b(ERROR|WARN|FATAL|CRIT)\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool Match(string input) => Regex.IsMatch(input);

    public string BuildPrompt(string content) =>
        $"以下是一段 Linux 系统日志，请分析系统异常并给出排查建议：\n\n```\n{content}\n```\n\n" +
        "请按以下格式回答：\n1. 异常概述\n2. 影响分析\n3. 排查步骤\n4. 修复建议";
}

public class SqlErrorStrategy : IPromptStrategy
{
    public string? Label => "SQL Issue";

    private static readonly Regex Regex = new(
        @"\b(deadlock|timeout|lock wait|foreign key constraint|duplicate entry|syntax error|ORA-\d+|SQLSTATE)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool Match(string input) => Regex.IsMatch(input);

    public string BuildPrompt(string content) =>
        $"以下是一段 SQL 错误或数据库相关日志，请分析问题原因并给出优化建议：\n\n```sql\n{content}\n```\n\n" +
        "请按以下格式回答：\n1. 错误概述\n2. 根因分析\n3. 优化建议\n4. 预防措施";
}

public class NginxErrorStrategy : IPromptStrategy
{
    public string? Label => "Nginx Error";

    private static readonly Regex Regex = new(
        @"\b(upstream|502|503|504|504 Gateway Time-out|nginx|worker process|client|server)\b.*\b(error|failed)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool Match(string input) => Regex.IsMatch(input);

    public string BuildPrompt(string content) =>
        $"以下是一段 Nginx/Web 服务器日志，请分析异常并给出排查建议：\n\n```\n{content}\n```\n\n" +
        "请按以下格式回答：\n1. 异常概述\n2. 可能原因\n3. 排查步骤\n4. 修复建议";
}

public class KubernetesErrorStrategy : IPromptStrategy
{
    public string? Label => "K8s Issue";

    private static readonly Regex Regex = new(
        @"\b(CrashLoopBackOff|ImagePullBackOff|OOMKilled|Evicted|FailedMount|FailedScheduling|ContainerCreating)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool Match(string input) => Regex.IsMatch(input);

    public string BuildPrompt(string content) =>
        $"以下是一段 Kubernetes 容器/Pod 相关日志，请分析异常并给出排查建议：\n\n```\n{content}\n```\n\n" +
        "请按以下格式回答：\n1. 异常概述\n2. 根因分析\n3. 排查命令\n4. 修复建议";
}

public class GenericTechStrategy : IPromptStrategy
{
    public string? Label => null; // 兜底策略无标签

    public bool Match(string input) => true;

    public string BuildPrompt(string content) =>
        $"请分析以下技术问题/日志信息，并给出详细解答：\n\n```\n{content}\n```";
}
