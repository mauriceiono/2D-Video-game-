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
    // Shield state
    private bool hasShield = false;
    // Visuals
    private SpriteRenderer spriteRenderer;
    public Sprite playerSprite; // assign in inspector or will attempt to load from Resources/Sprites/player
    public Sprite playerWithShieldSprite; // assign in inspector or will attempt to load from Resources/Sprites/playerwithshield
    private Sprite originalSprite;
    // Optional: if an Animator is driving the sprite, it can overwrite sprite changes every frame.
    // Set this true if you want the script to temporarily disable the Animator while shield is active.
    public bool disableAnimatorWhenShielded = false;
    private Animator animator;

    //Activates on initialization
    private void Awake()
    {
        attackTimer = 0f;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        // Sprite renderer setup
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        else
        {
            Debug.LogWarning("PlayerMovement2D: no SpriteRenderer found on player GameObject.");
        }

        animator = GetComponent<Animator>();
        if (animator != null)
        {
            // nothing by default; we only disable it if configured
        }

        // Try to auto-load sprites from Resources if not assigned in inspector
        if (playerSprite == null)
        {
            playerSprite = Resources.Load<Sprite>("Sprites/player");
            if (playerSprite == null)
            {
                // keep originalSprite as fallback
            }
        }

        if (playerWithShieldSprite == null)
        {
            playerWithShieldSprite = Resources.Load<Sprite>("Sprites/playerwithshield");
        }
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

    // Shield control
    public void GiveShield()
    {
        hasShield = true;
        // You can add visual/audio feedback here
        Debug.Log("Player: Shield granted");
        // Swap to shield sprite if available
        if (spriteRenderer == null)
        {
            Debug.LogWarning("GiveShield: no SpriteRenderer to change sprite on.");
        }
        else if (playerWithShieldSprite == null)
        {
            Debug.LogWarning("GiveShield: playerWithShieldSprite is not assigned and could not be loaded from Resources.");
        }
        else
        {
            spriteRenderer.sprite = playerWithShieldSprite;
        }

        // Optionally disable Animator to prevent it from overwriting the sprite each frame
        if (disableAnimatorWhenShielded && animator != null)
        {
            animator.enabled = false;
        }
    }

    public void RemoveShield()
    {
        hasShield = false;
        Debug.Log("Player: Shield removed");
        // Swap back to normal sprite
        if (spriteRenderer == null)
        {
            Debug.LogWarning("RemoveShield: no SpriteRenderer to change sprite on.");
        }
        else if (playerSprite != null)
        {
            spriteRenderer.sprite = playerSprite;
        }
        else if (originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }

        // Re-enable animator if we disabled it earlier
        if (disableAnimatorWhenShielded && animator != null)
        {
            animator.enabled = true;
        }
    }

    public bool HasShield()
    {
        return hasShield;
    }
}