using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb;
    public int health = 15;

    //Activates on initialization
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        //Debug.Log(GlobalFunctions.GetPlayerPosition());
    }
    private void FixedUpdate()
    {

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
            //Destroy this enemy

        }
    }

    //Gets and returns the enemy position
    public Vector2 GetEnemyPosition()
    {
        return rb.position;
    }

    //Makes the enemy move to this position
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
