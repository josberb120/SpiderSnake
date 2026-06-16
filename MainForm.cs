using SpiderSnake.Game;
using SpiderSnake.Models;
using SpiderSnake.Screens;
using SpiderSnake.Services;
using SpiderSnake.Theme;

namespace SpiderSnake;

/// <summary>
/// Ventana única ("shell") que alterna entre las distintas pantallas
/// (UserControls) y reproduce una transición tipo "cortina de cómic" al
/// cambiar de una a otra.
/// </summary>
internal class MainForm : Form
{
    private readonly GameSettings _settings = SettingsService.Load();

    private readonly SplashScreen _splash;
    private readonly MainMenuScreen _menu;
    private readonly InstructionsScreen _instructions;
    private readonly HighScoresScreen _highScores;
    private readonly SettingsScreen _settingsScreen;
    private readonly GameOverScreen _gameOver;
    private readonly GameScreen _game;
    private readonly CurtainOverlay _curtain = new();

    private Control? _current;

    public MainForm()
    {
        Text = "Spider-Snake — el trepamuros hambriento";
        ClientSize = new Size(AppLayout.ScreenWidth, AppLayout.ScreenHeight);
        MinimumSize = new Size(AppLayout.ScreenWidth, AppLayout.ScreenHeight) + (Size - ClientSize);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = SpideyTheme.NearBlack;

        _splash = new SplashScreen();
        _menu = new MainMenuScreen();
        _instructions = new InstructionsScreen();
        _highScores = new HighScoresScreen();
        _settingsScreen = new SettingsScreen(_settings);
        _gameOver = new GameOverScreen();
        _game = new GameScreen(_settings);

        WireUpNavigation();

        Controls.Add(_curtain);
        _curtain.Bounds = new Rectangle(0, 0, AppLayout.ScreenWidth, AppLayout.ScreenHeight);
        _curtain.Visible = false;

        SwitchTo(_splash);
    }

    private void WireUpNavigation()
    {
        _splash.Finished += () => SwitchTo(_menu);

        _menu.PlayClicked += () => { _game.StartNewGame(); SwitchTo(_game); };
        _menu.InstructionsClicked += () => SwitchTo(_instructions);
        _menu.HighScoresClicked += () => { _highScores.RefreshScores(); SwitchTo(_highScores); };
        _menu.SettingsClicked += () => SwitchTo(_settingsScreen);
        _menu.ExitClicked += ConfirmExit;

        _instructions.BackClicked += () => SwitchTo(_menu);
        _highScores.BackClicked += () => SwitchTo(_menu);
        _settingsScreen.BackClicked += () => SwitchTo(_menu);

        _game.GameEnded += score => { _gameOver.SetScore(score); SwitchTo(_gameOver); };
        _game.BackToMenuRequested += () => { _game.StopGame(); SwitchTo(_menu); };

        _gameOver.RetryClicked += () => { _game.StartNewGame(); SwitchTo(_game); };
        _gameOver.MenuClicked += () => SwitchTo(_menu);
    }

    private void ConfirmExit()
    {
        var result = MessageBox.Show(this, "¿Seguro que quieres salir, trepamuros?", "Spider-Snake",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (result == DialogResult.Yes) Application.Exit();
    }

    private void SwitchTo(Control next)
    {
        if (_current == next) return;

        _curtain.Cover(() =>
        {
            if (_current != null) Controls.Remove(_current);
            next.Dock = DockStyle.Fill;
            Controls.Add(next);
            next.BringToFront();
            _current = next;
            next.Focus();
        });
    }

    /// <summary>
    /// Overlay sólido que cubre toda la pantalla, hace el cambio de control
    /// mientras está cubierto, y luego se "abre" deslizándose hacia un lado
    /// como una cortina/panel de cómic, revelando la nueva pantalla.
    /// </summary>
    private class CurtainOverlay : Control
    {
        private const int DurationMs = 260;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 12 };
        private int _elapsedMs;
        private int _fullWidth;

        public CurtainOverlay()
        {
            BackColor = SpideyTheme.NearBlack;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            _timer.Tick += (_, _) => Advance();
        }

        public void Cover(Action swapAction)
        {
            // Recalcula el tamaño contra el padre por si la ventana fue redimensionada
            // desde la última transición (la cortina no usa Dock para poder animarse).
            if (Parent != null) Bounds = Parent.ClientRectangle;
            _fullWidth = Width;
            Left = 0;
            Visible = true;
            BringToFront();
            Refresh(); // pinta la cortina cerrada antes de hacer el cambio, para que no se note el "salto"
            swapAction();
            _elapsedMs = 0;
            _timer.Start();
        }

        private void Advance()
        {
            _elapsedMs += _timer.Interval;
            double t = Math.Min(1.0, _elapsedMs / (double)DurationMs);
            double eased = 1 - Math.Pow(1 - t, 3); // ease-out cúbico
            Left = (int)(_fullWidth * eased);

            if (t >= 1.0)
            {
                _timer.Stop();
                Visible = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var pen = new Pen(SpideyTheme.GoldAccent, 2f);
            e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
            SpideyTheme.PaintWebPattern(e.Graphics, ClientRectangle);
        }
    }
}
