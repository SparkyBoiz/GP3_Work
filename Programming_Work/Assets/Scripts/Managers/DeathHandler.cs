using UnityEngine;
using Core;

public class DeathHandler : Singleton<DeathHandler>
{
    [Tooltip("UI element to enable when the player dies.")]
    public GameObject gameOverUI;

    public void OnPlayerDeath()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }
}