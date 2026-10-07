using System.Text.Json;

namespace StableVolume;

public sealed class AppSettings
{
    public bool Hold { get; set; } = false;
    public int Target { get; set; } = 40;
    public bool CeilingOnly { get; set; } = false;
    public bool KeepUnmuted { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StableVolume", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                if (s != null)
                {
                    s.Target = Math.Clamp(s.Target, 0, 100);
                    return s;
                }
            }
        }
        catch
        {
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch
        {
        }
    }
}
