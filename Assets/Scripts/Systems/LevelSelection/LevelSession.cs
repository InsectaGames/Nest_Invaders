public static class LevelSession
{
    public static LevelDefinition SelectedLevel { get; private set; }

    public static void SelectLevel(LevelDefinition level)
    {
        SelectedLevel = level;
    }

    public static void ClearSelection()
    {
        SelectedLevel = null;
    }
}