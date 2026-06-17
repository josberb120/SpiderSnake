using SpiderSnake.Theme;

namespace SpiderSnake.Controls;

/// <summary>Overlay semitransparente que se muestra sobre el juego al pausar.</summary>
internal class PauseOverlay : UserControl
{
    public event EventHandler? ResumeClicked;
    public event EventHandler? RestartClicked;
    public event EventHandler? MenuClicked;

    private readonly Label _title;
    private readonly SpideyButton _resumeButton;
    private readonly SpideyButton _restartButton;
    private readonly SpideyButton _menuButton;

    public PauseOverlay()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        BackColor = Color.Black;

        _title = new Label
        {
            Text = "‖ EN PAUSA",
            Font = SpideyTheme.TitleFont(30f),
            ForeColor = SpideyTheme.GoldAccent,
            AutoSize = true,
            UseCompatibleTextRendering = true,
            Padding = new Padding(0, 4, 0, 6),
            BackColor = Color.Transparent,
        };

        _resumeButton = NewButton("REANUDAR", SpideyTheme.SpideyRed, SpideyTheme.SpideyRedDark);
        _restartButton = NewButton("REINICIAR", SpideyTheme.SpideyBlue, SpideyTheme.SpideyBlueDark);
        _menuButton = NewButton("MENÚ PRINCIPAL", Color.FromArgb(60, 60, 60), Color.FromArgb(25, 25, 25));

        _resumeButton.Click += (_, e) => ResumeClicked?.Invoke(this, e);
        _restartButton.Click += (_, e) => RestartClicked?.Invoke(this, e);
        _menuButton.Click += (_, e) => MenuClicked?.Invoke(this, e);

        Controls.Add(_title);
        Controls.Add(_resumeButton);
        Controls.Add(_restartButton);
        Controls.Add(_menuButton);

        Resize += (_, _) => LayoutControls();
        LayoutControls();
    }

    private static SpideyButton NewButton(string text, Color accent, Color accentDark) => new()
    {
        Text = text,
        Size = new Size(220, 48),
        AccentColor = accent,
        AccentColorDark = accentDark,
    };

    private void LayoutControls()
    {
        int centerX = Width / 2;
        _title.Location = new Point(centerX - _title.Width / 2, Height / 2 - 150);

        _resumeButton.Location = new Point(centerX - _resumeButton.Width / 2, Height / 2 - 60);
        _restartButton.Location = new Point(centerX - _restartButton.Width / 2, Height / 2);
        _menuButton.Location = new Point(centerX - _menuButton.Width / 2, Height / 2 + 60);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var overlay = new SolidBrush(Color.FromArgb(195, 8, 6, 10));
        e.Graphics.FillRectangle(overlay, ClientRectangle);
        SpideyTheme.PaintWebPattern(e.Graphics, ClientRectangle);
        base.OnPaint(e);
    }
}
