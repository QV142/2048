using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int gridSize = 4;
    public GameObject tilePrefab;
    public RectTransform gridParent;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public GameObject gameOverPanel;

    private int[,] board;
    private TileUI[,] tilesUI;

    private int score = 0;
    private int bestScore = 0;

    private Vector2 touchStartPos;
    private bool isSwipe;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        StartGame();
    }

    void StartGame()
    {
        board = new int[gridSize, gridSize];
        tilesUI = new TileUI[gridSize, gridSize];

        foreach (Transform child in gridParent) Destroy(child.gameObject);

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject obj = Instantiate(tilePrefab, gridParent);
                TileUI tile = obj.GetComponent<TileUI>();
                tile.SetValue(0);
                tilesUI[x, y] = tile;
            }
        }

        score = 0;
        gameOverPanel.SetActive(false);

        SpawnTile();
        SpawnTile();
        UpdateBoardUI();
    }

    void UpdateBoardUI()
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int value = board[x, y];
                tilesUI[x, y].SetValue(value);
            }
        }

        scoreText.text = "Score: " + score;
        bestScoreText.text = "Best: " + bestScore;
    }

    void SpawnTile()
    {
        List<Vector2Int> empty = new List<Vector2Int>();
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (board[x, y] == 0) empty.Add(new Vector2Int(x, y));
            }
        }

        if (empty.Count > 0)
        {
            Vector2Int pos = empty[Random.Range(0, empty.Count)];
            board[pos.x, pos.y] = Random.value < 0.9f ? 2 : 4;
        }
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isSwipe = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwipe)
            {
                Vector2 delta = touch.position - touchStartPos;

                if (delta.magnitude > 50f)
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0) Move(Vector2Int.right);
                        else Move(Vector2Int.left);
                    }
                    else
                    {
                        if (delta.y > 0) Move(Vector2Int.up);
                        else Move(Vector2Int.down);
                    }
                }

                isSwipe = false;
            }
        }
    }

    public void Move(Vector2Int dir)
    {
        bool moved = false;

        if (dir == Vector2Int.left) moved = MoveHorizontal(true);
        else if (dir == Vector2Int.right) moved = MoveHorizontal(false);
        else if (dir == Vector2Int.up) moved = MoveVertical(true);
        else if (dir == Vector2Int.down) moved = MoveVertical(false);

        if (moved)
        {
            SpawnTile();
            UpdateBoardUI();
            if (CheckGameOver()) GameOver();
        }
    }

    bool MoveHorizontal(bool left)
    {
        bool moved = false;
        for (int y = 0; y < gridSize; y++)
        {
            int[] line = new int[gridSize];
            for (int x = 0; x < gridSize; x++)
                line[x] = board[left ? x : gridSize - 1 - x, y];

            if (CompressAndMerge(line))
            {
                moved = true;
                for (int x = 0; x < gridSize; x++)
                    board[left ? x : gridSize - 1 - x, y] = line[x];
            }
        }
        return moved;
    }

    bool MoveVertical(bool up)
    {
        bool moved = false;
        for (int x = 0; x < gridSize; x++)
        {
            int[] line = new int[gridSize];
            for (int y = 0; y < gridSize; y++)
                line[y] = board[x, up ? y : gridSize - 1 - y];

            if (CompressAndMerge(line))
            {
                moved = true;
                for (int y = 0; y < gridSize; y++)
                    board[x, up ? y : gridSize - 1 - y] = line[y];
            }
        }
        return moved;
    }

    bool CompressAndMerge(int[] line)
    {
        List<int> list = new List<int>();
        foreach (int v in line) if (v != 0) list.Add(v);

        for (int i = 0; i < list.Count - 1; i++)
        {
            if (list[i] == list[i + 1])
            {
                list[i] *= 2;
                list[i + 1] = 0;
                score += list[i];
                if (score > bestScore)
                {
                    bestScore = score;
                    PlayerPrefs.SetInt("BestScore", bestScore);
                }
            }
        }

        List<int> finalLine = new List<int>();
        foreach (int v in list) if (v != 0) finalLine.Add(v);
        while (finalLine.Count < gridSize) finalLine.Add(0);

        bool changed = false;
        for (int i = 0; i < gridSize; i++)
        {
            if (line[i] != finalLine[i]) changed = true;
            line[i] = finalLine[i];
        }
        return changed;
    }

    bool CheckGameOver()
    {
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                if (board[x, y] == 0) return false;

        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize - 1; x++)
                if (board[x, y] == board[x + 1, y]) return false;

        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize - 1; y++)
                if (board[x, y] == board[x, y + 1]) return false;

        return true;
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
