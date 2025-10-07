using UnityEngine;

/// <summary>
/// Helper script responsible for spawning the ShieldPowerUp prefab from
/// Assets/Resources/PowerUps/ShieldPowerUp.prefab.
///
/// Usage:
/// - Call SpawnAtTop() to spawn the prefab at the top-center of the screen.
/// - Call SpawnAtPosition(worldPos) to spawn it at a specific world position
///   (useful when an enemy dies and should drop the power-up).
/// </summary>
public class ShieldPowerUpScript : MonoBehaviour
{
    // Cached prefab reference (loaded on demand)
    private static GameObject shieldPrefab;

    public float attackTimer;
    private void Awake()
    {
        attackTimer = 0f;
    }
    /// <summary>
    /// Loads the ShieldPowerUp prefab from Resources/PowerUps if needed.
    /// </summary>
    private static GameObject GetShieldPrefab()
    {
        if (shieldPrefab == null)
        {
            shieldPrefab = Resources.Load<GameObject>("PowerUps/ShieldPowerUp");
            if (shieldPrefab == null)
            {
                Debug.LogError("ShieldPowerUpScript: Could not find prefab at Resources/PowerUps/ShieldPowerUp");
            }
        }
        return shieldPrefab;
    }

    /// <summary>
    /// Spawns the ShieldPowerUp prefab at the top-center of the screen.
    /// verticalOffset moves the spawn point in world units (negative moves it down).
    /// By default it leaves a small gap (-0.5f) so future enemy-kill spawns can appear
    /// slightly below this area if needed.
    /// </summary>
    /// <param name="verticalOffset">Vertical offset in world units (default -0.5f)</param>
    public void SpawnAtTop(float verticalOffset = -0.5f)
    {
        GameObject prefab = GetShieldPrefab();
        if (prefab == null) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("ShieldPowerUpScript: No main camera found.");
            return;
        }

        Vector3 topCenterScreen = new Vector3(Screen.width * 0.5f, Screen.height, 0f);
        Vector3 worldPos = cam.ScreenToWorldPoint(topCenterScreen);
        worldPos.z = 0f;
        worldPos += Vector3.up * verticalOffset;

        Instantiate(prefab, worldPos, Quaternion.identity);
    }

    /// <summary>
    /// Spawns the ShieldPowerUp prefab at a specific world position. Use this when
    /// you want to spawn a power-up where an enemy was destroyed.
    /// </summary>
    /// <param name="worldPosition">World space position to spawn the prefab at.</param>
    public void SpawnAtPosition(Vector3 worldPosition)
    {
        GameObject prefab = GetShieldPrefab();
        if (prefab == null) return;

        Vector3 pos = worldPosition;
        pos.z = 0f;
        Instantiate(prefab, pos, Quaternion.identity);
    }

    // Unity start hook — called when the GameObject is instantiated or scene starts
    private void Start()
    {
        SpawnAtTop();
    }

    // Movement for the spawned prefab is handled by a separate component
    // (attach ShieldPowerUpMovement.cs to the prefab in Assets/Resources/PowerUps)
}
