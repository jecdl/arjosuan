using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public float speed = 0.08f; 
    
    private Vector3 targetPosition;
    private GameManager gameManager;

    void Start()
    {
        // El destino es el centro pero manteniendo la misma altura Y
        targetPosition = new Vector3(0f, transform.localPosition.y, 0f);
        
        // Buscar el GameManager
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        // Moverse hacia el centro manteniendo la altura
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, speed * Time.deltaTime);

        // Si llega muy cerca del centro, notificar al GameManager y destruirse
        float distanceToCenter = Vector3.Distance(
            new Vector3(transform.localPosition.x, 0, transform.localPosition.z), 
            Vector3.zero
        );
        
        if (distanceToCenter < 0.04f)
        {
            // Notificar que un enemigo llegó a la base
            if (gameManager != null)
            {
                gameManager.OnEnemyReachedBase();
            }
            Destroy(gameObject); 
        }
    }
}
