namespace NeonLadder.Mechanics.Enums
{
    /// <summary>
    /// Standard spawn point types for scene transitions
    /// Simple directional naming
    /// </summary>
    public enum SpawnPointType
    {
        None,           // No spawn point (disable spawning)
        Auto,           // Let system decide based on transition direction
        Default,        // Use "Default" spawn point
        Center,         // Center/doorway spawn point
        Left,           // Left side spawn point
        Right,          // Right side spawn point
        BossArena,      // Use "BossArena" spawn point (for boss scenes)
        Custom          // Use custom name (shows text field)
    }
}