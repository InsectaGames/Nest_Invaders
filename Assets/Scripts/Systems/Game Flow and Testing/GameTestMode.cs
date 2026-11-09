using UnityEngine.SceneManagement;

public static class GameTestMode
{
    public const string BehaviourArenaScene = "Test_behaviour_arena";

    public static bool IsBehaviourTestScene =>
        SceneManager.GetActiveScene().name == BehaviourArenaScene;
}