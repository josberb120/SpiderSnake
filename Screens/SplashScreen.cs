using System.Drawing.Drawing2D;
using SpiderSnake.Controls;
using SpiderSnake.Theme;

namespace SpiderSnake.Screens;

/// <summary>Pantalla de carga animada: telaraña girando + barra de progreso temática.</summary>
internal class SplashScreen : ThemedScreenBase
{
    private const int DurationMs = 2200;

    public event Action? Finished;

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private double _elapsedMs;
    private bool _completed;

    private readonly Label _title;
    private readonly Label _subtitle;
    private readonly Label _hint;
    private Point _webCenter;
    private Rectangle _barRect;

    public SplashScreen()
    {
        Accent = SpideyTheme.SpideyRed;
        AccentDark = SpideyTheme.NearBlack;

        _title = MakeTitle("SPIDER-SNAKE", 58f, Color.White);
        _subtitle = MakeLabel("tu vecino arácnido tiene hambre de telaraña", 12f, SpideyTheme.GoldAccent, FontStyle.Italic);
        _hint = MakeLabel("haz clic para continuar", 9f, Color.FromArgb(170, 255, 255, 255));

        Controls.Add(_title);
        Controls.Add(_subtitle);
        Controls.Add(_hint);

        Click += (_, _) => Complete();
        _title.Click += (_, _) => Complete();

        _timer.Tick += (_, _) =>
        {
            _elapsedMs += _timer.Interval;
            Invalidate();
            if (_elapsedMs >= DurationMs) Complete();
        };

        Reflow();
    }

    protected override void Reflow()
    {
        CenterHorizontally(_title, Height / 2 - 150);
        CenterHorizontally(_subtitle, Height / 2 - 50);
        CenterHorizontally(_hint, Height - 60);

        _webCenter = new Point(Width / 2, Height / 2 - 220);
        _barRect = new Rectangle(Width / 2 - 150, Height - 100, 300, 14);
    }

    public void Restart()
    {
        _elapsedMs = 0;
        _completed = false;
        _timer.Start();
    }

    private void Complete()
    {
        if (_completed) return;
        _completed = true;
        _timer.Stop();
        Finished?.Invoke();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Telaraña central girando lentamente.
        double rotation = _elapsedMs * 0.05;
        using var webPen = new Pen(Color.FromArgb(90, 255, 255, 255), 1.4f);
        const int rays = 8;
        const int radius = 70;
        for (int i = 0; i < rays; i++)
        {
            double angle = rotation + i * (Math.PI * 2 / rays);
            var end = new Point(_webCenter.X + (int)(radius * Math.Cos(angle)), _webCenter.Y + (int)(radius * Math.Sin(angle)));
            g.DrawLine(webPen, _webCenter, end);
        }
        for (int r = 18; r <= radius; r += 18)
        {
            g.DrawEllipse(webPen, _webCenter.X - r, _webCenter.Y - r, r * 2, r * 2);
        }
        SpideyTheme.PaintSpider(g, _webCenter, 22);

        // Barra de progreso (color plano, sin degradado).
        double progress = Math.Clamp(_elapsedMs / DurationMs, 0, 1);
        using var barBg = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
        g.FillRectangle(barBg, _barRect);
        var fillRect = new Rectangle(_barRect.X, _barRect.Y, (int)(_barRect.Width * progress), _barRect.Height);
        using var barFill = new SolidBrush(SpideyTheme.GoldAccent);
        g.FillRectangle(barFill, fillRect);
        using var barBorder = new Pen(Color.White, 1.4f);
        g.DrawRectangle(barBorder, _barRect);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Restart();
    }
}
