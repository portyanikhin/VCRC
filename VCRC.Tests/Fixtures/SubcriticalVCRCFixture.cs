using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SubcriticalVCRCFixture<T> : VCRCFixture<T> where T : AbstractVCRC, IEntropyAnalysable
{
    public SubcriticalVCRCFixture() : base(FluidsList.R32)
    {
        Condenser = new Condenser(
            Refrigerant.Name, 45.DegreesCelsius(), TemperatureDelta.FromKelvins(3));
        Cycle = CreateVCRC(Condenser);
        EjectorFlows = CalculateEjectorFlowsIfNecessary();
        AnalysisResult = PerformEntropyAnalysis();
    }

    public override T Cycle { get; }
    public Condenser Condenser { get; }
}