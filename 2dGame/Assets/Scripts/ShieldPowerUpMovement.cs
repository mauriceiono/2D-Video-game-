using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
/// <summary>
/// Movement component for ShieldPowerUp prefab: slowly descends and despawns when
/// it goes below a configurable Y threshold.
/// Attach this to the PowerUp prefab in Assets/Resources/PowerUps/ShieldPowerUp.prefab.
/// The component will ensure a BoxCollider2D (trigger) and a kinematic Rigidbody2D are present.
/// </summary>
public class ShieldPowerUpMovement : MonoBehaviour
{
    [Tooltip("Downward speed in world units per second")]
    public float fallSpeed = 1.5f;

    [Tooltip("World Y position below which the powerup will be destroyed")]
    public float despawnY = -6.8088f;

    private CircleCollider2D circleCollider;
    private Rigidbody2D rb2d;

    private void Awake()
    {
        // Ensure components exist and are configured for trigger collisions
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        rb2d.isKinematic = true;
        rb2d.gravityScale = 0f;
    }

    private void Update()
    {
        // Move downwards every frame
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Destroy when below threshold
        if (transform.position.y < despawnY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to objects with tag "player"; ignore all others
        if (other == null) return;

        if (other.CompareTag("Player"))
        {
            // Try to give shield to the player
            PlayerMovement2D player = other.GetComponent<PlayerMovement2D>();
            if (player == null)
            {
                // Try parent lookup
                player = other.GetComponentInParent<PlayerMovement2D>();
            }

            if (player != null)
            {
                player.GiveShield();
            }

            // Destroy the power-up after granting
            Destroy(gameObject);
        }
    }
}
