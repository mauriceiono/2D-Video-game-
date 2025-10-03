using UnityEngine;

//For global functions
//Made this because the global entity didn't function as I expected
//I left it in the project just in case
///Usage: GlobalFunctions.FunctionName()
public static class GlobalFunctions
{
    //Gets the players current position
    public static Vector2 GetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 pos2D = player.transform.position;
            // Return only the x and y components for 2D.
            return new Vector2(pos2D.x, pos2D.y);
        }
        else
        {
            Debug.LogWarning("Player object with tag 'Player' not found in the scene.");
            return Vector2.zero;
        }
    }

    public static Vector2 GetFakePlayer1Position()
    {
        GameObject player = GameObject.FindGameObjectWithTag("FakePlayer");
        if (player != null)
        {
            Vector2 pos2D = player.transform.position;
            // Return only the x and y components for 2D.
            return new Vector2(pos2D.x, pos2D.y);
        }
        else
        {
            Debug.LogWarning("FakePlayer not found in the scene.");
            return Vector2.zero;
        }
    }
}