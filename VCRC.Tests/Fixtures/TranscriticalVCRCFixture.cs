// ReSharper disable All

using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public sealed class TranscriticalVCRCFixture<T> : VCRCFixture<T> where T : AbstractVCRC, IEntropyAnalysable
{
    public TranscriticalVCRCFixture() : base(FluidsList.R744)
    {
        GasCooler = new GasCooler(Refrigerant.Name, 40.DegreesCelsius());
        Instance = CreateVCRC(GasCooler);
        EjectorFlows = CalculateEjectorFlowsIfNecessary();
        AnalysisResult = PerformEntropyAnalysis();
    }

    public override T Instance { get; }
    public GasCooler GasCooler { get; }
}