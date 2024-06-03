using NeonLadder.Mechanics.Controllers;

public static class PlayerActionDebugging
{
    public static string GetPlayerActionParameters(Player player, PlayerAction playerActions)
    {
        var result = "Action Parms\r\n";
        result += $"VelocityX: {player.velocity.x}\n";
        result += $"VelocityY: {player.velocity.y}\n";
        result += $"IsGrounded: {player.IsGrounded}\n";
        result += $"IsJumping: {playerActions.IsJumping}\n";
        result += $"jumpState: {playerActions.jumpState}\n";
        result += $"sprintDuration: {playerActions.sprintDuration}\n";
        result += $"sprintState: {playerActions.sprintState}\n";
        result += $"IsSprinting: {playerActions.IsSprinting}\n";
        return result;
    }
}
