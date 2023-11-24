using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    #region Variables
    [SerializeField] private TextMeshProUGUI m_timerTxt;
    private int m_timerCounter = 0;
    private int m_totalTimeInSeconds;
    private bool m_isTimerCompleted;
    private bool m_isTimerRunning = false;

    #endregion Variables


    #region Unity Methods
    private void OnEnable()
    {
        GlobalEventHandler.OnGameStarted += Callback_On_Game_Started;
        GlobalEventHandler.OnLevelCompleted += Callback_On_Level_Complete;
        GlobalEventHandler.OnGameOver += Callback_On_Level_Complete;
    }
    private void OnDisable()
    {

        GlobalEventHandler.OnGameStarted -= Callback_On_Game_Started;
        GlobalEventHandler.OnLevelCompleted -= Callback_On_Level_Complete;
        GlobalEventHandler.OnGameOver -= Callback_On_Level_Complete;
    }
    #endregion Unity Methods


    #region Public Methods 
    public void InitTimer(int timeInSeconds)
    {
        m_totalTimeInSeconds = m_timerCounter = timeInSeconds;
        m_isTimerCompleted = false;
        m_isTimerRunning = false;
        if (m_timerTxt)
            m_timerTxt.text = GetFormattedSeconds(m_timerCounter);
    }
    public void InitTimerAndStartTimer(int timeInSeconds)
    {
        InitTimer(timeInSeconds);
        StartTimer();
    }
    public void StartTimer()
    {
        if (m_isTimerCompleted || m_isTimerRunning) return;
        m_isTimerRunning = true;
        Debug.Log($"Timer Started::");
        InvokeRepeating(nameof(_Tick), 1, 1);
    }
    public void StopTimer()
    {
        Debug.Log($"Timer Stopped::");
        m_isTimerRunning = false;
        CancelInvoke(nameof(_Tick));
    }
    public void RestartTimer()
    {
        StopTimer();
        InitTimerAndStartTimer(m_totalTimeInSeconds);
    }
    public void RestartTimer(int newTimeInSeconds)
    {
        m_totalTimeInSeconds = newTimeInSeconds;
        StopTimer();
        InitTimerAndStartTimer(m_totalTimeInSeconds);
    }
    public int GetRemaingTimeInSeconds()
    {
        return m_timerCounter;
    }
    public int GetElapsedTimeInSeconds() => m_totalTimeInSeconds - m_timerCounter;
    #endregion Public Methods 

    #region Private Methods 
    private void _Tick()
    {
        m_timerCounter--;
        if (m_timerCounter <= 0)
        {
            m_timerCounter = 0;
            m_isTimerCompleted = true;
            StopTimer();
            GlobalEventHandler.OnLevelTimerIsCompleted?.Invoke();
        }
        if (m_timerTxt)
            m_timerTxt.text = GetFormattedSeconds(m_timerCounter);
    }
    private string GetFormattedSeconds(int seconds)
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);

        string formattedString = "";
        if (timeSpan.Days > 0)
            formattedString = timeSpan.ToString(@"d\d\ h\h");
        else if (timeSpan.Hours > 0)
            formattedString = timeSpan.ToString(@"h\h\ m\m");
        else
            formattedString = timeSpan.ToString(@"mm\:ss");
        return formattedString;
    }
    #endregion Private Methods 

    #region Callbacks
    private void Callback_On_Game_Started()
    {
        InitTimerAndStartTimer(GlobalVariables.LEVEL_TIME_IN_SECONDS);
    }
    private void Callback_On_Level_Complete()
    {
        StopTimer();
    }
    #endregion Callbacks
}
