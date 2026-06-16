using SpiderSnake.Controls;
using SpiderSnake.Services;
using SpiderSnake.Theme;

namespace SpiderSnake.Screens;

internal class HighScoresScreen : ThemedScreenBase
{
    private const int ListWidth = 520;
    private const int ListHeight = 400;

    public event Action? BackClicked;

    private readonly Label _title;
    private readonly Panel _listPanel;
    private readonly SpideyButton _back;

    public HighScoresScreen()
    {
        Accent = SpideyTheme.GoldAccent;
        AccentDark = SpideyTheme.NearBlack;

        _title = MakeTitle("PUNTUACIONES", 36f, Color.White);
        Controls.Add(_title);

        _listPanel = new Panel { BackColor = Color.Transparent, Size = new Size(ListWidth, ListHeight) };
        _listPanel.Paint += (_, e) => SpideyTheme.PaintCard(e.Graphics, _listPanel.ClientRectangle);
        Controls.Add(_listPanel);

        _back = new SpideyButton
        {
            Text = "← VOLVER",
            Size = new Size(180, 46),
            AccentColor = SpideyTheme.SpideyBlue,
            AccentColorDark = SpideyTheme.SpideyBlueDark,
        };
        _back.Click += (_, _) => BackClicked?.Invoke();
        Controls.Add(_back);

        RefreshScores();
        Reflow();
    }

    protected override void Reflow()
    {
        // Centra todo el bloque (título + lista + volver) según el alto disponible en vez de
        // fijar la lista arriba y el botón pegado al fondo real de la ventana.
        const int blockHeight = 552;
        int top = Math.Max(36, (Height - blockHeight) / 2);

        _title.Size = new Size(Width, 50);
        _title.Location = new Point(0, top);

        _listPanel.Location = new Point((Width - _listPanel.Width) / 2, top + 74);
        CenterHorizontally(_back, top + 506);
    }

    public void RefreshScores()
    {
        _listPanel.Controls.Clear();
        var entries = HighScoreService.Load();

        if (entries.Count == 0)
        {
            _listPanel.Controls.Add(new Label
            {
                Text = "Todavía no hay puntuaciones. ¡Sé el primer trepamuros del ranking!",
                Font = SpideyTheme.BodyFont(11f, FontStyle.Italic),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Location = new Point(20, 20),
                Size = new Size(_listPanel.Width - 40, 40),
            });
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            bool top3 = i < 3;
            var row = new Label
            {
                Text = $"{i + 1,2}.   {entry.Name,-16}   {entry.Score,6} pts   {entry.Date:dd/MM/yyyy}",
                Font = SpideyTheme.BodyFont(top3 ? 13f : 11.5f, top3 ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = top3 ? SpideyTheme.GoldAccent : Color.White,
                AutoSize = false,
                BackColor = Color.Transparent,
                Location = new Point(30, 16 + i * 35),
                Size = new Size(_listPanel.Width - 60, 30),
            };
            _listPanel.Controls.Add(row);
        }
    }
}
