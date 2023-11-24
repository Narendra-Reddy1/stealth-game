using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Varibales

    #endregion Varibales

    #region Unity Methods
    private void Start()
    {
        GlobalVariables.UpdateGameState(GameState.Started);
    }
    private void OnEnable()
    {
        GlobalEventHandler.OnGameOver += Callback_On_Game_Over;
        GlobalEventHandler.OnLevelCompleted += Callback_On_Game_Complete;
        GlobalEventHandler.OnLevelTimerIsCompleted += Callback_On_Level_Timer_Complete;
    }
    private void OnDisable()
    {
        GlobalEventHandler.OnGameOver -= Callback_On_Game_Over;
        GlobalEventHandler.OnLevelCompleted -= Callback_On_Game_Complete;
        GlobalEventHandler.OnLevelTimerIsCompleted -= Callback_On_Level_Timer_Complete;
    }
    #endregion Unity Methods

    #region Public Methods
    #endregion Public Methods

    #region Private Methods
    #endregion Private Methods

    #region Callbacks
    private void Callback_On_Game_Over()
    {
        Debug.Log($"Game Over!!!!");
        GlobalVariables.UpdateGameState(GameState.Failed);
    }
    private void Callback_On_Level_Timer_Complete()
    {
        GlobalEventHandler.OnGameOver?.Invoke();
    }

    private void Callback_On_Game_Complete()
    {
        GlobalVariables.UpdateGameState(GameState.Completed);
    }
    #endregion Callbacks    
}
