using SpiderSnake.Controls;
using SpiderSnake.Theme;

namespace SpiderSnake.Screens;

internal class InstructionsScreen : ThemedScreenBase
{
    private const int CardWidth = 640;
    private const int CardHeight = 380;

    public event Action? BackClicked;

    private readonly Label _title;
    private readonly Panel _card;
    private readonly SpideyButton _back;

    public InstructionsScreen()
    {
        Accent = SpideyTheme.SpideyBlue;
        AccentDark = SpideyTheme.NearBlack;

        _title = MakeTitle("CÓMO JUGAR", 38f, Color.White);
        Controls.Add(_title);

        _card = new Panel { BackColor = Color.Transparent, Size = new Size(CardWidth, CardHeight) };
        _card.Paint += (_, e) => SpideyTheme.PaintCard(e.Graphics, _card.ClientRectangle);
        Controls.Add(_card);

        AddLine("►  CONTROLES", true, 16);
        AddLine("Flechas direccionales o W A S D para moverte.", false, 46);
        AddLine("P o ESC para pausar la partida en cualquier momento.", false, 70);

        AddLine("►  OBJETIVO", true, 112);
        AddLine("Guía a tu serpiente arácnida y come tantas arañas como puedas", false, 142);
        AddLine("sin chocar contra los bordes ni contra tu propio cuerpo.", false, 164);

        AddLine("★  PUNTAJE", true, 206);
        AddLine("Araña normal = 10 puntos.", false, 236);
        AddLine("Araña dorada (aparece de vez en cuando) = 30 puntos,", false, 258);
        AddLine("¡pero se escapa si tardas demasiado en atraparla!", false, 280);

        AddLine("⚡  DIFICULTAD", true, 322);
        AddLine("La serpiente acelera a medida que comes más arañas.", false, 352);

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
        // Centra título + tarjeta + botón como un solo bloque (antes el botón se anclaba al
        // fondo real de la ventana mientras la tarjeta quedaba fija arriba).
        const int blockHeight = 556;
        int top = Math.Max(32, (Height - blockHeight) / 2);

        CenterHorizontally(_title, top);
        _card.Location = new Point((Width - _card.Width) / 2, top + 72);
        CenterHorizontally(_back, top + 510);
    }

    private void AddLine(string text, bool isHeader, int top)
    {
        var label = new Label
        {
            Text = text,
            Font = isHeader ? SpideyTheme.BodyFont(13f, FontStyle.Bold) : SpideyTheme.BodyFont(11f),
            ForeColor = isHeader ? SpideyTheme.GoldAccent : Color.White,
            AutoSize = false,
            UseCompatibleTextRendering = true,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            Location = new Point(24, top),
            Size = new Size(CardWidth - 48, 28),
        };
        _card.Controls.Add(label);
    }
}
