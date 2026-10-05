using System;
using System.Collections.Generic;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Core.Spinning;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Core.Game
{
    public interface IWheelGame
    {
        event Action<GameState> StateChanged;
        event Action<int> ZoneChanged;
        event Action<SpinResult> SpinStarted;
        event Action<RewardStack> RewardGained;

        GameState State { get; }
        int CurrentZone { get; }
        ZoneType CurrentZoneType { get; }
        WheelDefinition CurrentWheel { get; }
        IReadOnlyList<RewardStack> CollectedRewards { get; }
        int ReviveCost { get; }

        bool CanSpin { get; }
        bool CanLeave { get; }
        bool CanRevive { get; }

        bool TryStartSpin();

        /// <summary>Applies the pending spin outcome once its presentation has finished.</summary>
        bool TryCompleteSpin();

        bool TryLeave();

        bool TryRevive();

        bool TryGiveUp();

        bool TryRestart();
    }
}
