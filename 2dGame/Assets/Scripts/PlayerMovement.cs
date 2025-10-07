using UnityEngine;

//This import is needed to access the mouse cursor position
using UnityEngine.InputSystem;

//Player movement script
public class PlayerMovement2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Camera mainCamera;

    public GameObject boomerangPrefab;

    public GameObject basicProjectilePrefab;

    //Set to default projectiles shoot interval because basic attack is the starting one
    public float shootInterval = 0.6f;
    public Transform shootPoint;

    private float shootTimer = 0f;

    //Basic attack is the default starting attack
    private string attackType = "Basic";


    //These will only get changed during balance patches
    public float basicProjectileShootInterval = 0.3f;
    public float boomerangShootInterval = 1f;

    //Activates on initialization
    private void Awake()
    {
        shootTimer = 0f;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        attackType = "Basic";
        SetShootInterval(basicProjectileShootInterval);
    }

    private void Update()
    {
        //Makes the player follow the mouse cursor for its movement
        playerMoving();
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (attackType == "Basic")
            {
                attackType = "Boomerang";
                SetShootInterval(boomerangShootInterval);
            }
            else if (attackType == "Boomerang")
            {
                attackType = "Basic";
                SetShootInterval(basicProjectileShootInterval);
            }
        }

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            if (attackType == "Basic")
            {
                attack(basicProjectilePrefab);
            }
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

    public void attack(GameObject projectileType)
    {
        if (projectileType == null || shootPoint == null)
        {
            Debug.LogWarning("ProjectileType or ShootPoint not assigned.");
            return;
        }

        Instantiate(projectileType, shootPoint.position, Quaternion.identity);
    }
    public void SetProjectileType(string newAttackType)
    {
        attackType = newAttackType;
    }

    public string GetProjectileType()
    {
        return attackType;
    }
}