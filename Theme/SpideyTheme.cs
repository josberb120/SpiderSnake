using System.Drawing.Drawing2D;

namespace SpiderSnake.Theme;

/// <summary>
/// Paleta de colores, fuentes y helpers de dibujo compartidos por toda la UI.
/// Todo el arte de este juego es generado por código (GDI+): no se usa ningún
/// logo ni asset oficial de Marvel/Spider-Man, solo una estética propia
/// inspirada en arañas y telarañas (rojo/azul, comic, web pattern).
/// </summary>
internal static class SpideyTheme
{
    public static readonly Color SpideyRed = Color.FromArgb(230, 36, 41);
    public static readonly Color SpideyRedDark = Color.FromArgb(120, 14, 18);
    public static readonly Color SpideyBlue = Color.FromArgb(27, 58, 140);
    public static readonly Color SpideyBlueDark = Color.FromArgb(10, 20, 55);
    public static readonly Color NearBlack = Color.FromArgb(12, 10, 14);
    public static readonly Color WebGray = Color.FromArgb(235, 235, 235);
    public static readonly Color GoldAccent = Color.FromArgb(255, 200, 40);

    public static Font TitleFont(float size) => new("Impact", size, FontStyle.Regular, GraphicsUnit.Point);

    public static Font BodyFont(float size, FontStyle style = FontStyle.Regular) =>
        new("Segoe UI", size, style, GraphicsUnit.Point);

    /// <summary>
    /// Único degradado de fondo de la app: una transición diagonal suave de tres
    /// paradas (acento → casi-negro → negro) para dar profundidad sin apilar
    /// varios degradados encima. El resto de la pantalla es color plano + telaraña.
    /// </summary>
    public static void PaintBackdrop(Graphics g, Rectangle bounds, Color accent, Color accentDark)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var bg = new LinearGradientBrush(bounds, accentDark, NearBlack, 50f);
        bg.Blend = new Blend(3)
        {
            Positions = new[] { 0f, 0.45f, 1f },
            Factors = new[] { 1f, 0.55f, 0f },
        };
        g.FillRectangle(bg, bounds);

        PaintWebPattern(g, bounds);
    }

    /// <summary>
    /// Panel translúcido con esquinas redondeadas y borde dorado, usado para las
    /// "tarjetas" de contenido (instrucciones, puntuaciones). Sin degradado:
    /// color plano para mantener la estética limpia y coherente con los botones.
    /// </summary>
    public static void PaintCard(Graphics g, Rectangle bounds)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        using var path = RoundedRectPath(rect, 16);
        using var fill = new SolidBrush(Color.FromArgb(150, 10, 8, 12));
        g.FillPath(fill, path);
        using var pen = new Pen(GoldAccent, 1.5f);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedRectPath(Rectangle r, int radius)
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

    /// <summary>Dibuja una telaraña (líneas radiales + arcos concéntricos) desde una esquina.</summary>
    public static void PaintWebPattern(Graphics g, Rectangle bounds)
    {
        using var pen = new Pen(Color.FromArgb(28, 255, 255, 255), 1.4f);
        DrawWebFromCorner(g, pen, new Point(bounds.Left - 40, bounds.Top - 40), Math.Max(bounds.Width, bounds.Height));
        DrawWebFromCorner(g, pen, new Point(bounds.Right + 40, bounds.Bottom + 40), Math.Max(bounds.Width, bounds.Height) / 2);
    }

    private static void DrawWebFromCorner(Graphics g, Pen pen, Point center, int radius)
    {
        const int rays = 10;
        for (int i = 0; i < rays; i++)
        {
            double angle = Math.PI / 2 * (i / (double)(rays - 1)) - Math.PI; // abanico de 90°
            var end = new Point(
                center.X + (int)(radius * Math.Cos(angle)),
                center.Y + (int)(radius * Math.Sin(angle)));
            g.DrawLine(pen, center, end);
        }

        for (int r = radius / rays; r < radius; r += radius / rays)
        {
            var rect = new Rectangle(center.X - r, center.Y - r, r * 2, r * 2);
            g.DrawArc(pen, rect, 180, 90);
        }
    }

    /// <summary>Dibuja una pequeña araña (cuerpo + 8 patas) centrada en <paramref name="center"/>.
    /// Las normales usan un cuerpo rojo "viuda negra" con patas claras para que
    /// siempre destaquen sobre el fondo oscuro del tablero (antes eran casi
    /// invisibles: cuerpo casi-negro sobre fondo casi-negro).</summary>
    public static void PaintSpider(Graphics g, Point center, int size, bool golden = false)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        Color body = golden ? GoldAccent : SpideyRed;
        Color legs = golden ? Color.FromArgb(200, 150, 10) : Color.FromArgb(225, 225, 230);
        Color glowColor = golden ? GoldAccent : SpideyRed;

        // Halo sutil para que la araña resalte siempre sobre el tablero, sea cual sea el fondo.
        using var glowPen = new Pen(Color.FromArgb(golden ? 130 : 90, glowColor), golden ? 2f : 1.6f);
        g.DrawEllipse(glowPen, center.X - size, center.Y - size, size * 2, size * 2);

        using var legPen = new Pen(legs, Math.Max(1.5f, size / 9f));
        int legSpan = size;
        for (int i = 0; i < 4; i++)
        {
            float t = i / 3f;
            int yOffset = (int)(-legSpan / 2 + t * legSpan);
            g.DrawLine(legPen, center.X - size / 6, center.Y + yOffset / 3,
                center.X - legSpan / 2, center.Y + yOffset);
            g.DrawLine(legPen, center.X + size / 6, center.Y + yOffset / 3,
                center.X + legSpan / 2, center.Y + yOffset);
        }

        using var bodyBrush = new SolidBrush(body);
        int headSize = size / 2;
        g.FillEllipse(bodyBrush, center.X - headSize / 2, center.Y - size * 2 / 5, headSize, headSize);
        int abdomenSize = (int)(size * 0.8);
        var abdomenRect = new Rectangle(center.X - abdomenSize / 2, center.Y - abdomenSize / 4, abdomenSize, abdomenSize);
        g.FillEllipse(bodyBrush, abdomenRect);

        // Marca de telaraña en el abdomen (estilo viuda negra / traje arácnido) para dar detalle temático.
        Color markColor = golden ? Color.FromArgb(160, 120, 30, 0) : Color.FromArgb(200, 10, 8, 12);
        using var markPen = new Pen(markColor, Math.Max(1f, size / 14f));
        int mw = abdomenSize / 3;
        g.DrawLine(markPen, center.X, abdomenRect.Top + abdomenSize / 6, center.X, abdomenRect.Bottom - abdomenSize / 6);
        g.DrawLine(markPen, center.X - mw / 2, abdomenRect.Top + abdomenSize / 3, center.X + mw / 2, abdomenRect.Top + abdomenSize / 3);
        g.DrawLine(markPen, center.X - mw / 2, abdomenRect.Bottom - abdomenSize / 3, center.X + mw / 2, abdomenRect.Bottom - abdomenSize / 3);

        // Ojitos blancos brillantes en la cabeza, igual de visibles que los del propio Spider-Man.
        int eyeSize = Math.Max(2, size / 8);
        using var eyeBrush = new SolidBrush(Color.White);
        g.FillEllipse(eyeBrush, center.X - eyeSize - 1, center.Y - size * 2 / 5 + headSize / 4, eyeSize, eyeSize);
        g.FillEllipse(eyeBrush, center.X + 1, center.Y - size * 2 / 5 + headSize / 4, eyeSize, eyeSize);
    }
}
