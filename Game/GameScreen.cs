using System.Drawing.Drawing2D;
using System.Media;
using SpiderSnake.Controls;
using SpiderSnake.Models;
using SpiderSnake.Theme;

namespace SpiderSnake.Game;

/// <summary>
/// Pantalla jugable: dibuja el grid, la serpiente y las arañas, captura el
/// teclado y maneja la pausa. La lógica de juego en sí vive en <see cref="GameEngine"/>.
/// El tamaño de celda se recalcula en cada pintado a partir del tamaño actual
/// del canvas, así el tablero se adapta al redimensionar/maximizar la ventana.
/// </summary>
internal class GameScreen : UserControl
{
    private const int GridWidth = AppLayout.GridWidth;
    private const int GridHeight = AppLayout.GridHeight;
    private const int TopBarHeight = AppLayout.TopBarHeight;
    private const int MinCellSize = 12;
    private const int MaxCellSize = 44;

    public event Action<int>? GameEnded;
    public event Action? BackToMenuRequested;

    private const double PopupLifeMs = 850;
    private const double MilestoneDurationMs = 1300;
    private const int MilestoneStep = 100;

    private readonly GameEngine _engine = new(GridWidth, GridHeight);
    private readonly GameSettings _settings;
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly System.Windows.Forms.Timer _fxTimer = new() { Interval = 33 };
    private readonly GameCanvas _canvas = new();
    private readonly Label _scoreLabel;
    private readonly Label _bestLabel;
    private readonly Label _streakLabel;
    private readonly SpideyButton _pauseButton;
    private readonly PauseOverlay _pauseOverlay;
    private readonly List<ScorePopup> _popups = new();
    private bool _isPaused;
    private bool _started;
    private int _lastMilestoneReached;
    private string? _milestoneText;
    private double _milestoneAgeMs;
    private Bitmap? _backdropCache;

    private class ScorePopup
    {
        public required PointF Position;
        public required string Text;
        public required Color Color;
        public double AgeMs;
    }

    public GameScreen(GameSettings settings)
    {
        _settings = settings;
        Dock = DockStyle.Fill;
        BackColor = SpideyTheme.NearBlack;
        TabStop = true;

        var topBar = new Panel { Dock = DockStyle.Top, Height = TopBarHeight, BackColor = SpideyTheme.SpideyBlueDark };
        topBar.Paint += (_, e) =>
        {
            using var pen = new Pen(SpideyTheme.GoldAccent, 2f);
            e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
        };

        _scoreLabel = new Label
        {
            Text = "PUNTOS: 0",
            Font = SpideyTheme.TitleFont(16f),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(16, 14),
            BackColor = Color.Transparent,
        };
        _bestLabel = new Label
        {
            Text = $"MEJOR: {Services.HighScoreService.BestScore()}",
            Font = SpideyTheme.BodyFont(11f, FontStyle.Bold),
            ForeColor = SpideyTheme.GoldAccent,
            AutoSize = true,
            Location = new Point(20, 38),
            BackColor = Color.Transparent,
        };
        _streakLabel = new Label
        {
            Text = "",
            Font = SpideyTheme.BodyFont(13f, FontStyle.Bold),
            ForeColor = SpideyTheme.GoldAccent,
            AutoSize = true,
            Visible = false,
            BackColor = Color.Transparent,
        };
        _pauseButton = new SpideyButton
        {
            Text = "‖ PAUSA",
            Size = new Size(110, 38),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            AccentColor = SpideyTheme.SpideyBlue,
            AccentColorDark = SpideyTheme.SpideyBlueDark,
        };
        _pauseButton.Click += (_, _) => TogglePause();

        topBar.Controls.Add(_scoreLabel);
        topBar.Controls.Add(_bestLabel);
        topBar.Controls.Add(_streakLabel);
        topBar.Controls.Add(_pauseButton);
        void ReflowTopBar()
        {
            _pauseButton.Location = new Point(topBar.Width - _pauseButton.Width - 16, 11);
            _streakLabel.Location = new Point((topBar.Width - _streakLabel.Width) / 2, 12);
        }
        topBar.Resize += (_, _) => ReflowTopBar();
        ReflowTopBar();

        _canvas.Dock = DockStyle.Fill;
        _canvas.PaintCallback = PaintGame;
        _canvas.BackColor = Color.FromArgb(16, 14, 18);

        _pauseOverlay = new PauseOverlay
        {
            Dock = DockStyle.Fill,
            Visible = false,
        };
        _pauseOverlay.ResumeClicked += (_, _) => TogglePause();
        _pauseOverlay.RestartClicked += (_, _) => { TogglePause(); StartNewGame(); };
        _pauseOverlay.MenuClicked += (_, _) => BackToMenuRequested?.Invoke();

        Controls.Add(_pauseOverlay);
        Controls.Add(_canvas);
        Controls.Add(topBar);
        _pauseOverlay.BringToFront();

        _timer.Tick += (_, _) => OnTick();
        _fxTimer.Tick += (_, _) => OnFxTick();
    }

