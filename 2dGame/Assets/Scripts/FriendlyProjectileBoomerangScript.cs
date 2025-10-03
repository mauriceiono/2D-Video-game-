using UnityEngine;

//Boomerang projectile script
/*
How it works:
-When spawned, it moves upwards
-After being far away enough from the player, it returns to the player
-The boomerang constantly increases in speed when returning to the player
--Ensures the player always catches it, reducing lag

Implementation:
-Replace all "GetFakePlayer1Position()" calls with "GetPlayerPosition()"
-Replace all "FakePlayer" tags with "Player" tags
-Give the player a shootPoint
--1: right click player in Hierarchy
--2: "create empty" option in menu
--3: rename it to "shootPoint"
--4: view the shootPoint in the inspector
--5: set its position to (0, 1, 0)
---It will break if the shootPoint is too close to the player by making the boomerang not move until reaching ax distance
--6: assign the shootPoint to the PlayerMovement script in the inspector
*/
public class FriendlyProjectileBoomerangScript : MonoBehaviour
{
    public float speed = 3f;
    public float maxDistance = 5f; //Max distance from player before returning
    public int damage = 10;

    private Vector2 playerStartPos;
    private Vector2 direction;
    private bool returning = false;

    private Rigidbody2D rb;

    //Acceleration when returning to player
    public float returnAcceleration = 3f;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStartPos = GlobalFunctions.GetFakePlayer1Position();
        Vector2 currentPos = rb.position;
        direction = (currentPos - playerStartPos).normalized;
        rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        Vector2 playerPos = GlobalFunctions.GetFakePlayer1Position();
        float distanceFromPlayer = Vector2.Distance(rb.position, playerPos);

        if (!returning && distanceFromPlayer >= maxDistance)
        {
            returning = true;
        }

        if (returning)
        {
            direction = (playerPos - rb.position).normalized;

            //Increase speed by acceleration * deltaTime * 1.5 each frame when returning
            speed += returnAcceleration * Time.deltaTime * 1.5f;

            rb.linearVelocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Check if collided with an enemy
        if (collision.CompareTag("Enemy"))
        {
            var enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            //Do not destroy projectile, so it pierces enemies
        }
        else if (collision.CompareTag("FakePlayer") && returning)
        {
            // When boomerang returns to player, destroy it
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Ensure destruction if boomerang stays overlapping player on return
        if (collision.CompareTag("FakePlayer") && returning)
        {
            Destroy(gameObject);
        }
    }
}
    