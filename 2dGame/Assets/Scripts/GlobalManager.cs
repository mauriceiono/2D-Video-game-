using UnityEngine;

//Global script for managing game-wide settings and references.
//Going to make some scripts here to help with coding some powerups once we get there.
//Reference global functions with GlobalManager.Instance.FunctionName();
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    //Assign the Player GameObject here.
    public Transform playerTransform;

    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        //Debug.Log(GetPlayerPosition());
    }

    private void Start()
    {
        //Initializes the camera
        mainCamera = Camera.main;

        //Gets the player.
        //Used for getting the player position.
        //Player must have the "Player" tag at all times.
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
        else
        {
            //Will display a warning in the console if no player is found with the "Player" tag.
            Debug.LogWarning("Player GameObject with tag 'Player' not found.");
        }

    }

    //Gets the players current world position as a Vector2
    //Returns players Vector2 position, or 0,0 if playerTransform null / no player is found
    public Vector2 GetPlayerPosition()
    {
        if (playerTransform != null)
        {
            return playerTransform.position;
        }
        return Vector2.zero;
    }

    public Vector2 GetCameraPosition()
    {
        if (mainCamera != null)
        {
            return mainCamera.transform.position;
        }
        return Vector2.zero;
    }

    //Sets the players position to the given Vector2
    public void SetPlayerPosition(Vector2 newPosition)
    {
        if (playerTransform != null)
        {
            playerTransform.position = newPosition;
        }
    }
}