using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    #region Varibales
    [SerializeField] private LevelTimer _levelTimer;
    [Space(5)]
    [Header("Game Over")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;

    [Space(5)]
    [Header("Start panel")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _gameQuitButton;

    [Space(5)]
    [Header("Level Complete")]
    [SerializeField] private TextMeshProUGUI _gameCompleteMsgTxt;
    [SerializeField] private GameObject _scorePanel;
    [SerializeField] private TextMeshProUGUI _scoreTxt;
    [SerializeField] private Button _levelExitBtn;

    #endregion Varibales

    #region Unity Methods
    private void OnEnable()
    {
        GlobalEventHandler.OnGameOver += Callback_On_Game_Over;
        GlobalEventHandler.OnLevelCompleted += Callback_On_Game_Complete;

        _startButton.onClick.AddListener(OnClickStart);
        _gameQuitButton.onClick.AddListener(OnClickQuitGame);

        _restartButton.onClick.AddListener(OnClickRestart);
        _levelExitBtn.onClick.AddListener(OnClickRestart);
    }
    private void OnDisable()
    {
        GlobalEventHandler.OnGameOver -= Callback_On_Game_Over;
        GlobalEventHandler.OnLevelCompleted -= Callback_On_Game_Complete;

        _startButton.onClick.RemoveListener(OnClickStart);
        _gameQuitButton.onClick.RemoveListener(OnClickQuitGame);

        _restartButton.onClick.RemoveListener(OnClickRestart);
        _levelExitBtn.onClick.RemoveListener(OnClickRestart);
    }
    #endregion Unity Methods

    #region Public Methods
    #endregion Public Methods

    #region Private Methods
    private void OnClickStart()
    {

        GlobalEventHandler.OnGameStartRequested?.Invoke();
        _startPanel.SetActive(false);
    }
    private void OnClickQuitGame()
    {
        Application.Quit();
    }

    private void OnClickRestart()
    {
        SceneManager.LoadSceneAsync(0);
    }
    #endregion Private Methods

    #region Callbacks
    private void Callback_On_Game_Over()
    {
        Debug.Log($"Game Over!!!!");
        _gameOverPanel.SetActive(true);
    }
    private void Callback_On_Game_Complete()
    {
        Debug.Log($"Level Complete");
        _gameCompleteMsgTxt.SetText($"Level Completed");
        _scorePanel.gameObject.SetActive(true);
        _scoreTxt.SetText($"{_levelTimer.GetRemaingTimeInSeconds() * 100}");
        _gameOverPanel.SetActive(true);
    }
    #endregion Callbacks
}
