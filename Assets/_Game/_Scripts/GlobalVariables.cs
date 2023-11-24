
public static class GlobalVariables
{
    public const int LEVEL_TIME_IN_SECONDS = 60;


    public static GameState CurrentGameState { get; private set; }
    public static void UpdateGameState(GameState state)
    {
        CurrentGameState = state;
    }
}
public enum GameState
{
    Started = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
}