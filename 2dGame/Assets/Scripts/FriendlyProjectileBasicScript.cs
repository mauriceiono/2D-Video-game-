using UnityEngine;

public class FriendlyProjectileBasicScript : MonoBehaviour
{
    private Rigidbody2D rb;

    //Projectile speed
    public float speed = 10f;

    //Projectile lifespan in seconds(how long it lasts before despawning)
    public float lifetime = 2f;

    //Damage it deals
    public int damage = 10;

    //Activates on initialization
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        // Set the projectile to move forward at the specified speed
        rb.linearVelocity = transform.up * speed;

        // Destroy the projectile after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    void Start()
    {
        rb.gravityScale = 0f;
    }

    public Vector2 GetProjectilePosition()
    {
        return rb.position;
    }

    public void SetProjectilePosition(Vector2 newPosition)
    {
        rb.position = newPosition;

        // Ensures physics position sync
        rb.MovePosition(newPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal damage to the enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Destroy the projectile after hitting an enemy
            Destroy(gameObject, lifetime);
        }
    }
}
