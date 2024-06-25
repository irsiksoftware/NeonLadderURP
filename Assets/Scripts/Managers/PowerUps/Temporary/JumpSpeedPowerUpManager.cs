using NeonLadder.Common;
using NeonLadder.Mechanics.Collectibles;
using NeonLadder.Mechanics.Enums;
using UnityEngine;

public class JumpSpeedPowerUpManager : BasePowerUpManager
{

    protected override void Start()
    {
        base.Start();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject.name != "Enemy")
        {

            Buff newBuff;
            if (buffType == BuffTypes.Permanent)
            {
                newBuff = new Buff(BuffTypes.Permanent, 0, Magnitude); // Permanent buff
                ApplyBuff(newBuff);
            }
            else
            {
                newBuff = new Buff(BuffTypes.Temporary, PowerUpDuration, Magnitude); // Temporary buff
                ApplyBuff(newBuff);
            }
        }
    }

    protected override void ApplyTemporaryBuffEffect(Buff buff)
    {
        Constants.JumpTakeOffSpeed += buff.Magnitude * Constants.DefaultJumpTakeOffSpeed;
    }

    protected override void RemoveBuffEffect(Buff buff)
    {
        Constants.JumpTakeOffSpeed = Constants.DefaultJumpTakeOffSpeed;
    }
}
