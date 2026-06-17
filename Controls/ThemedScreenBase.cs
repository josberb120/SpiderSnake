using SpiderSnake.Theme;

namespace SpiderSnake.Controls;

/// <summary>
/// Base para todas las pantallas de menú: pinta el fondo (un único degradado +
/// telaraña) y ofrece el punto de extensión <see cref="Reflow"/> para que cada
/// pantalla concreta recoloque sus controles según el tamaño actual — así la
/// ventana puede redimensionarse/maximizarse sin que nada quede descentrado o
/// cortado.
/// </summary>
internal class ThemedScreenBase : UserControl
{
    public Color Accent { get; set; } = SpideyTheme.SpideyRed;
    public Color AccentDark { get; set; } = SpideyTheme.SpideyBlueDark;

    protected ThemedScreenBase()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                  ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        BackColor = SpideyTheme.NearBlack;
        Dock = DockStyle.Fill;
        MinimumSize = new Size(AppLayout.ScreenWidth, AppLayout.ScreenHeight);
        Resize += (_, _) => Reflow();
    }

    /// <summary>
    /// Recoloca/redimensiona los controles de la pantalla a partir del
    /// <see cref="Control.Width"/>/<see cref="Control.Height"/> actuales.
    /// Las subclases lo sobreescriben y deben llamarlo una vez al final de su
    /// constructor para fijar el layout inicial.
    /// </summary>
    protected virtual void Reflow()
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        SpideyTheme.PaintBackdrop(e.Graphics, ClientRectangle, Accent, AccentDark);
        base.OnPaint(e);
    }

    /// <summary>
    /// Etiqueta de título grande, estilo cómic. Usa <c>AutoSize</c> a propósito: con una caja de
    /// alto fijo en píxeles, el texto se corta en cuanto la fuente "Impact" no está instalada
    /// (Windows la sustituye por otra con métricas distintas) o el sistema tiene un escalado de
    /// pantalla distinto al 100%. Con AutoSize, WinForms mide el texto real en tiempo de
    /// ejecución y la caja siempre es del tamaño exacto que necesita, sin adivinar números.
    /// </summary>
    protected static Label MakeTitle(string text, float size, Color color)
    {
        return new Label
        {
            Text = text,
            Font = SpideyTheme.TitleFont(size),
            ForeColor = color,
            AutoSize = true,
            // GDI (el renderizador de texto por defecto desde .NET 2.0) calcula el alto a partir
            // de métricas genéricas que para una fuente decorativa como "Impact" se quedan
            // cortas y rebanan astas/colas por el medio. GDI+ (compatible) mide con las métricas
            // reales de la fuente, así el cuadro AutoSize siempre alcanza para la letra completa.
            UseCompatibleTextRendering = true,
            Padding = new Padding(0, 4, 0, 6),
            BackColor = Color.Transparent,
        };
    }

    /// <summary>
    /// Etiqueta de texto normal (subtítulos, pies, mensajes). Igual que <see cref="MakeTitle"/>,
    /// usa AutoSize + renderizado GDI+ para que nunca se corte el texto sin importar fuente/DPI.
    /// </summary>
    protected static Label MakeLabel(string text, float size, Color color, FontStyle style = FontStyle.Regular)
    {
        return new Label
        {
            Text = text,
            Font = SpideyTheme.BodyFont(size, style),
            ForeColor = color,
            AutoSize = true,
            UseCompatibleTextRendering = true,
            Padding = new Padding(0, 2, 0, 3),
            BackColor = Color.Transparent,
        };
    }

    /// <summary>Centra horizontalmente un control dentro del ancho actual de la pantalla.</summary>
    protected void CenterHorizontally(Control control, int top)
    {
        control.Location = new Point((Width - control.Width) / 2, top);
    }
}
