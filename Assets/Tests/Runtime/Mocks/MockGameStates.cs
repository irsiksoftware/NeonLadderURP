namespace NeonLadder.Testing.Mocks
{
    /// <summary>
    /// Mock GameStates enum for testing purposes.
    /// This allows our Steam tests to run without compilation dependencies.
    /// 
    /// Author: @storm - Weather Control Specialist  
    /// "Sometimes we must create our own weather patterns to test in controlled conditions!"
    /// </summary>
    public enum MockGameStates
    {
        Inactive,
        Active,
        Winner,
        Loser,
        Paused
    }
}