using System;
using NUnit.Framework;
using Vertigo.Wheel.Core.Zones;

namespace Vertigo.Wheel.Tests
{
    public sealed class ZoneRulesTests
    {
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new ZoneRules(safeZoneInterval: 5, superZoneInterval: 30);
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(29)]
        [TestCase(31)]
        public void GetZoneType_RegularZone_ReturnsNormal(int zone)
        {
            Assert.AreEqual(ZoneType.Normal, _rules.GetZoneType(zone));
        }

        [TestCase(5)]
        [TestCase(10)]
        [TestCase(25)]
        [TestCase(35)]
        public void GetZoneType_EveryFifthZone_ReturnsSafe(int zone)
        {
            Assert.AreEqual(ZoneType.Safe, _rules.GetZoneType(zone));
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(90)]
        public void GetZoneType_EveryThirtiethZone_ReturnsSuperOverSafe(int zone)
        {
            Assert.AreEqual(ZoneType.Super, _rules.GetZoneType(zone));
        }

        [TestCase(1, false)]
        [TestCase(5, true)]
        [TestCase(30, true)]
        public void IsRiskFree_MatchesZoneType(int zone, bool expected)
        {
            Assert.AreEqual(expected, _rules.IsRiskFree(zone));
        }

        [Test]
        public void GetZoneType_ZoneBelowFirst_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rules.GetZoneType(0));
        }

        [TestCase(0, 30)]
        [TestCase(5, 0)]
        public void Constructor_NonPositiveInterval_Throws(int safe, int super)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZoneRules(safe, super));
        }
    }
}
