namespace NeonLadder.Mechanics.Enums
{
    /// <summary>
    /// Standard spawn point types for scene transitions
    /// </summary>
    public enum SpawnPointType
    {
        Auto,           // Let SpawnPointManager decide based on direction
        Default,        // Use "Default" spawn point
        FromLeft,       // Use "FromLeft" spawn point  
        FromRight,      // Use "FromRight" spawn point
        BossArena,      // Use "BossArena" spawn point (for boss scenes)
        Custom          // Use custom name (shows text field)
    }
}