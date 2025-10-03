using UnityEngine;

//This import is needed to access the mouse cursor position
using UnityEngine.InputSystem;

//Player movement script
public class FakePlayer1Behaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Camera mainCamera;

    public float attackTimer;

    public GameObject boomerangPrefab;
    public float shootInterval = 1f;
    public Transform shootPoint;

    private float shootTimer = 0f;

private void Awake()
    {
        attackTimer = 0f;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {

        playerMoving();

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            ShootBoomerang();
            shootTimer = 0f;
        }
    }

    private void ShootBoomerang()
    {
        if (boomerangPrefab == null || shootPoint == null)
        {
            Debug.LogWarning("BoomerangPrefab or ShootPoint not assigned.");
            return;
        }

        GameObject boomerangInstance = Instantiate(boomerangPrefab, shootPoint.position, Quaternion.identity);

        // Set initial velocity upwards
        Rigidbody2D rb = boomerangInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * 10f;  // Initial speed upwards matches boomerang speed in script
        }
    }

    public void playerMoving()
    {
        //Get the current position of the mouse cursor
        Vector2 cursorScreenPos = Mouse.current.position.ReadValue();

        //Convert the screen position to world position
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(cursorScreenPos);

        SetPlayerPosition(new Vector2(worldPos.x + 4f, worldPos.y));
    }

    public void SetPlayerPosition(Vector2 newPosition)
    {
        rb.position = newPosition;

        // Ensures physics position sync
        rb.MovePosition(newPosition);
    }
}