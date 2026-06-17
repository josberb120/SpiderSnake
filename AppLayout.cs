namespace SpiderSnake;

/// <summary>
/// Tamaños compartidos por toda la app. La ventana es de tamaño fijo (no
/// redimensionable), así que cada pantalla puede usar estas constantes en
/// lugar de recalcular layouts dinámicos.
/// </summary>
internal static class AppLayout
{
    public const int CellSize = 20;
    public const int GridWidth = 38;
    public const int GridHeight = 28;
    public const int TopBarHeight = 72;

    public const int ScreenWidth = GridWidth * CellSize;                  // 760
    public const int ScreenHeight = GridHeight * CellSize + TopBarHeight; // 620
}
