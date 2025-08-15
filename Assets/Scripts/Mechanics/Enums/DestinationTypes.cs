namespace NeonLadder.Mechanics.Enums
{
    /// <summary>
    /// Types of destination determination for scene transitions
    /// </summary>
    public enum DestinationType
    {
        Procedural,     // Use procedural generation
        Manual,         // Use override scene name
        NextInPath      // Use next node in path
    }
}