namespace AIWorkspace.Core.Utils;

public static class FileProcessor
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".json", ".xml", ".yaml", ".yml", ".sql", ".csv", ".md", ".ini", ".conf", ".config", ".properties"
    };

    public static bool IsSupported(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return SupportedExtensions.Contains(ext);
    }

    public static string ReadContent(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var content = File.ReadAllText(filePath);

        return ext switch
        {
            ".json" => FormatJson(content),
            ".xml" => FormatXml(content),
            ".yaml" or ".yml" => content,  // Already human-readable
            _ => content
        };
    }

    private static string FormatJson(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return System.Text.Json.JsonSerializer.Serialize(doc, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return json;
        }
    }

    private static string FormatXml(string xml)
    {
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            using var stringWriter = new StringWriter();
            using var xmlTextWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlTextWriter);
            return stringWriter.ToString();
        }
        catch
        {
            return xml;
        }
    }
}
