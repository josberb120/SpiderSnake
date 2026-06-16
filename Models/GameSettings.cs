namespace SpiderSnake.Models;

internal enum Difficulty
{
    Facil,
    Normal,
    Dificil
}

internal class GameSettings
{
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;
    public bool SoundEnabled { get; set; } = true;

    public int StartingIntervalMs => Difficulty switch
    {
        Difficulty.Facil => 170,
        Difficulty.Normal => 130,
        Difficulty.Dificil => 95,
        _ => 130
    };

    public int MinIntervalMs => Difficulty switch
    {
        Difficulty.Facil => 90,
        Difficulty.Normal => 65,
        Difficulty.Dificil => 50,
        _ => 65
    };
}
