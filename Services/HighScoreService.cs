using System.Text.Json;
using SpiderSnake.Models;

namespace SpiderSnake.Services;

internal static class HighScoreService
{
    private const int MaxEntries = 10;

    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpiderSnake");

    private static readonly string FilePath = Path.Combine(FolderPath, "highscores.json");

    public static List<HighScoreEntry> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new List<HighScoreEntry>();
            string json = File.ReadAllText(FilePath);
            var entries = JsonSerializer.Deserialize<List<HighScoreEntry>>(json);
            return entries ?? new List<HighScoreEntry>();
        }
        catch
        {
            return new List<HighScoreEntry>();
        }
    }

    public static bool QualifiesForTop(int score)
    {
        if (score <= 0) return false;
        var entries = Load();
        return entries.Count < MaxEntries || entries.Any(e => score > e.Score);
    }

    public static List<HighScoreEntry> Save(HighScoreEntry entry)
    {
        var entries = Load();
        entries.Add(entry);
        entries = entries.OrderByDescending(e => e.Score).Take(MaxEntries).ToList();

        try
        {
            Directory.CreateDirectory(FolderPath);
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // Si no se puede persistir (permisos, disco, etc.) seguimos sin romper el juego.
        }

        return entries;
    }

    public static int BestScore()
    {
        var entries = Load();
        return entries.Count == 0 ? 0 : entries.Max(e => e.Score);
    }
}
