using SpiderSnake.Controls;
using SpiderSnake.Models;
using SpiderSnake.Services;
using SpiderSnake.Theme;

namespace SpiderSnake.Screens;

internal class SettingsScreen : ThemedScreenBase
{
    private const int DifficultyButtonWidth = 160;
    private const int DifficultyButtonGap = 20;

    public event Action? BackClicked;

    private readonly GameSettings _settings;
    private readonly List<(Difficulty Value, SpideyButton Button)> _difficultyButtons = new();
    private readonly Label _title;
    private readonly Label _difficultyLabel;
    private readonly Label _soundLabel;
    private readonly SpideyButton _soundButton;
    private readonly SpideyButton _back;

    public SettingsScreen(GameSettings settings)
    {
        _settings = settings;
        Accent = SpideyTheme.SpideyBlue;
        AccentDark = SpideyTheme.NearBlack;

        _title = MakeTitle("AJUSTES", 40f, Color.White);
        Controls.Add(_title);

        _difficultyLabel = MakeSectionLabel("DIFICULTAD");
        Controls.Add(_difficultyLabel);

        AddDifficultyButton(Difficulty.Facil, "FÁCIL");
        AddDifficultyButton(Difficulty.Normal, "NORMAL");
        AddDifficultyButton(Difficulty.Dificil, "DIFÍCIL");

        _soundLabel = MakeSectionLabel("SONIDO");
        Controls.Add(_soundLabel);

        _soundButton = new SpideyButton { Size = new Size(220, 50) };
        _soundButton.Click += (_, _) =>
        {
            _settings.SoundEnabled = !_settings.SoundEnabled;
            SettingsService.Save(_settings);
            UpdateSoundButton();
        };
        Controls.Add(_soundButton);
        UpdateSoundButton();
        UpdateDifficultyButtons();

        _back = new SpideyButton
        {
            Text = "← VOLVER",
            Size = new Size(180, 46),
            AccentColor = SpideyTheme.SpideyBlue,
            AccentColorDark = SpideyTheme.SpideyBlueDark,
        };
        _back.Click += (_, _) => BackClicked?.Invoke();
        Controls.Add(_back);

        Reflow();
    }

    protected override void Reflow()
    {
        _title.Size = new Size(Width, 60);
        _title.Location = new Point(0, 46);

        _difficultyLabel.Size = new Size(Width, 24);
        _difficultyLabel.Location = new Point(0, 146);

        int totalWidth = DifficultyButtonWidth * 3 + DifficultyButtonGap * 2;
        int startX = (Width - totalWidth) / 2;
        for (int i = 0; i < _difficultyButtons.Count; i++)
        {
            _difficultyButtons[i].Button.Location = new Point(startX + i * (DifficultyButtonWidth + DifficultyButtonGap), 186);
        }

        _soundLabel.Size = new Size(Width, 24);
        _soundLabel.Location = new Point(0, 276);

        CenterHorizontally(_soundButton, 316);
        CenterHorizontally(_back, Height - 78);
    }

    private static Label MakeSectionLabel(string text) => new()
    {
        Text = text,
        Font = SpideyTheme.BodyFont(13f, FontStyle.Bold),
        ForeColor = SpideyTheme.GoldAccent,
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleCenter,
        BackColor = Color.Transparent,
    };

    private void AddDifficultyButton(Difficulty difficulty, string text)
    {
        var button = new SpideyButton { Text = text, Size = new Size(DifficultyButtonWidth, 46) };
        button.Click += (_, _) =>
        {
            _settings.Difficulty = difficulty;
            SettingsService.Save(_settings);
            UpdateDifficultyButtons();
        };
        Controls.Add(button);
        _difficultyButtons.Add((difficulty, button));
    }

    private void UpdateDifficultyButtons()
    {
        foreach (var (value, button) in _difficultyButtons)
        {
            bool selected = value == _settings.Difficulty;
            button.AccentColor = selected ? SpideyTheme.SpideyRed : Color.FromArgb(55, 55, 60);
            button.AccentColorDark = selected ? SpideyTheme.SpideyRedDark : Color.FromArgb(20, 20, 22);
            button.Invalidate();
        }
    }

    private void UpdateSoundButton()
    {
        _soundButton.Text = _settings.SoundEnabled ? "♪ SONIDO: ACTIVADO" : "✕ SONIDO: DESACTIVADO";
        _soundButton.AccentColor = _settings.SoundEnabled ? SpideyTheme.SpideyRed : Color.FromArgb(55, 55, 60);
        _soundButton.AccentColorDark = _settings.SoundEnabled ? SpideyTheme.SpideyRedDark : Color.FromArgb(20, 20, 22);
        _soundButton.Invalidate();
    }
}