    public void StartNewGame()
    {
        _engine.Reset();
        _isPaused = false;
        _pauseOverlay.Visible = false;
        _started = true;
        _scoreLabel.Text = "PUNTOS: 0";
        _bestLabel.Text = $"MEJOR: {Services.HighScoreService.BestScore()}";
        _streakLabel.Visible = false;
        _popups.Clear();
        _milestoneText = null;
        _lastMilestoneReached = 0;
        _timer.Interval = _settings.StartingIntervalMs;
        _timer.Start();
        _fxTimer.Start();
        _canvas.Invalidate();
        Focus();
    }

    public void StopGame()
    {
        _timer.Stop();
        _fxTimer.Stop();
    }

    private void TogglePause()
    {
        if (!_started) return;
        _isPaused = !_isPaused;
        _pauseOverlay.Visible = _isPaused;
        if (_isPaused)
        {
            _timer.Stop();
            _fxTimer.Stop();
            _pauseOverlay.BringToFront();
        }
        else
        {
            _timer.Start();
            _fxTimer.Start();
            Focus();
        }
    }

    private void OnTick()
    {
        var result = _engine.Tick();
        switch (result)
        {
            case TickResult.AteSpider:
            case TickResult.AteGoldenSpider:
                bool golden = result == TickResult.AteGoldenSpider;
                _scoreLabel.Text = $"PUNTOS: {_engine.Score}";
                if (_settings.SoundEnabled) (golden ? SystemSounds.Exclamation : SystemSounds.Asterisk).Play();
                _timer.Interval = _engine.CurrentIntervalMs(_settings.StartingIntervalMs, _settings.MinIntervalMs);
                SpawnScorePopup(golden);
                UpdateStreakLabel();
                CheckMilestone();
                break;
            case TickResult.GameOver:
                _timer.Stop();
                _fxTimer.Stop();
                _started = false;
                if (_settings.SoundEnabled) SystemSounds.Hand.Play();
                GameEnded?.Invoke(_engine.Score);
                return;
        }
        _canvas.Invalidate();
    }

    private void SpawnScorePopup(bool golden)
    {
        var (cellSize, offsetX, offsetY) = ComputeGridMetrics();
        var cell = _engine.Snake[0]; // la cabeza acaba de entrar a la celda donde estaba la araña comida
        var center = new PointF(
            offsetX + cell.X * cellSize + cellSize / 2f,
            offsetY + cell.Y * cellSize + cellSize / 2f);

        string text = $"+{_engine.LastPointsAwarded}";
        if (golden) text += " ¡DORADA!";
        else if (_engine.LastComboBonus > 0) text += $" RACHA x{_engine.Streak}";

        _popups.Add(new ScorePopup
        {
            Position = center,
            Text = text,
            Color = golden ? SpideyTheme.GoldAccent : Color.White,
        });
    }

    private void UpdateStreakLabel()
    {
        _streakLabel.Visible = _engine.Streak >= 3;
        if (!_streakLabel.Visible) return;

        _streakLabel.Text = $"⚡ RACHA x{_engine.Streak}";
        var parent = _streakLabel.Parent!;
        _streakLabel.Location = new Point((parent.Width - _streakLabel.Width) / 2, 12);
    }

    private void CheckMilestone()
    {
        int reached = (_engine.Score / MilestoneStep) * MilestoneStep;
        if (reached > _lastMilestoneReached && reached > 0)
        {
            _lastMilestoneReached = reached;
            _milestoneText = $"¡{reached} PUNTOS, TREPAMUROS!";
            _milestoneAgeMs = 0;
        }
    }

    private void OnFxTick()
    {
        bool any = false;
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            _popups[i].AgeMs += _fxTimer.Interval;
            if (_popups[i].AgeMs >= PopupLifeMs) _popups.RemoveAt(i);
            else any = true;
        }

        if (_milestoneText != null)
        {
            _milestoneAgeMs += _fxTimer.Interval;
            if (_milestoneAgeMs >= MilestoneDurationMs) _milestoneText = null;
            else any = true;
        }

