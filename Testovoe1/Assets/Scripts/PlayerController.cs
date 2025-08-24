using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject bulletPrefab1; 
    public GameObject bulletPrefab2; 
    public Transform firePoint;

    public Sprite[] deathSprites; 
    public Image[] lifeImages; 
    public TMP_Text scoreText;
    public TMP_Text recordText;
    public GameObject pauseMenu; 
    public Button pauseButton;

    public List<GameObject> levelPrefabs; 
    public List<GameObject> activeLevels = new List<GameObject>();


    private Animator animator; 
    public SpriteRenderer spriteRenderer; 

    public int maxLives = 3;
    private int currentLives;
    private Vector2 moveInput = Vector2.zero;
    private bool isPaused = false;

    private float shootCooldown = 0.3f;
    private float lastShootTime = 0f;

    private int enemyCount = 0; 

    private bool moveUp, moveDown, moveLeft, moveRight;

    private enum Direction { Up, Down, Left, Right, Idle };
    private Direction currentDirection = Direction.Down; 

    private int score = 0;       
    private int maxScore = 0;    

    private void Start()
    {
        currentLives = maxLives;
        LoadGameData();

        pauseButton.onClick.AddListener(TogglePause);
        pauseMenu.SetActive(false);

        transform.position = Vector3.zero;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        GenerateNewLevel();

        UpdateUI();
    }

    private void Update()
    {
        if (isPaused)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        if (moveUp) moveInput = Vector2.up;
        else if (moveDown) moveInput = Vector2.down;
        else if (moveLeft) moveInput = Vector2.left;
        else if (moveRight) moveInput = Vector2.right;

        if (moveInput != Vector2.zero)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                currentDirection = moveInput.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                currentDirection = moveInput.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = moveInput * moveSpeed;

        UpdateWalkingAnimation();

        if (Input.GetButtonDown("Fire1"))
        {
            ShootDirection(GetCurrentDirectionVector());
        }
        if (Input.GetButtonDown("Fire2"))
        {
            ShootRandom();
        }

        if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }

        UpdateSpriteDirection();
    }

    private Vector2 GetCurrentDirectionVector()
    {
        switch (currentDirection)
        {
            case Direction.Up: return Vector2.up;
            case Direction.Down: return Vector2.down;
            case Direction.Left: return Vector2.left;
            case Direction.Right: return Vector2.right;
            default: return Vector2.down;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            moveInput = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
{
        if (collision.CompareTag("Enemy"))
        {
            TakeDamage();
    }
    if (collision.CompareTag("Door"))
            {
                foreach (var level in activeLevels)
                {
                    if (level != null)
                        Destroy(level);
                }
                activeLevels.Clear();

                GenerateNewLevel();
            }
}


    private void UpdateWalkingAnimation()
    {
        if (animator == null) return;

        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
        animator.SetBool("Moving", moveInput.sqrMagnitude > 0);
    }

    private void UpdateSpriteDirection()
    {
        if (currentDirection == Direction.Left)
        {
            spriteRenderer.flipX = false;
        }
        else if (currentDirection == Direction.Right)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void ShootDirection(Vector2 direction)
    {
        if (direction == Vector2.zero || Time.time - lastShootTime < shootCooldown)
            return;

        lastShootTime = Time.time;

        switch (currentDirection)
        {
            case Direction.Up:
                animator.SetTrigger("ShootUp");
                break;
            case Direction.Down:
                animator.SetTrigger("ShootDown");
                break;
            case Direction.Left:
                animator.SetTrigger("ShootLeft");
                break;
            case Direction.Right:
                animator.SetTrigger("ShootRight");
                break;
        }

        Instantiate(bulletPrefab1, firePoint.position, Quaternion.identity).GetComponent<Bullet>().Setup(direction);
    }

    public void ShootRandom()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;

        if (Mathf.Abs(randomDir.x) > Mathf.Abs(randomDir.y))
        {
            currentDirection = randomDir.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            currentDirection = randomDir.y > 0 ? Direction.Up : Direction.Down;
        }

        switch (currentDirection)
        {
            case Direction.Up:
                animator.SetTrigger("ShootUp");
                break;
            case Direction.Down:
                animator.SetTrigger("ShootDown");
                break;
            case Direction.Left:
                animator.SetTrigger("ShootLeft");
                break;
            case Direction.Right:
                animator.SetTrigger("ShootRight");
                break;
        }

        Instantiate(bulletPrefab2, firePoint.position, Quaternion.identity).GetComponent<Bullet>().Setup(randomDir);
    }

    public void TakeDamage()
    {
        currentLives--;
        UpdateUI();

        spriteRenderer.color = Color.red;
        Invoke("ResetColor", 0.5f);

        if (currentLives <= 0)
        {
            RestartLevel();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void EnemyDestroyed()
    {
        enemyCount++;
        AddScore(10);
    }

    private void ResetColor()
    {
        spriteRenderer.color = Color.white;
    }

    private void RestartLevel()
    {
        enemyCount = 0;
        if (score > maxScore)
        {
            maxScore = score;
            SaveGameData();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateUI()
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (i < currentLives)
                lifeImages[i].sprite = deathSprites[0];
            else
                lifeImages[i].sprite = deathSprites[1];
        }

        scoreText.text = "Score: " + score;
        recordText.text = "Max: " + maxScore;
    }

    private void SaveGameData()
    {
        PlayerPrefs.SetInt("MaxScore", maxScore);
        PlayerPrefs.Save();
    }

    private void LoadGameData()
    {
        maxScore = PlayerPrefs.GetInt("MaxScore", 0);
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void GenerateNewLevel()
{
    foreach (var level in activeLevels)
    {
        if (level != null)
            Destroy(level);
    }
    activeLevels.Clear();

    if (levelPrefabs == null || levelPrefabs.Count == 0)
    {
        Debug.LogError("Нет добавленных уровней в список levelPrefabs");
        return;
    }

    int index = Random.Range(0, levelPrefabs.Count);
    GameObject selectedLevelPrefab = levelPrefabs[index];

    GameObject newLevel = Instantiate(selectedLevelPrefab);
    newLevel.transform.position = Vector3.zero;
    activeLevels.Add(newLevel);
}



    public void OnPressUp() { moveUp = true; moveDown = false; moveLeft = false; moveRight = false; currentDirection = Direction.Up; }
    public void OnReleaseUp() { moveUp = false; }
    public void OnPressDown() { moveDown = true; moveUp = false; moveLeft = false; moveRight = false; currentDirection = Direction.Down; }
    public void OnReleaseDown() { moveDown = false; }
    public void OnPressLeft() { moveLeft = true; moveRight = false; moveUp = false; moveDown = false; currentDirection = Direction.Left; }
    public void OnReleaseLeft() { moveLeft = false; }
    public void OnPressRight() { moveRight = true; moveLeft = false; moveUp = false; moveDown = false; currentDirection = Direction.Right; }
    public void OnReleaseRight() { moveRight = false; }
}
