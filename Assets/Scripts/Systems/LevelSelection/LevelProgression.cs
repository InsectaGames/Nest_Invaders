using UnityEngine;

public static class LevelProgression
{
    private const string UNLOCKED_PREFIX = "LEVEL_UNLOCKED_";
    private const string COMPLETED_PREFIX = "LEVEL_COMPLETED_";

    private const string PENDING_ANIMATION_KEY =
        "PENDING_UNLOCK_ANIMATION";

    // Comprueba si un nivel está desbloqueado.
    public static bool IsUnlocked(LevelDefinition level, bool unlockedByDefault = false)
    {
        if (level == null)
            return false;

        string key = UNLOCKED_PREFIX + level.ID;

        // Desbloqueo inicial de los primeros niveles.
        if (!PlayerPrefs.HasKey(key) && unlockedByDefault)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // Desbloquea un nivel y devuelve true solamente si acaba de desbloquearse.
    public static bool UnlockLevel(LevelDefinition level)
    {
        if (level == null)
            return false;

        string key = UNLOCKED_PREFIX + level.ID;

        bool wasUnlocked = PlayerPrefs.GetInt(key, 0) == 1;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        return !wasUnlocked;
    }

    // Comprueba si un nivel ya fue completado.
    public static bool IsCompleted(LevelDefinition level)
    {
        if (level == null)
            return false;

        string key = COMPLETED_PREFIX + level.ID;

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // Registra la victoria y desbloquea el siguiente nivel.
    public static bool CompleteLevel(LevelDefinition level)
    {
        if (level == null)
            return false;

        string completedKey = COMPLETED_PREFIX + level.ID;

        PlayerPrefs.SetInt(completedKey, 1);

        bool newlyUnlocked = false;

        if (level.nextLevel != null)
        {
            newlyUnlocked = UnlockLevel(level.nextLevel);

            // Guardar qué botón debe reproducir la animación.
            if (newlyUnlocked)
            {
                PlayerPrefs.SetInt(PENDING_ANIMATION_KEY, level.ID);
            }
        }

        PlayerPrefs.Save();

        return newlyUnlocked;
    }

    // Solo permite reproducir una vez la animación pendiente.
    public static bool ConsumePendingAnimation(int completedLevelID)
    {
        if (!PlayerPrefs.HasKey(PENDING_ANIMATION_KEY))
            return false;

        int pendingID = PlayerPrefs.GetInt(PENDING_ANIMATION_KEY, -1);

        if (pendingID != completedLevelID)
            return false;

        PlayerPrefs.DeleteKey(PENDING_ANIMATION_KEY);
        PlayerPrefs.Save();

        return true;
    }
}
