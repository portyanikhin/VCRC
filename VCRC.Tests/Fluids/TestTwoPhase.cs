using NUnit.Framework;
using VCRC.Extensions;
using VCRC.Fluids;

namespace VCRC.Tests.Fluids
{
    public static class TestTwoPhase
    {
        [TestCase(TwoPhase.Bubble, ExpectedResult = 0)]
        [TestCase(TwoPhase.Dew, ExpectedResult = 100)]
        [TestCase(TwoPhase.Middle, ExpectedResult = 50)]
        public static double TestVaporQuality(TwoPhase member) => member.VaporQuality().Percent;
    }
}