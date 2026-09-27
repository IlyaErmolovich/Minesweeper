using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinesweeperManager : MonoBehaviour
{
    [Header("Сетка")]
    [SerializeField] private RectTransform boardArea;
    [SerializeField] private GridLayoutGroup gridGroup;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private float targetCellSize = 96f;

    [Header("Верхний UI")]
    [SerializeField] private Image faceImage;
    [SerializeField] private Sprite normalFace;
    [SerializeField] private Sprite deadFace;
    [SerializeField] private Sprite winFace;
    [SerializeField] private TextMeshProUGUI minesText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Sprite surprisedFace;

    public bool IsGameOver { get; private set; }

    private int columns;
    private int rows;
    private int totalMines;
    private int flagsLeft;
    private bool isFirstClick = true;

    private Cell[,] grid;
    private float timer;
    private bool isTimerRunning;

    private void Start()
    {
        StartNewGame();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            timerText.text = Mathf.Min(999, Mathf.FloorToInt(timer)).ToString("D3");
        }
    }

    public void StartNewGame()
    {
        foreach (Transform child in gridGroup.transform)
        {
            Destroy(child.gameObject);
        }

        IsGameOver = false;
        isFirstClick = true;
        isTimerRunning = false;
        timer = 0;
        timerText.text = "000";
        faceImage.sprite = normalFace;

        CalculateGrid();

        grid = new Cell[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Cell cell = Instantiate(cellPrefab, gridGroup.transform);
                cell.Init(x, y, this);
                grid[x, y] = cell;
            }
        }

        totalMines = Mathf.RoundToInt(columns * rows * 0.15f);
        flagsLeft = totalMines;
        UpdateMinesUI();
    }

    private void CalculateGrid()
    {
        Canvas.ForceUpdateCanvases();

        float width = boardArea.rect.width;
        float height = boardArea.rect.height;

        columns = Mathf.Max(5, Mathf.RoundToInt(width / targetCellSize));
        rows = Mathf.Max(5, Mathf.RoundToInt(height / targetCellSize));

        float cellWidth = width / columns;
        float cellHeight = height / rows;

        gridGroup.cellSize = new Vector2(cellWidth, cellHeight);
        gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridGroup.constraintCount = columns;
    }

    public void OnCellClicked(Cell cell)
    {
        if (isFirstClick)
        {
            isFirstClick = false;
            GenerateMines(cell.X, cell.Y);
            isTimerRunning = true;
        }

        if (cell.IsMine)
        {
            GameOver(false, cell);
            return;
        }

        OpenCellRecursive(cell.X, cell.Y);
        CheckWin();
    }

    private void GenerateMines(int safeX, int safeY)
    {
        int placedMines = 0;
        while (placedMines < totalMines)
        {
            int rx = Random.Range(0, columns);
            int ry = Random.Range(0, rows);

            if (Mathf.Abs(rx - safeX) <= 1 && Mathf.Abs(ry - safeY) <= 1) continue;

            if (!grid[rx, ry].IsMine)
            {
                grid[rx, ry].IsMine = true;
                placedMines++;
            }
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (!grid[x, y].IsMine)
                {
                    grid[x, y].AdjacentMines = CountAdjacentMines(x, y);
                }
            }
        }
    }

    private int CountAdjacentMines(int cx, int cy)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = cx + dx;
                int ny = cy + dy;
                if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
                {
                    if (grid[nx, ny].IsMine) count++;
                }
            }
        }
        return count;
    }

    private void OpenCellRecursive(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return;
        Cell cell = grid[x, y];

        if (cell.IsOpened || cell.IsFlagged || cell.IsMine) return;

        cell.Open();

        if (cell.AdjacentMines == 0)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx != 0 || dy != 0)
                        OpenCellRecursive(x + dx, y + dy);
                }
            }
        }
    }

    public void OnFlagToggled(bool isAdded)
    {
        flagsLeft += isAdded ? -1 : 1;
        UpdateMinesUI();
    }

    private void UpdateMinesUI()
    {
        minesText.text = Mathf.Clamp(flagsLeft, -99, 999).ToString("D3");
    }

    private void CheckWin()
    {
        foreach (var cell in grid)
        {
            if (!cell.IsMine && !cell.IsOpened) return;
        }

        GameOver(true);
    }

    private void GameOver(bool isWin, Cell explodedCell = null)
    {
        IsGameOver = true;
        isTimerRunning = false;
        faceImage.sprite = isWin ? winFace : deadFace;

        if (!isWin)
        {
            RDG.Vibration.Vibrate(30);

            if (explodedCell != null)
            {
                explodedCell.Explode();
            }

            foreach (var cell in grid)
            {
                if (cell == explodedCell) continue;

                if (cell.IsMine)
                {
                    if (!cell.IsFlagged)
                    {
                        cell.Open();
                    }
                }
                else if (cell.IsFlagged)
                {
                    cell.ShowWrongFlag();
                }
            }
        }
    }

    public void SetSurprisedFace(bool isSurprised)
    {
        if (IsGameOver) return;
        faceImage.sprite = isSurprised ? surprisedFace : normalFace;
    }
}