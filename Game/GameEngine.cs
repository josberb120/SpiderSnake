namespace SpiderSnake.Game;

internal enum Direction { Up, Down, Left, Right }

internal enum TickResult { Continue, AteSpider, AteGoldenSpider, GameOver }

/// <summary>
/// Lógica pura del Snake: grid, posición de la serpiente, comida y colisiones.
/// No conoce nada de WinForms; <see cref="Game.GameScreen"/> es quien la dibuja
/// y la conecta a un Timer + teclado.
/// </summary>
internal class GameEngine
{
    public const int GoldenSpiderEveryNFoods = 5;
    private const int GoldenSpiderLifeTicks = 35; // se "escapa" si no se come a tiempo

    public int GridWidth { get; }
    public int GridHeight { get; }

    public List<Point> Snake { get; private set; } = new();
    public Point SpiderPosition { get; private set; }
    public bool IsGoldenSpider { get; private set; }
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    private Direction _direction = Direction.Right;
    private Direction _pendingDirection = Direction.Right;
    private readonly Random _rng = new();
    private int _spidersEaten;
    private int _goldenSpiderTicksLeft;

    public GameEngine(int gridWidth, int gridHeight)
    {
        GridWidth = gridWidth;
        GridHeight = gridHeight;
        Reset();
    }

    public void Reset()
    {
        int startX = GridWidth / 4;
        int startY = GridHeight / 2;
        Snake = new List<Point>
        {
            new(startX, startY),
            new(startX - 1, startY),
            new(startX - 2, startY),
        };
        _direction = Direction.Right;
        _pendingDirection = Direction.Right;
        Score = 0;
        _spidersEaten = 0;
        IsGameOver = false;
        SpawnSpider();
    }

    public void ChangeDirection(Direction d)
    {
        bool isOpposite =
            (d == Direction.Up && _direction == Direction.Down) ||
            (d == Direction.Down && _direction == Direction.Up) ||
            (d == Direction.Left && _direction == Direction.Right) ||
            (d == Direction.Right && _direction == Direction.Left);

        if (!isOpposite)
        {
            _pendingDirection = d;
        }
    }

    public TickResult Tick()
    {
        if (IsGameOver) return TickResult.GameOver;

        _direction = _pendingDirection;
        Point head = Snake[0];
        Point newHead = _direction switch
        {
            Direction.Up => new Point(head.X, head.Y - 1),
            Direction.Down => new Point(head.X, head.Y + 1),
            Direction.Left => new Point(head.X - 1, head.Y),
            Direction.Right => new Point(head.X + 1, head.Y),
            _ => head
        };

        bool hitWall = newHead.X < 0 || newHead.X >= GridWidth || newHead.Y < 0 || newHead.Y >= GridHeight;
        bool hitSelf = Snake.Contains(newHead);

        if (hitWall || hitSelf)
        {
            IsGameOver = true;
            return TickResult.GameOver;
        }

        Snake.Insert(0, newHead);

        bool ateFood = newHead == SpiderPosition;
        TickResult result = TickResult.Continue;

        if (ateFood)
        {
            bool wasGolden = IsGoldenSpider;
            Score += wasGolden ? 30 : 10;
            _spidersEaten++;
            result = wasGolden ? TickResult.AteGoldenSpider : TickResult.AteSpider;
            SpawnSpider();
        }
        else
        {
            Snake.RemoveAt(Snake.Count - 1);

            if (IsGoldenSpider && --_goldenSpiderTicksLeft <= 0)
            {
                IsGoldenSpider = false; // la araña dorada se escapa: vuelve a una normal
                SpawnSpider(forceNormal: true);
            }
        }

        return result;
    }

    public int CurrentIntervalMs(int startingIntervalMs, int minIntervalMs)
    {
        int reduced = startingIntervalMs - _spidersEaten * 3;
        return Math.Max(minIntervalMs, reduced);
    }

    private void SpawnSpider(bool forceNormal = false)
    {
        Point pos;
        do
        {
            pos = new Point(_rng.Next(GridWidth), _rng.Next(GridHeight));
        } while (Snake.Contains(pos));

        SpiderPosition = pos;
        IsGoldenSpider = !forceNormal && _spidersEaten > 0 &&
                          _spidersEaten % GoldenSpiderEveryNFoods == 0 && _rng.NextDouble() < 0.6;
        _goldenSpiderTicksLeft = GoldenSpiderLifeTicks;
    }
}
