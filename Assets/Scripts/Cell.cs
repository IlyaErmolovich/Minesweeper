using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Cell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Ссылки")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image mineImage;
    [SerializeField] private Image flagImage;
    [SerializeField] private Image crossImage;
    [SerializeField] private TextMeshProUGUI numberText;

    [Header("Спрайты состояний")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openedSprite;
    [SerializeField] private Sprite mineSprite;

    [Header("Цвета")]
    [SerializeField] private Color pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
    [SerializeField] private Color explosionColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField]
    private Color[] numberColors = new Color[8]
    {
        new Color(0f, 0f, 1f),
        new Color(0f, 0.5f, 0f),
        new Color(1f, 0f, 0f),
        new Color(0f, 0f, 0.5f),
        new Color(0.5f, 0f, 0f),
        new Color(0f, 0.5f, 0.5f),
        new Color(0.5f, 0f, 0.5f),
        new Color(0.5f, 0.5f, 0.5f)
    };

    [Header("Удержание")]
    [SerializeField] private float holdThreshold = 0.35f;


    public int X { get; private set; }
    public int Y { get; private set; }
    public bool IsMine { get; set; }
    public bool IsOpened { get; private set; }
    public bool IsFlagged { get; private set; }
    public int AdjacentMines { get; set; }

    private MinesweeperManager gameManager;
    private bool isPointerDown;
    private float pointerDownTimer;
    private bool isLongPressTriggered;

    public void Init(int x, int y, MinesweeperManager manager)
    {
        X = x;
        Y = y;
        gameManager = manager;

        IsOpened = false;
        IsFlagged = false;
        IsMine = false;

        bgImage.sprite = closedSprite;
        bgImage.color = Color.white;

        mineImage.gameObject.SetActive(false);
        flagImage.gameObject.SetActive(false);
        crossImage.gameObject.SetActive(false);
        numberText.gameObject.SetActive(false);
        numberText.text = string.Empty;
    }

    private void Update()
    {
        if (isPointerDown && !IsOpened && !gameManager.IsGameOver && !isLongPressTriggered)
        {
            pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= holdThreshold)
            {
                isLongPressTriggered = true;
                bgImage.color = Color.white;
                ToggleFlag();
                gameManager.SetSurprisedFace(false);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsOpened || gameManager.IsGameOver) return;

        isPointerDown = true;
        pointerDownTimer = 0f;
        isLongPressTriggered = false;

        if (!IsFlagged)
        {
            bgImage.color = pressedColor;
            gameManager.SetSurprisedFace(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;
        isPointerDown = false;

        bgImage.color = Color.white;

        if (gameManager.IsGameOver) return;

        gameManager.SetSurprisedFace(false);

        if (!isLongPressTriggered && !IsOpened && !IsFlagged)
        {
            gameManager.OnCellClicked(this);
        }
    }

    public void ToggleFlag()
    {
        if (IsOpened) return;

        IsFlagged = !IsFlagged;
        flagImage.gameObject.SetActive(IsFlagged);
        gameManager.OnFlagToggled(IsFlagged);

        RDG.Vibration.Vibrate(30);
    }

    public void Explode()
    {
        Open();
        bgImage.color = explosionColor;
    }

    public void Open()
    {
        if (IsOpened) return;

        IsOpened = true;
        bgImage.sprite = openedSprite;

        flagImage.gameObject.SetActive(false);
        crossImage.gameObject.SetActive(false);

        if (IsMine)
        {
            numberText.gameObject.SetActive(false);
            mineImage.sprite = mineSprite;
            mineImage.gameObject.SetActive(true);
        }
        else if (AdjacentMines > 0)
        {
            mineImage.gameObject.SetActive(false);

            numberText.text = AdjacentMines.ToString();
            int colorIndex = Mathf.Clamp(AdjacentMines - 1, 0, numberColors.Length - 1);
            numberText.color = numberColors[colorIndex];
            numberText.gameObject.SetActive(true);
        }
        else
        {
            mineImage.gameObject.SetActive(false);
            numberText.gameObject.SetActive(false);
        }
    }

    public void ShowWrongFlag()
    {
        if (IsFlagged && !IsMine)
        {
            bgImage.sprite = openedSprite;
            bgImage.color = Color.white;

            flagImage.gameObject.SetActive(false);
            numberText.gameObject.SetActive(false);

            mineImage.sprite = mineSprite;
            mineImage.gameObject.SetActive(true);

            crossImage.gameObject.SetActive(true);
        }
    }
}