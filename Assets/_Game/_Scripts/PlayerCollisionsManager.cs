using UnityEngine;

public class PlayerCollisionsManager : MonoBehaviour
{
    private int _swordHitCounter = 3;
    private void OnTriggerEnter(Collider other)
    {
        if (GlobalVariables.CurrentGameState != GameState.Running) return;
        switch (other.tag)
        {
            case "Sword":
                if (_swordHitCounter-- <= 0)
                    GlobalEventHandler.OnGameOver?.Invoke();
                break;
            case "ExitPoint":
                GlobalEventHandler.OnLevelCompleted?.Invoke();
                break;
        }
    }
}