        if (any) _canvas.Invalidate();
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(keyData);

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (HandleDirectionKey(keyData)) return true;
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (HandleDirectionKey(e.KeyCode))
        {
            e.Handled = true;
            return;
        }

        if (e.KeyCode is Keys.P or Keys.Escape)
        {
            TogglePause();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private bool HandleDirectionKey(Keys key)
    {
        if (_isPaused || !_started) return false;
        switch (key)
        {
            case Keys.Up or Keys.W: _engine.ChangeDirection(Direction.Up); return true;
            case Keys.Down or Keys.S: _engine.ChangeDirection(Direction.Down); return true;
            case Keys.Left or Keys.A: _engine.ChangeDirection(Direction.Left); return true;
            case Keys.Right or Keys.D: _engine.ChangeDirection(Direction.Right); return true;
            default: return false;
        }
    }

    /// <summary>
    /// Calcula el tamaño de celda y el margen de centrado a partir del tamaño
    /// actual del canvas, para que el tablero se vea bien sin importar cómo
    /// quede la ventana al redimensionarla.
    /// </summary>
    private (int cellSize, int offsetX, int offsetY) ComputeGridMetrics()
    {
        var size = _canvas.ClientSize;
        int cellSize = Math.Min(size.Width / GridWidth, size.Height / GridHeight);
        cellSize = Math.Clamp(cellSize, MinCellSize, MaxCellSize);
        int offsetX = (size.Width - cellSize * GridWidth) / 2;
        int offsetY = (size.Height - cellSize * GridHeight) / 2;
        return (cellSize, offsetX, offsetY);
    }

    /// <summary>
    /// Regenera el bitmap de fondo (degradado + telaraña + marco del tablero) sólo cuando el
    /// tamaño del canvas cambió respecto a la última vez. Evita repintar gradientes y arcos en
    /// cada tick del juego, lo cual es notablemente más pesado en gráficas integradas modestas
    /// (p. ej. Intel HD Graphics) que un simple "blit" del bitmap ya calculado.
    /// </summary>
    private void EnsureBackdropCache()
    {
        var size = _canvas.ClientSize;
        if (size.Width <= 0 || size.Height <= 0) return;
        if (_backdropCache != null && _backdropCache.Size == size) return;

        _backdropCache?.Dispose();
        _backdropCache = new Bitmap(size.Width, size.Height);
        using var bg = Graphics.FromImage(_backdropCache);
        bg.SmoothingMode = SmoothingMode.AntiAlias;
        SpideyTheme.PaintBackdrop(bg, new Rectangle(Point.Empty, size), SpideyTheme.SpideyBlue, SpideyTheme.SpideyBlueDark);

        var (cellSize, offsetX, offsetY) = ComputeGridMetrics();
        var frameRect = new Rectangle(offsetX - 10, offsetY - 10, GridWidth * cellSize + 20, GridHeight * cellSize + 20);
        using var framePath = RoundedRect(frameRect, 16);
        using var frameFill = new SolidBrush(Color.FromArgb(150, 10, 8, 12));
        bg.FillPath(frameFill, framePath);
        using var framePen = new Pen(SpideyTheme.GoldAccent, 2f);
        bg.DrawPath(framePen, framePath);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _backdropCache?.Dispose();
        base.Dispose(disposing);
    }

    private void PaintGame(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // El fondo (degradado + telaraña + marco) sólo cambia si se redimensiona la ventana,
        // pero el tablero se invalida en cada tick del juego. Recalcular ese degradado a mano
        // (con sus blends y arcos) en cada tick es caro para una GPU integrada modesta; lo
        // cacheamos en un bitmap y aquí sólo se vuelca ("blit"), que es prácticamente gratis.
        EnsureBackdropCache();
        g.DrawImageUnscaled(_backdropCache!, 0, 0);

        var (cellSize, offsetX, offsetY) = ComputeGridMetrics();

        using var gridPen = new Pen(Color.FromArgb(18, 255, 255, 255), 1f);
        for (int x = 0; x <= GridWidth; x++)
            g.DrawLine(gridPen, offsetX + x * cellSize, offsetY, offsetX + x * cellSize, offsetY + GridHeight * cellSize);
        for (int y = 0; y <= GridHeight; y++)
            g.DrawLine(gridPen, offsetX, offsetY + y * cellSize, offsetX + GridWidth * cellSize, offsetY + y * cellSize);

        // Araña (comida)
        var spiderCenter = new Point(
            offsetX + _engine.SpiderPosition.X * cellSize + cellSize / 2,
            offsetY + _engine.SpiderPosition.Y * cellSize + cellSize / 2);
        SpideyTheme.PaintSpider(g, spiderCenter, (int)(cellSize * 0.9), _engine.IsGoldenSpider);

        // Serpiente: cuerpo alternado rojo/azul tipo traje, cabeza con ojos blancos.
        for (int i = _engine.Snake.Count - 1; i >= 0; i--)
        {
            var cell = _engine.Snake[i];
            var rect = new Rectangle(
                offsetX + cell.X * cellSize + 1, offsetY + cell.Y * cellSize + 1, cellSize - 2, cellSize - 2);
            bool isHead = i == 0;
            Color color = isHead ? SpideyTheme.SpideyRed : (i % 2 == 0 ? SpideyTheme.SpideyRed : SpideyTheme.SpideyBlue);

            using var path = RoundedRect(rect, Math.Max(3, cellSize / 3));
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
            using var pen = new Pen(Color.FromArgb(160, 0, 0, 0), 1f);
            g.DrawPath(pen, path);

            if (isHead)
            {
                using var web = new Pen(Color.FromArgb(120, 0, 0, 0), 1f);
                g.DrawLine(web, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2);
                g.DrawLine(web, rect.Left + rect.Width / 2, rect.Top, rect.Left + rect.Width / 2, rect.Bottom);

                int eyeSize = Math.Max(3, cellSize / 3);
                using var eyeBrush = new SolidBrush(Color.White);
                g.FillEllipse(eyeBrush, rect.Left + 2, rect.Top + 2, eyeSize, eyeSize);
                g.FillEllipse(eyeBrush, rect.Right - eyeSize - 2, rect.Top + 2, eyeSize, eyeSize);
            }
        }

        PaintScorePopups(g);
        PaintMilestoneBanner(g);
    }

    /// <summary>"+10"/"+30 ¡DORADA!" flotantes que suben y se desvanecen donde se comió la araña.</summary>
    private void PaintScorePopups(Graphics g)
    {
        using var font = SpideyTheme.BodyFont(13f, FontStyle.Bold);
        foreach (var popup in _popups)
        {
            double t = Math.Clamp(popup.AgeMs / PopupLifeMs, 0, 1);
            int alpha = (int)(255 * (1 - t));
            float riseY = (float)(t * 38);

            var pos = new PointF(popup.Position.X, popup.Position.Y - riseY - 14);
            using var brush = new SolidBrush(Color.FromArgb(alpha, popup.Color));
            using var shadowBrush = new SolidBrush(Color.FromArgb(alpha / 2, 0, 0, 0));
            using var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(popup.Text, font, shadowBrush, pos.X + 1, pos.Y + 1, sf);
            g.DrawString(popup.Text, font, brush, pos.X, pos.Y, sf);
        }
    }

    /// <summary>Banner grande que celebra cada 100 puntos alcanzados, estilo viñeta de cómic.</summary>
    private void PaintMilestoneBanner(Graphics g)
    {
        if (_milestoneText == null) return;

        double t = Math.Clamp(_milestoneAgeMs / MilestoneDurationMs, 0, 1);
        double fade = t < 0.15 ? t / 0.15 : t > 0.75 ? (1 - t) / 0.25 : 1;
        int alpha = (int)(220 * Math.Clamp(fade, 0, 1));
        if (alpha <= 0) return;

        using var font = SpideyTheme.TitleFont(22f);
        var size = g.MeasureString(_milestoneText, font);
        var bounds = _canvas.ClientRectangle;
        var bannerRect = new RectangleF((bounds.Width - size.Width) / 2 - 24, bounds.Height * 0.18f - 14, size.Width + 48, size.Height + 28);

        using var path = RoundedRect(Rectangle.Round(bannerRect), 18);
        using var fill = new SolidBrush(Color.FromArgb(alpha * 150 / 220, 10, 8, 12));
        g.FillPath(fill, path);
        using var border = new Pen(Color.FromArgb(alpha, SpideyTheme.GoldAccent), 2.5f);
        g.DrawPath(border, path);

        using var textBrush = new SolidBrush(Color.FromArgb(alpha, SpideyTheme.GoldAccent));
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(_milestoneText, font, textBrush, bannerRect, sf);
    }

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private class GameCanvas : Panel
    {
        public Action<Graphics>? PaintCallback;

        public GameCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e) => PaintCallback?.Invoke(e.Graphics);
    }
}
