using UnityEngine;
using UnityEngine.SceneManagement;

public class ChasingEnemy : MonoBehaviour
{
    private Rigidbody2D rb;
    public int health = 15;

    // Movement speed of the enemy chasing the player
    [SerializeField] private float chaseSpeed = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Get the player's current position (2D)
        Vector2 playerPos = GlobalFunctions.GetPlayerPosition();

        // Calculate direction vector from enemy to player
        Vector2 direction = (playerPos - rb.position).normalized;

        // Calculate new position the enemy should move to this frame
        Vector2 newPosition = rb.position + direction * chaseSpeed * Time.fixedDeltaTime;

        // Move the enemy's Rigidbody2D to the new position
        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Instantly kill the player by reloading the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (other.CompareTag("FriendlyProjectile"))
        {
            // Take damage or destroy this enemy on hit by player's projectile
            TakeDamage(1);
        }
    }

    public Vector2 GetEnemyPosition()
    {
        return rb.position;
    }

    public void SetEnemyPosition(Vector2 newPosition)
    {
        rb.position = newPosition;
        rb.MovePosition(newPosition);
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int setTo)
    {
        health = setTo;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}