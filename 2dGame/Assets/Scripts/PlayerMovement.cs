using UnityEngine;

//This import is needed to access the mouse cursor position
using UnityEngine.InputSystem;

//Player movement script
public class PlayerMovement2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Camera mainCamera;

    //Assigned in inspector.
    //The prefab for the boomerang projectile.
    public GameObject boomerangPrefab;

    //Assigned in inspector.
    //Prefab for the basic attack projectile.
    public GameObject basicProjectilePrefab;

    //Set to default projectiles shoot interval because basic attack is the starting one
    public float shootInterval = 0.6f;
    public Transform shootPoint;

    //Constantly goes up. Once it equals shootInterval, the projectile for the attack is shot and this resets to 0.
    private float shootTimer = 0f;

    //Basic attack is the default starting attack
    private string attackType = "Basic";


    //These will only get changed during balance patches, no need to make functions for these.
    public float basicProjectileShootInterval = 0.3f;
    public float boomerangShootInterval = 1f;

    //Activates on initialization
    private void Awake()
    {
        shootTimer = 0f;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        //Player starts with basic attack when starting the game
        attackType = "Basic";
        //The basic attack has less damage but shoots faster than the boomerang, so its shoot interval is changed accordingly.
        SetShootInterval(basicProjectileShootInterval);
    }

    private void Update()
    {
        //Makes the player follow the mouse cursor for its movement
        playerMoving();

        //Press E to swap weapons. This should be deleted later after a power ups system is made.
        if (Input.GetKeyDown(KeyCode.E))
        {

            //If using basic attack swap to boomerang attack
            if (attackType == "Basic")
            {
                attackType = "Boomerang";
                SetShootInterval(boomerangShootInterval);
            }

            //If using boomerang attack, swap to basic attack
            else if (attackType == "Boomerang")
            {
                attackType = "Basic";
                SetShootInterval(basicProjectileShootInterval);
            }
        }

        //Shoot timer goes up
        shootTimer += Time.deltaTime;

        //Player is ready to attack
        if (shootTimer >= shootInterval)
        {

            //If using a basic attack, shoot the basic projectile
            if (attackType == "Basic")
            {
                attack(basicProjectilePrefab);
            }

            //If using boomerang attack, shoot a boomerang
            else if (attackType == "Boomerang")
            {
                attack(boomerangPrefab);
            }
            shootTimer = 0f;
        }
    }

    private void FixedUpdate()
    {

    }

    //Gets the current position of the player in world coordinates.
    //Returns players current position as a Vector2.
    public Vector2 GetPlayerPosition()
    {
        return rb.position;
    }

    //Sets the player's position in world coordinates.
    //newPosition is the new position to move the player to
    public void SetPlayerPosition(Vector2 newPosition)
    {
        rb.position = newPosition;

        // Ensures physics position sync
        rb.MovePosition(newPosition);
    }


    //Player movement function
    public void playerMoving()
    {
        //Get the current position of the mouse cursor
        Vector2 cursorScreenPos = Mouse.current.position.ReadValue();

        //Convert the screen position to world position
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(cursorScreenPos);

        //Move the player to the world position of the mouse cursor
        SetPlayerPosition(worldPos);

        //Set the players position 4 units to the right of the mouse cursor

        //SetPlayerPosition(new Vector2(worldPos.x + 4f, worldPos.y));
    }

    //More getters and setters
    public void SetAttackTimer(float newSpeed)
    {
        shootTimer = newSpeed;
    }

    public float GetAttackTimer()
    {
        return shootTimer;
    }

    public float GetShootInterval()
    {
        return shootInterval;
    }

    public void SetShootInterval(float newInterval)
    {
        shootInterval = newInterval;
    }

    //Creates a projectile for an attack.
    public void attack(GameObject projectileType)
    {

        //Makes sure the projectile to fire has been properly assigned in the inspector
        if (projectileType == null || shootPoint == null)
        {
            Debug.LogWarning("ProjectileType or ShootPoint not assigned.");
            return;
        }

        //Create the projectile
        Instantiate(projectileType, shootPoint.position, Quaternion.identity);
    }

    //Used for changing attack types.
    public void SetProjectileType(string newAttackType)
    {
        attackType = newAttackType;
    }

    public string GetProjectileType()
    {
        return attackType;
    }
}
