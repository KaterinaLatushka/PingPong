using UnityEngine;
using TMPro;

public class BallController : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 8f;
    private Rigidbody2D rb;

    [Header("Scoring")]
    public int winScore = 5;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    private int score1, score2;
    private bool isGameOver = false;

    [Header("FX")]
    public GameObject wallHitBurstPrefab;  // assign Particles/WallHitBurst
    private BackgroundColorChanger colorFX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colorFX = FindObjectOfType<BackgroundColorChanger>();

        // Auto-find UI if not dragged in the Inspector
        if (scoreText == null)
        {
            var go = GameObject.Find("ScoreText");
            if (go) scoreText = go.GetComponent<TextMeshProUGUI>();
        }
        if (winText == null)
        {
            var go = GameObject.Find("WinText");
            if (go) winText = go.GetComponent<TextMeshProUGUI>();
        }

        // Ensure texts are visible and initialized
        if (scoreText) scoreText.color = new Color(1f, 1f, 1f, 1f);
        if (winText)
        {
            winText.gameObject.SetActive(true);           // make sure it's enabled
            winText.text = "";                            // start empty
            winText.color = new Color(1f, 1f, 1f, 1f);    // white, alpha 1
            winText.fontSize = 72;
            winText.alignment = TextAlignmentOptions.Center;
            var rt = winText.rectTransform;               // center it
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        Time.timeScale = 1f;
        isGameOver = false;
        UpdateScore();
        LaunchBall();
    }

    void Update()
    {
        if (isGameOver) return;

        // Safety nudge if velocity ever gets too small (prevents stalls)
        if (rb.linearVelocity.sqrMagnitude < 1e-4f)
        {
            float x = (transform.position.x >= 0f) ? -1f : 1f;
            rb.linearVelocity = new Vector2(x, 0.2f).normalized * speed;
        }

        // Quick restart for testing / demo
        if (Input.GetKeyDown(KeyCode.R))
        {
            isGameOver = false;
            Time.timeScale = 1f;
            score1 = score2 = 0;
            if (winText) winText.text = "";
            UpdateScore();
            ResetBall();
        }
    }

    void LaunchBall()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-0.7f, 0.7f);
        rb.linearVelocity = new Vector2(x, y).normalized * speed; // use velocity
    }

    void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;
        Invoke(nameof(LaunchBall), 0.9f);
    }

    void UpdateScore()
    {
        if (scoreText != null) scoreText.text = $"{score1} : {score2}";
        else Debug.LogWarning("ScoreText reference is missing.");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Debug.Log("Hit: " + col.collider.name); // uncomment to debug names

        // Particle burst at contact
        if (wallHitBurstPrefab != null && col.contactCount > 0)
        {
            Vector2 p = col.GetContact(0).point;
            Instantiate(wallHitBurstPrefab, p, Quaternion.identity);
        }

        string n = col.collider.gameObject.name;

        // --- Scoring walls (names must match exactly) ---
        if (n == "LeftWall")
        {
            score2++;
            UpdateScore();
            if (colorFX) colorFX.FlashForPlayer(2);
            CheckWinOrContinue();
            return;
        }
        else if (n == "RightWall")
        {
            score1++;
            UpdateScore();
            if (colorFX) colorFX.FlashForPlayer(1);
            CheckWinOrContinue();
            return;
        }

        // --- Deterministic bounces ---

        // Paddle bounce (tag your paddles as "Paddle")
        if (col.collider.CompareTag("Paddle"))
        {
            float currentSpeed = Mathf.Max(rb.linearVelocity.magnitude, speed);

            // Angle based on where the ball hit the paddle
            float yOffset = transform.position.y - col.transform.position.y;
            float halfPaddle = 1.0f;
            var paddleCol = col.collider as BoxCollider2D;
            if (paddleCol != null) halfPaddle = Mathf.Max(0.5f, paddleCol.bounds.extents.y);

            float normalizedY = Mathf.Clamp(yOffset / halfPaddle, -1f, 1f);

            // Flip X, set Y by contact offset
            float newX = (rb.linearVelocity.x >= 0f) ? -1f : 1f;
            Vector2 dir = new Vector2(newX, normalizedY).normalized;

            if (dir.sqrMagnitude < 1e-4f) dir = new Vector2(newX, 0.2f).normalized; // safety

            rb.linearVelocity = dir * currentSpeed;
            return;
        }

        // Top/Bottom (and other) bounces: reflect using surface normal
        if (col.contactCount > 0)
        {
            float currentSpeed = Mathf.Max(rb.linearVelocity.magnitude, speed);
            Vector2 normal = col.GetContact(0).normal;
            Vector2 incoming = rb.linearVelocity.sqrMagnitude > 1e-4f ? rb.linearVelocity : new Vector2(1f, 0.1f);
            Vector2 reflected = Vector2.Reflect(incoming, normal).normalized;

            // Ensure some horizontal component
            if (Mathf.Abs(reflected.x) < 0.05f)
                reflected.x = Mathf.Sign(reflected.x == 0 ? incoming.x : reflected.x) * 0.1f;

            rb.linearVelocity = reflected * currentSpeed;
        }
    }

    void CheckWinOrContinue()
    {
        Debug.Log($"WIN? s1={score1}, s2={score2}, target={winScore}");

        if (score1 >= winScore)
        {
            isGameOver = true;
            if (winText)
            {
                winText.gameObject.SetActive(true);
                winText.text = "Player 1 Wins!";
                winText.color = new Color(1f, 1f, 1f, 1f);
                winText.fontSize = 72;
                winText.alignment = TextAlignmentOptions.Center;
                var rt = winText.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
            Time.timeScale = 0f;
            return;
        }
        else if (score2 >= winScore)
        {
            isGameOver = true;
            if (winText)
            {
                winText.gameObject.SetActive(true);
                winText.text = "Player 2 Wins!";
                winText.color = new Color(1f, 1f, 1f, 1f);
                winText.fontSize = 72;
                winText.alignment = TextAlignmentOptions.Center;
                var rt = winText.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
            Time.timeScale = 0f;
            return;
        }

        ResetBall();
    }
}
