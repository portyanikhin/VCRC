// ReSharper disable All

using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public sealed class SubcriticalVCRCFixture<T> : VCRCFixture<T>
    where T : class, IVCRC
{
    public SubcriticalVCRCFixture()
        : base(FluidsList.R32)
    {
        Condenser = new Condenser(
            Refrigerant.Name,
            45.DegreesCelsius(),
            TemperatureDelta.FromKelvins(3)
        );
        Instance = CreateVCRC(Condenser);
        EjectorFlows = CalculateEjectorFlowsIfNecessary();
        AnalysisResult = PerformEntropyAnalysis();
    }

    public override T Instance { get; }
    public ICondenser Condenser { get; }
}
