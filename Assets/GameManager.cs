using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Vuforia;

public class GameManager : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public Transform imageTarget;  
    public TextMeshProUGUI scoreText;

    [Header("Configuración")]
    public string playerName = "Josuan Castillo";
    public int maxLives = 3;
    public float spawnInterval = 1.5f;
    public float spawnRadius = 0.45f;
    public float enemyHeight = 0.04f;
    public float enemyScale = 0.06f;

    private int score = 0;
    private int lives;
    private float timer = 0f;
    private bool wasTouching = false;
    private bool isTargetTracked = false;
    private ObserverBehaviour observerBehaviour;

    void Start()
    {
        lives = maxLives;
        UpdateUI();
        
        // Conectar con Vuforia para detectar la imagen
        if (imageTarget != null)
        {
            observerBehaviour = imageTarget.GetComponent<ObserverBehaviour>();
            if (observerBehaviour != null)
            {
                observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
                Debug.Log("Vuforia conectado - esperando detección de imagen");
            }
        }
    }

    void OnDestroy()
    {
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        isTargetTracked = (targetStatus.Status == Status.TRACKED || 
                          targetStatus.Status == Status.EXTENDED_TRACKED);
        Debug.Log("Imagen detectada: " + isTargetTracked + " - Status: " + targetStatus.Status);
    }

    void Update()
    {
        // Solo generar enemigos si la imagen está siendo rastreada
        if (isTargetTracked && lives > 0)
        {
            timer += Time.deltaTime;
            if (timer > spawnInterval)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }

        // Detectar toques
        if (lives > 0)
        {
            DetectTouch();
        }
    }

    void DetectTouch()
    {
        bool isTouching = false;
        Vector2 touchPosition = Vector2.zero;

        // Pantalla táctil
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            isTouching = true;
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // Mouse (para editor)
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            isTouching = true;
            touchPosition = Mouse.current.position.ReadValue();
        }

        // Solo procesar cuando INICIA el toque
        if (isTouching && !wasTouching)
        {
            ProcessTouch(touchPosition);
        }

        wasTouching = isTouching;
    }

    void ProcessTouch(Vector2 touchPosition)
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            EnemyBehavior enemy = hit.transform.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                Destroy(hit.transform.gameObject);
                score += 10;
                UpdateUI();
                Debug.Log("¡Enemigo destruido! Puntos: " + score);
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || imageTarget == null) 
        {
            Debug.LogError("Falta enemyPrefab o imageTarget!");
            return;
        }
        
        // Crear enemigo como hijo del ImageTarget
        GameObject newEnemy = Instantiate(enemyPrefab, imageTarget);
        
        // Escalar
        newEnemy.transform.localScale = new Vector3(enemyScale, enemyScale, enemyScale);
        
        // Posición aleatoria en círculo
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        newEnemy.transform.localPosition = new Vector3(
            Mathf.Cos(angle) * spawnRadius, 
            enemyHeight, 
            Mathf.Sin(angle) * spawnRadius
        );
        
        Debug.Log("Enemigo creado en posición: " + newEnemy.transform.localPosition);
    }

    public void OnEnemyReachedBase()
    {
        lives--;
        UpdateUI();
        Debug.Log("¡Enemigo llegó a la base! Vidas restantes: " + lives);
        
        if (lives <= 0)
        {
            if (scoreText != null)
            {
                scoreText.text = "GAME OVER - Puntos: " + score + "\n" + playerName;
            }
            Debug.Log("GAME OVER!");
        }
    }

    public void RestartGame()
    {
        score = 0;
        lives = maxLives;
        timer = 0f;
        
        // Destruir todos los enemigos
        EnemyBehavior[] enemies = FindObjectsByType<EnemyBehavior>(FindObjectsSortMode.None);
        foreach (EnemyBehavior enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        
        UpdateUI();
        Debug.Log("Juego reiniciado");
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            if (lives > 0)
            {
                scoreText.text = "Puntos: " + score + " | Vidas: " + lives + " | " + playerName;
            }
        }
    }
}
