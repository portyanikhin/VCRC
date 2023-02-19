using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public abstract class VCRCFixture<T> where T : AbstractVCRC, IEntropyAnalysable
{
    protected VCRCFixture(FluidsList refrigerantName)
    {
        Refrigerant = new Refrigerant(refrigerantName);
        Evaporator = new Evaporator(Refrigerant.Name,
            5.DegreesCelsius(), TemperatureDelta.FromKelvins(8));
        Recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
        Compressor = new Compressor(80.Percent());
        Economizer = new Economizer(TemperatureDelta.FromKelvins(5), TemperatureDelta.FromKelvins(5));
        Ejector = new Ejector(90.Percent(), 90.Percent(), 80.Percent());
    }

    public abstract T Instance { get; }
    public EntropyAnalysisResult AnalysisResult { get; protected init; } = null!;
    public Refrigerant Refrigerant { get; }
    public Evaporator Evaporator { get; }
    public Recuperator Recuperator { get; }
    public Compressor Compressor { get; }
    public Economizer Economizer { get; }
    public Ejector Ejector { get; }
    public EjectorFlows EjectorFlows { get; protected init; } = null!;

    protected T CreateVCRC(IHeatReleaser heatReleaser) =>
        (typeof(T) switch
        {
            var type when type == typeof(SimpleVCRC) =>
                new SimpleVCRC(Evaporator, Compressor, heatReleaser) as T,
            var type when type == typeof(VCRCWithRecuperator) =>
                new VCRCWithRecuperator(Evaporator, Recuperator, Compressor, heatReleaser) as T,
            var type when type == typeof(VCRCWithIIC) =>
                new VCRCWithIIC(Evaporator, Compressor, heatReleaser) as T,
            var type when type == typeof(VCRCWithCIC) =>
                new VCRCWithCIC(Evaporator, Compressor, heatReleaser) as T,
            var type when type == typeof(VCRCWithPC) =>
                new VCRCWithPC(Evaporator, Compressor, heatReleaser) as T,
            var type when type == typeof(VCRCWithEconomizer) =>
                new VCRCWithEconomizer(Evaporator, Compressor, heatReleaser, Economizer) as T,
            var type when type == typeof(VCRCWithEconomizerAndPC) =>
                new VCRCWithEconomizerAndPC(Evaporator, Compressor, heatReleaser, Economizer) as T,
            var type when type == typeof(VCRCWithEconomizerAndTPI) =>
                new VCRCWithEconomizerAndTPI(Evaporator, Compressor, heatReleaser, Economizer) as T,
            var type when type == typeof(VCRCWithEjector) =>
                new VCRCWithEjector(Evaporator, Compressor, heatReleaser, Ejector) as T,
            var type when type == typeof(VCRCWithEjectorAndEconomizer) =>
                new VCRCWithEjectorAndEconomizer(Evaporator, Compressor, heatReleaser, Ejector, Economizer) as T,
            var type when type == typeof(VCRCWithEjectorEconomizerAndPC) =>
                new VCRCWithEjectorEconomizerAndPC(Evaporator, Compressor, heatReleaser, Ejector, Economizer) as T,
            var type when type == typeof(VCRCWithEjectorEconomizerAndTPI) =>
                new VCRCWithEjectorEconomizerAndTPI(Evaporator, Compressor, heatReleaser, Ejector, Economizer) as T,
            _ => new VCRCMitsubishiZubadan(Evaporator, Compressor, (Condenser) heatReleaser, Economizer) as T
        })!;

    protected EjectorFlows CalculateEjectorFlowsIfNecessary() =>
        typeof(T) switch
        {
            var type when type == typeof(VCRCWithEjector) =>
                Ejector.CalculateFlows(
                    (Instance as VCRCWithEjector)!.Point3,
                    (Instance as VCRCWithEjector)!.Point9),
            var type when type == typeof(VCRCWithEjectorAndEconomizer) =>
                Ejector.CalculateFlows(
                    (Instance as VCRCWithEjectorAndEconomizer)!.Point8,
                    (Instance as VCRCWithEjectorAndEconomizer)!.Point14),
            var type when type == typeof(VCRCWithEjectorEconomizerAndPC) =>
                Ejector.CalculateFlows(
                    (Instance as VCRCWithEjectorEconomizerAndPC)!.Point8,
                    (Instance as VCRCWithEjectorEconomizerAndPC)!.Point14),
            var type when type == typeof(VCRCWithEjectorEconomizerAndTPI) =>
                Ejector.CalculateFlows(
                    (Instance as VCRCWithEjectorEconomizerAndTPI)!.Point8,
                    (Instance as VCRCWithEjectorEconomizerAndTPI)!.Point14),
            _ => null!
        };

    protected EntropyAnalysisResult PerformEntropyAnalysis() =>
        Instance.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
}