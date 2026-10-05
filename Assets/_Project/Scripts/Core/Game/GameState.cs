namespace Vertigo.Wheel.Core.Game
{
    public enum GameState
    {
        /// <summary>Waiting for the player to spin or leave.</summary>
        Ready = 0,

        /// <summary>The outcome is decided and the wheel is animating towards it.</summary>
        Spinning = 1,

        /// <summary>A bomb was hit; the player can revive or give up.</summary>
        Bombed = 2,

        /// <summary>The player gave up after a bomb and lost every reward.</summary>
        Lost = 3,

        /// <summary>The player walked away with the collected rewards.</summary>
        Collected = 4
    }
}
