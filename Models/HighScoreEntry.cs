namespace SpiderSnake.Models;

internal class HighScoreEntry
{
    public string Name { get; set; } = "Trepamuros";
    public int Score { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}
