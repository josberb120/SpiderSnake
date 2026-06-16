using System.Drawing.Drawing2D;
using SpiderSnake.Theme;

namespace SpiderSnake.Controls;

/// <summary>
/// Botón temático dibujado a mano: esquinas redondeadas, degradado rojo/azul,
/// brillo al pasar el mouse y borde estilo cómic. No hereda de Button para
/// tener control total sobre el pintado.
/// </summary>
internal class SpideyButton : Control
{
    private bool _hover;
    private bool _pressed;

    public Color AccentColor { get; set; } = SpideyTheme.SpideyRed;
    public Color AccentColorDark { get; set; } = SpideyTheme.SpideyRedDark;

    public SpideyButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Cursor = Cursors.Hand;
        Font = SpideyTheme.BodyFont(13f, FontStyle.Bold);
        ForeColor = Color.White;
        Size = new Size(220, 52);
        TabStop = true;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Enter or Keys.Space)
        {
            OnClick(EventArgs.Empty);
        }
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        var g = pe.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int pressOffset = _pressed ? 2 : 0; // el botón "se hunde" 2px al hacer click
        var rect = new Rectangle(1, 1 + pressOffset, Width - 3, Height - 3 - pressOffset);

        if (!_pressed)
        {
            using var shadowPath = RoundedRect(new Rectangle(rect.X, rect.Y + 3, rect.Width, rect.Height), 12);
            using var shadowBrush = new SolidBrush(Color.FromArgb(70, 0, 0, 0));
            g.FillPath(shadowBrush, shadowPath);
        }

        using var path = RoundedRect(rect, 12);
        Color top = _hover ? Lighten(AccentColor) : AccentColor;
        using var fill = new LinearGradientBrush(rect, top, AccentColorDark, LinearGradientMode.Vertical);
        fill.Blend = new Blend(3)
        {
            Positions = new[] { 0f, 0.18f, 1f },
            Factors = new[] { 1f, 0.65f, 0f },
        };
        g.FillPath(fill, path);

        using var border = new Pen(_hover ? Color.White : Color.FromArgb(220, 255, 255, 255), _hover ? 2.4f : 1.6f);
        g.DrawPath(border, path);

        if (Focused)
        {
            using var focusPen = new Pen(SpideyTheme.GoldAccent, 1.6f) { DashStyle = DashStyle.Dot };
            var focusRect = Rectangle.Inflate(rect, -3, -3);
            using var focusPath = RoundedRect(focusRect, 9);
            g.DrawPath(focusPen, focusPath);
        }

        var textRect = ClientRectangle;
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var textBrush = new SolidBrush(ForeColor);
        g.DrawString(Text, Font, textBrush, textRect, sf);
    }

    private static Color Lighten(Color c) => Color.FromArgb(c.A,
        Math.Min(255, c.R + 35), Math.Min(255, c.G + 35), Math.Min(255, c.B + 35));

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
}
