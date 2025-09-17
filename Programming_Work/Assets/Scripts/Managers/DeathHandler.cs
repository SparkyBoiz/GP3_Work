using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    public GameObject gameOverUI;

    public void OnPlayerDeath()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        
    }
}