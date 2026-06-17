using SpiderSnake.Controls;
using SpiderSnake.Theme;

namespace SpiderSnake.Screens;

internal class MainMenuScreen : ThemedScreenBase
{
    public event Action? PlayClicked;
    public event Action? InstructionsClicked;
    public event Action? HighScoresClicked;
    public event Action? SettingsClicked;
    public event Action? ExitClicked;

    private readonly Label _title;
    private readonly Label _subtitle;
    private readonly Label _footer;
    private readonly List<SpideyButton> _buttons = new();

    public MainMenuScreen()
    {
        Accent = SpideyTheme.SpideyRed;
        AccentDark = SpideyTheme.SpideyBlueDark;

        _title = MakeTitle("SPIDER-SNAKE", 52f, Color.White);
        _subtitle = MakeLabel("¡A comer arañas por las calles de Nueva York!", 12f, SpideyTheme.GoldAccent, FontStyle.Italic);
        Controls.Add(_title);
        Controls.Add(_subtitle);

        AddMenuButton("▶  JUGAR", SpideyTheme.SpideyRed, SpideyTheme.SpideyRedDark, () => PlayClicked?.Invoke());
        AddMenuButton("❓  CÓMO JUGAR", SpideyTheme.SpideyBlue, SpideyTheme.SpideyBlueDark, () => InstructionsClicked?.Invoke());
        AddMenuButton("★  PUNTUACIONES", SpideyTheme.SpideyBlue, SpideyTheme.SpideyBlueDark, () => HighScoresClicked?.Invoke());
        AddMenuButton("⚙  AJUSTES", SpideyTheme.SpideyBlue, SpideyTheme.SpideyBlueDark, () => SettingsClicked?.Invoke());
        AddMenuButton("✕  SALIR", Color.FromArgb(70, 70, 70), Color.FromArgb(25, 25, 25), () => ExitClicked?.Invoke());

        _footer = MakeLabel("Autor: Josber Betancourt", 8f, Color.FromArgb(140, 255, 255, 255));
        Controls.Add(_footer);

        Reflow();
    }

    protected override void Reflow()
    {
        CenterHorizontally(_title, Height / 2 - 235);
        CenterHorizontally(_subtitle, Height / 2 - 142);

        const int spacing = 62;
        int startY = Height / 2 - 95;
        for (int i = 0; i < _buttons.Count; i++)
        {
            CenterHorizontally(_buttons[i], startY + spacing * i);
        }

        CenterHorizontally(_footer, Height - 28);
    }

    private void AddMenuButton(string text, Color accent, Color accentDark, Action onClick)
    {
        var button = new SpideyButton
        {
            Text = text,
            Size = new Size(280, 48),
            AccentColor = accent,
            AccentColorDark = accentDark,
        };
        button.Click += (_, _) => onClick();
        Controls.Add(button);
        _buttons.Add(button);
    }
}
