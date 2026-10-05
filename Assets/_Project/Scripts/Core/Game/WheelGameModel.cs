using System;
using System.Collections.Generic;
using Vertigo.Wheel.Core.Economy;
using Vertigo.Wheel.Core.Rewards;
using Vertigo.Wheel.Core.Spinning;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Core.Game
{
    public sealed class WheelGameModel : IWheelGame
    {
        private readonly IZoneRules _zoneRules;
        private readonly IWheelProvider _wheelProvider;
        private readonly ISpinResolver _spinResolver;
        private readonly IRewardInventory _inventory;
        private readonly IReviveService _reviveService;
        private readonly IRewardClaimHandler _claimHandler;

        private SpinResult _pendingSpin;

        public event Action<GameState> StateChanged;
        public event Action<int> ZoneChanged;
        public event Action<SpinResult> SpinStarted;
        public event Action<RewardStack> RewardGained;

        public GameState State { get; private set; }
        public int CurrentZone { get; private set; }
        public ZoneType CurrentZoneType { get; private set; }
        public WheelDefinition CurrentWheel { get; private set; }
        public IReadOnlyList<RewardStack> CollectedRewards => _inventory.Stacks;
        public int ReviveCost => _reviveService.CurrentCost;

        public bool CanSpin => State == GameState.Ready;
        public bool CanLeave => State == GameState.Ready && _zoneRules.IsRiskFree(CurrentZone);
        public bool CanRevive => State == GameState.Bombed && _reviveService.CanAfford;

        public WheelGameModel(
            IZoneRules zoneRules,
            IWheelProvider wheelProvider,
            ISpinResolver spinResolver,
            IRewardInventory inventory,
            IReviveService reviveService,
            IRewardClaimHandler claimHandler)
        {
            _zoneRules = zoneRules ?? throw new ArgumentNullException(nameof(zoneRules));
            _wheelProvider = wheelProvider ?? throw new ArgumentNullException(nameof(wheelProvider));
            _spinResolver = spinResolver ?? throw new ArgumentNullException(nameof(spinResolver));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _reviveService = reviveService ?? throw new ArgumentNullException(nameof(reviveService));
            _claimHandler = claimHandler ?? throw new ArgumentNullException(nameof(claimHandler));

            ResetRun();
        }

        public bool TryStartSpin()
        {
            if (!CanSpin)
            {
                return false;
            }

            int index = _spinResolver.ResolveSliceIndex(CurrentWheel.Slices);
            _pendingSpin = new SpinResult(index, CurrentWheel.Slices[index]);

            SetState(GameState.Spinning);
            SpinStarted?.Invoke(_pendingSpin);
            return true;
        }

        public bool TryCompleteSpin()
        {
            if (State != GameState.Spinning)
            {
                return false;
            }

            WheelSlice slice = _pendingSpin.Slice;
            if (slice.IsBomb)
            {
                SetState(GameState.Bombed);
                return true;
            }

            _inventory.Add(slice.RewardId, slice.Amount);
            RewardGained?.Invoke(new RewardStack(slice.RewardId, slice.Amount));

            EnterZone(CurrentZone + 1);
            SetState(GameState.Ready);
            return true;
        }

        public bool TryLeave()
        {
            if (!CanLeave)
            {
                return false;
            }

            _claimHandler.Claim(new List<RewardStack>(_inventory.Stacks));
            SetState(GameState.Collected);
            return true;
        }

        public bool TryRevive()
        {
            if (State != GameState.Bombed || !_reviveService.TryRevive())
            {
                return false;
            }

            SetState(GameState.Ready);
            return true;
        }

        public bool TryGiveUp()
        {
            if (State != GameState.Bombed)
            {
                return false;
            }

            _inventory.Clear();
            SetState(GameState.Lost);
            return true;
        }

        public bool TryRestart()
        {
            if (State != GameState.Lost && State != GameState.Collected)
            {
                return false;
            }

            ResetRun();
            return true;
        }

        private void ResetRun()
        {
            _inventory.Clear();
            _reviveService.Reset();
            EnterZone(ZoneRules.FirstZone);
            SetState(GameState.Ready);
        }

        private void EnterZone(int zone)
        {
            ZoneType zoneType = _zoneRules.GetZoneType(zone);
            WheelDefinition wheel = _wheelProvider.GetWheel(zone, zoneType)
                ?? throw new InvalidOperationException($"No wheel provided for zone {zone}.");

            if (zoneType != ZoneType.Normal && wheel.HasBomb)
            {
                throw new InvalidOperationException($"Zone {zone} is {zoneType} and must not contain a bomb.");
            }

            CurrentZone = zone;
            CurrentZoneType = zoneType;
            CurrentWheel = wheel;
            ZoneChanged?.Invoke(CurrentZone);
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(State);
        }
    }
}
