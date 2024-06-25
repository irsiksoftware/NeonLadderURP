using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;

namespace NeonLadder.Events
{
    public abstract class BaseGameEvent<T> : Simulation.Event<T> where T : Simulation.Event<T>, new()
    {
        protected PlatformerModel model => Game.Instance.model;
    }
}
