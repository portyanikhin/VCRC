using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class TranscriticalVCRCFixture<T> : VCRCFixture<T> where T : AbstractVCRC, IEntropyAnalysable
{
    public TranscriticalVCRCFixture() : base(FluidsList.R744)
    {
        GasCooler = new GasCooler(Refrigerant.Name, 40.DegreesCelsius());
        Cycle = CreateVCRC(GasCooler);
        EjectorFlows = CalculateEjectorFlowsIfNecessary();
        AnalysisResult = PerformEntropyAnalysis();
    }

    public override T Cycle { get; }
    public GasCooler GasCooler { get; }
}