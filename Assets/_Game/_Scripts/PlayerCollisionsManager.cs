using UnityEngine;

public class PlayerCollisionsManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GlobalVariables.CurrentGameState != GameState.Running) return;
        switch (other.tag)
        {
            case "Sword":
                //GlobalEventHandler.OnGameOver?.Invoke();
                break;
            case "ExitPoint":
                GlobalEventHandler.OnLevelCompleted?.Invoke();
                break;
        }
    }
}
