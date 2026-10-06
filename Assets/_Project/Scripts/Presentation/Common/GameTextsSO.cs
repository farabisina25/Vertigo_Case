using UnityEngine;

namespace Vertigo.Wheel.Presentation.Common
{
    [CreateAssetMenu(fileName = "game_texts", menuName = "Vertigo/Wheel/Game Texts")]
    public sealed class GameTextsSO : ScriptableObject
    {
        [SerializeField] private string _collectedTitle = "REWARDS COLLECTED";
        [Tooltip("{0} is replaced with the zone the player left at.")]
        [SerializeField] private string _collectedInfoFormat = "You walked away at zone {0}.";
        [SerializeField] private string _lostTitle = "ALL REWARDS LOST";
        [Tooltip("{0} is replaced with the zone the bomb exploded at.")]
        [SerializeField] private string _lostInfoFormat = "The bomb at zone {0} took everything.";

        public string CollectedTitle => _collectedTitle;
        public string LostTitle => _lostTitle;

        public string FormatCollectedInfo(int zone)
        {
            return string.Format(_collectedInfoFormat, zone);
        }

        public string FormatLostInfo(int zone)
        {
            return string.Format(_lostInfoFormat, zone);
        }
    }
}
