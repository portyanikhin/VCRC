namespace VCRC.Tests.Components;

public static class TestCompressor
{
    [TestCase(0)]
    [TestCase(100)]
    public static void TestWrongIsentropicEfficiency(double isentropicEfficiency)
    {
        Action action = () => _ = new Compressor(isentropicEfficiency.Percent());
        action.Should().Throw<ValidationException>().WithMessage(
            "*Isentropic efficiency of the compressor should be in (0;100) %!*");
    }
}