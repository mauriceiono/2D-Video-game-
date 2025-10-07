using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb;
    public int health = 15;
    public GameObject enemyProjectilePrefab;

    public float shootInterval = 2f;

    private float shootTimer;

    // Activates on initialization
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        shootTimer = shootInterval; // So the enemy shoots immediately on start
    }

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            ShootAtPlayer();
            shootTimer = shootInterval;
        }
    }

    private void ShootAtPlayer()
    {
        Vector2 playerPos = GlobalFunctions.GetPlayerPosition();
        Vector2 enemyPos = rb.position;

        if (enemyProjectilePrefab != null)
        {
            // Instantiate projectile at enemy position
            GameObject projectileGO = Instantiate(enemyProjectilePrefab, enemyPos, Quaternion.identity);

            // Assuming the projectile has a script to handle movement/direction:
            EnemyProjectile projectileScript = projectileGO.GetComponent<EnemyProjectile>();
            if (projectileScript != null)
            {
                projectileScript.SetTarget(playerPos);
            }
            else
            {
                Debug.LogWarning("EnemyProjectile script not found on projectile prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Enemy projectile prefab is not assigned in the inspector.");
        }
    }

    private void FixedUpdate()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if the player has a shield
            PlayerMovement2D player = other.GetComponent<PlayerMovement2D>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerMovement2D>();
            }

            if (player != null && player.HasShield())
            {
                // Consume the shield and destroy this enemy instead of killing the player
                player.RemoveShield();
                Destroy(gameObject);
                return;
            }

            // No shield: instantly kill the player by reloading the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (other.CompareTag("FriendlyProjectile"))
        {
            // Destroy this enemy
            // You may want to add damage handling here if not handled elsewhere
            // For example: TakeDamage(damageAmount);
        }
    }

    // Gets and returns the enemy position
    public Vector2 GetEnemyPosition()
    {
        return rb.position;
    }

    // Makes the enemy move to this position
    public void SetEnemyPosition(Vector2 newPosition)
    {
        rb.position = newPosition;

        // Ensures physics position sync
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