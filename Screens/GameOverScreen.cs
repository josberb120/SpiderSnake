using SpiderSnake.Controls;
using SpiderSnake.Models;
using SpiderSnake.Services;
using SpiderSnake.Theme;

namespace SpiderSnake.Screens;

internal class GameOverScreen : ThemedScreenBase
{
    public event Action? RetryClicked;
    public event Action? MenuClicked;

    private readonly Label _title;
    private readonly Label _scoreLabel;
    private readonly Label _newRecordLabel;
    private readonly TextBox _nameBox;
    private readonly SpideyButton _saveButton;
    private readonly Label _savedLabel;
    private readonly SpideyButton _retry;
    private readonly SpideyButton _menu;

    private int _pendingScore;
    private bool _qualifies;
    private bool _saved;

    public GameOverScreen()
    {
        Accent = SpideyTheme.SpideyRedDark;
        AccentDark = SpideyTheme.NearBlack;

        _title = MakeTitle("¡TE ATRAPARON!", 40f, SpideyTheme.SpideyRed);
        Controls.Add(_title);

        _scoreLabel = new Label
        {
            Font = SpideyTheme.BodyFont(20f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        Controls.Add(_scoreLabel);

        _newRecordLabel = new Label
        {
            Text = "★ ¡Nueva puntuación en el top 10! Ingresa tu nombre:",
            Font = SpideyTheme.BodyFont(12f, FontStyle.Bold),
            ForeColor = SpideyTheme.GoldAccent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        Controls.Add(_newRecordLabel);

        _nameBox = new TextBox
        {
            Font = SpideyTheme.BodyFont(13f, FontStyle.Bold),
            ForeColor = Color.Black,
            MaxLength = 18,
            TextAlign = HorizontalAlignment.Center,
            Size = new Size(260, 30),
        };
        Controls.Add(_nameBox);

        _saveButton = new SpideyButton
        {
            Text = "GUARDAR PUNTUACIÓN",
            Size = new Size(260, 44),
            AccentColor = SpideyTheme.GoldAccent,
            AccentColorDark = Color.FromArgb(150, 110, 0),
        };
        _saveButton.Click += (_, _) => SavePendingScore();
        Controls.Add(_saveButton);

        _savedLabel = new Label
        {
            Text = "✔ ¡Guardado en el ranking!",
            Font = SpideyTheme.BodyFont(11f, FontStyle.Italic),
            ForeColor = SpideyTheme.GoldAccent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            Visible = false,
        };
        Controls.Add(_savedLabel);

        _retry = new SpideyButton
        {
            Text = "↻ REINTENTAR",
            Size = new Size(220, 48),
            AccentColor = SpideyTheme.SpideyRed,
            AccentColorDark = SpideyTheme.SpideyRedDark,
        };
        _retry.Click += (_, _) => RetryClicked?.Invoke();
        Controls.Add(_retry);

        _menu = new SpideyButton
        {
            Text = "MENÚ PRINCIPAL",
            Size = new Size(220, 44),
            AccentColor = SpideyTheme.SpideyBlue,
            AccentColorDark = SpideyTheme.SpideyBlueDark,
        };
        _menu.Click += (_, _) => MenuClicked?.Invoke();
        Controls.Add(_menu);

        Reflow();
    }

    protected override void Reflow()
    {
        // Bloque de contenido centrado verticalmente como un todo (antes el título/puntaje
        // quedaban fijos arriba mientras los botones se anclaban al fondo real de la ventana,
        // dejando un hueco enorme en pantallas grandes/maximizadas).
        const int blockHeight = 514;
        int top = Math.Max(50, (Height - blockHeight) / 2);

        _title.Size = new Size(Width, 60);
        _title.Location = new Point(0, top);

        _scoreLabel.Size = new Size(Width, 36);
        _scoreLabel.Location = new Point(0, top + 90);

        _newRecordLabel.Size = new Size(Width, 26);
        _newRecordLabel.Location = new Point(0, top + 160);

        CenterHorizontally(_nameBox, top + 200);
        CenterHorizontally(_saveButton, top + 240);

        _savedLabel.Size = new Size(Width, 24);
        _savedLabel.Location = new Point(0, top + 292);

        CenterHorizontally(_retry, top + 410);
        CenterHorizontally(_menu, top + 470);
    }

    public void SetScore(int score)
    {
        _pendingScore = score;
        _qualifies = HighScoreService.QualifiesForTop(score);
        _saved = false;

        _scoreLabel.Text = $"Puntuación final: {score} pts";
        _nameBox.Text = "Trepamuros";
        _newRecordLabel.Visible = _qualifies;
        _nameBox.Visible = _qualifies;
        _saveButton.Visible = _qualifies;
        _savedLabel.Visible = false;

        if (_qualifies)
        {
            _nameBox.Focus();
            _nameBox.SelectAll();
        }
    }

    private void SavePendingScore()
    {
        if (_saved) return;
        string name = string.IsNullOrWhiteSpace(_nameBox.Text) ? "Trepamuros" : _nameBox.Text.Trim();
        HighScoreService.Save(new HighScoreEntry { Name = name, Score = _pendingScore, Date = DateTime.Now });
        _saved = true;
        _saveButton.Visible = false;
        _nameBox.Visible = false;
        _savedLabel.Visible = true;
    }
}
