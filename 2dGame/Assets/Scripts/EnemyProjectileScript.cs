using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Speed at which the projectile moves")]
    public float speed = 8f;

    [Tooltip("Damage dealt to the player on hit")]
    public int damage = 1;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Make sure the Rigidbody2D is set up for kinematic movement (not affected by gravity)
        rb.gravityScale = 0f;
        rb.isKinematic = false; // allows velocity driven movement and collision detection
    }

    /// <summary>
    /// Sets the target position the projectile will move toward.
    /// Call this immediately after instantiation.
    /// </summary>
    /// <param name="targetPosition">World space position of the target</param>
    public void SetTarget(Vector2 targetPosition)
    {
        Vector2 currentPosition = rb.position;
        moveDirection = (targetPosition - currentPosition).normalized;

        // Optionally, rotate the projectile to face the movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void FixedUpdate()
    {
        // Move projectile rigidbody by setting velocity
        rb.linearVelocity = moveDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect collision with player or environment

        if (other.CompareTag("Player"))
        {
            // Here you can send a message or access a player health script to apply damage
            // Example: other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            // Destroy the projectile on hitting player
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // If hitting environment objects (walls, ground, etc.), destroy projectile
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        // Destroy projectile if it goes off-screen to avoid clutter
        Destroy(gameObject);
    }
}