using System.Text.Json;
using SpiderSnake.Models;

namespace SpiderSnake.Services;

internal static class SettingsService
{
    private static readonly string FolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpiderSnake");

    private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

    public static GameSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new GameSettings();
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
        }
        catch
        {
            return new GameSettings();
        }
    }

    public static void Save(GameSettings settings)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // Igual que con los puntajes: si falla la persistencia, no rompemos el juego.
        }
    }
}
