using UnityEngine;

//This import is needed to access the mouse cursor position
using UnityEngine.InputSystem;

//Player movement script
public class PlayerMovement2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Camera mainCamera;

    public float attackTimer;

    //Activates on initialization
    private void Awake()
    {
        attackTimer = 0f;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //Makes the player follow the mouse cursor for its movement
        playerMoving();
    }

    private void FixedUpdate()
    {
        //Attack timer countdown
        if (attackTimer <= 0f)
        {
            //Ready to attack
            SetAttackSpeed(100f);
            Object friendlyprojectile = Resources.Load("Projectiles/FriendlyProjectileBasic");
            attack(friendlyprojectile);
        }
        else
        {
            attackTimer -= 1f;
        }
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

    public void SetAttackSpeed(float newSpeed)
    {
        attackTimer = newSpeed;
    }

    public float GetAttackSpeed()
    {
        return attackTimer;
    }

    public void attack(Object projectileType)
    {
        // Offset distance in front of the player
    float spawnOffset = 0.5f; // change if needed
    Vector3 spawnPosition = transform.position + transform.up * spawnOffset;

    // Spawn projectile at the offset position, spawn projectile in front of player model 
    GameObject projectile = Instantiate(projectileType, spawnPosition, transform.rotation) as GameObject;

    }
}