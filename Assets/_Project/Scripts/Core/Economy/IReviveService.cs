namespace Vertigo.Wheel.Core.Economy
{
    public interface IReviveService
    {
        int CurrentCost { get; }
        bool CanAfford { get; }

        bool TryRevive();

        void Reset();
    }
}
