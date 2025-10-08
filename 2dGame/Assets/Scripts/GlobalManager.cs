using UnityEngine;
using System.Collections;

//Global script for managing game-wide settings and references.
//Going to make some scripts here to help with coding some powerups once we get there.
//Reference global functions with GlobalManager.Instance.FunctionName();
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    //Assign the Player GameObject here.
    public Transform playerTransform;

    private Camera mainCamera;

    //Array of enemy prefabs to spawn from.
    //Assign enemy prefabs in the inspector.
    public GameObject[] enemyPrefabs;

    //Amount of enemies to spawn per wave.
    public int[] enemiesPerWave;

    //Where the enemies will spawn from.
    public Transform[] spawnPoints;

    private int currentWave = 0;
    private bool waveInProgress = false;

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

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Enemy prefabs not assigned in GlobalManager.");
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Spawn points not assigned in GlobalManager.");
        }

        if (enemiesPerWave == null || enemiesPerWave.Length == 0)
        {
            Debug.LogWarning("Enemies per wave array is empty in GlobalManager.");
        }

        //Start first enemy wave
        StartCoroutine(SpawnWaveRoutine());

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

    private IEnumerator SpawnWaveRoutine()
    {
        while (currentWave < enemiesPerWave.Length)
        {
            while (true)  // Runs infinitely, can be limited if needed
            {
                waveInProgress = true;

                int enemiesToSpawn = CalculateEnemiesCount(currentWave);
                SpawnWave(enemiesToSpawn);

                yield return new WaitUntil(() => AreAllEnemiesDead());

                waveInProgress = false;
                currentWave++;

                // Wait 6 seconds before starting next wave
                yield return new WaitForSeconds(6f);
            }
        }

        Debug.Log("All waves completed!");
    }

    private void SpawnWave(int waveIndex)
    {
        for (int i = 0; i < CalculateEnemiesCount(waveIndex); i++)
        {
            GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyToSpawn, spawnPoint.position, spawnPoint.rotation);
        }

        Debug.Log($"Wave {currentWave + 1} spawned with {CalculateEnemiesCount(waveIndex)} enemies.");
    }

    private bool AreAllEnemiesDead()
    {
        //Checks if there are no more GameObjects tagged as "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length == 0;
    }
    
    private int CalculateEnemiesCount(int waveIndex)
    {
        int waveNumber = waveIndex + 1; // Wave 1-based
        int Z = waveNumber / 3; // Integer division: increments every 3 waves
        return waveNumber + Z;
    }
}