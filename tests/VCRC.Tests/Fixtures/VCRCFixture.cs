// ReSharper disable All

using UnitsNet.NumberExtensions.NumberToTemperature;

namespace VCRC.Tests;

public abstract class VCRCFixture<T>
    where T : class, IVCRC
{
    protected VCRCFixture(FluidsList refrigerantName)
    {
        Refrigerant = new Refrigerant(refrigerantName);
        Evaporator = new Evaporator(
            Refrigerant.Name,
            5.DegreesCelsius(),
            TemperatureDelta.FromKelvins(8)
        );
        Recuperator = new Recuperator(TemperatureDelta.FromKelvins(5));
        Compressor = new Compressor(80.Percent());
        Economizer = new Economizer(
            TemperatureDelta.FromKelvins(5),
            TemperatureDelta.FromKelvins(5)
        );
        Ejector = new Ejector(90.Percent(), 90.Percent(), 80.Percent());
    }

    public abstract T Instance { get; }
    public IEntropyAnalysisResult AnalysisResult { get; protected set; } = default!;
    public IRefrigerant Refrigerant { get; }
    public IEvaporator Evaporator { get; }
    public IAuxiliaryHeatExchanger Recuperator { get; }
    public ICompressor Compressor { get; }
    public IEconomizer Economizer { get; }
    public IEjector Ejector { get; }
    public IEjectorFlows EjectorFlows { get; protected set; } = null!;

    protected T CreateVCRC(IHeatReleaser heatReleaser) =>
        (
            typeof(T) switch
            {
                var type when type == typeof(ISimpleVCRC) => new SimpleVCRC(
                    Evaporator,
                    Compressor,
                    heatReleaser
                ) as T,
                var type when type == typeof(IVCRCWithRecuperator) => new VCRCWithRecuperator(
                    Evaporator,
                    Recuperator,
                    Compressor,
                    heatReleaser
                ) as T,
                var type when type == typeof(IVCRCWithIIC) => new VCRCWithIIC(
                    Evaporator,
                    Compressor,
                    heatReleaser
                ) as T,
                var type when type == typeof(IVCRCWithCIC) => new VCRCWithCIC(
                    Evaporator,
                    Compressor,
                    heatReleaser
                ) as T,
                var type when type == typeof(IVCRCWithPC) => new VCRCWithPC(
                    Evaporator,
                    Compressor,
                    heatReleaser
                ) as T,
                var type when type == typeof(IVCRCWithEconomizer) => new VCRCWithEconomizer(
                    Evaporator,
                    Compressor,
                    heatReleaser,
                    Economizer
                ) as T,
                var type when type == typeof(IVCRCWithEconomizerAndPC) =>
                    new VCRCWithEconomizerAndPC(Evaporator, Compressor, heatReleaser, Economizer)
                        as T,
                var type when type == typeof(IVCRCWithEconomizerAndTPI) =>
                    new VCRCWithEconomizerAndTPI(Evaporator, Compressor, heatReleaser, Economizer)
                        as T,
                var type when type == typeof(IVCRCWithEjector) => new VCRCWithEjector(
                    Evaporator,
                    Compressor,
                    heatReleaser,
                    Ejector
                ) as T,
                var type when type == typeof(IVCRCWithEjectorAndEconomizer) =>
                    new VCRCWithEjectorAndEconomizer(
                        Evaporator,
                        Compressor,
                        heatReleaser,
                        Ejector,
                        Economizer
                    ) as T,
                var type when type == typeof(IVCRCWithEjectorEconomizerAndPC) =>
                    new VCRCWithEjectorEconomizerAndPC(
                        Evaporator,
                        Compressor,
                        heatReleaser,
                        Ejector,
                        Economizer
                    ) as T,
                var type when type == typeof(IVCRCWithEjectorEconomizerAndTPI) =>
                    new VCRCWithEjectorEconomizerAndTPI(
                        Evaporator,
                        Compressor,
                        heatReleaser,
                        Ejector,
                        Economizer
                    ) as T,
                _ => new VCRCMitsubishiZubadan(
                    Evaporator,
                    Compressor,
                    (Condenser)heatReleaser,
                    Economizer
                ) as T,
            }
        )!;

    protected IEjectorFlows CalculateEjectorFlowsIfNecessary() =>
        typeof(T) switch
        {
            var type when type == typeof(IVCRCWithEjector) => Ejector.CalculateFlows(
                (Instance as IVCRCWithEjector)!.Point3,
                (Instance as IVCRCWithEjector)!.Point9
            ),
            var type when type == typeof(IVCRCWithEjectorAndEconomizer) => Ejector.CalculateFlows(
                (Instance as IVCRCWithEjectorAndEconomizer)!.Point8,
                (Instance as IVCRCWithEjectorAndEconomizer)!.Point14
            ),
            var type when type == typeof(IVCRCWithEjectorEconomizerAndPC) => Ejector.CalculateFlows(
                (Instance as IVCRCWithEjectorEconomizerAndPC)!.Point8,
                (Instance as IVCRCWithEjectorEconomizerAndPC)!.Point14
            ),
            var type when type == typeof(IVCRCWithEjectorEconomizerAndTPI) =>
                Ejector.CalculateFlows(
                    (Instance as IVCRCWithEjectorEconomizerAndTPI)!.Point8,
                    (Instance as IVCRCWithEjectorEconomizerAndTPI)!.Point14
                ),
            _ => default!,
        };

    protected IEntropyAnalysisResult PerformEntropyAnalysis() =>
        Instance.EntropyAnalysis(18.DegreesCelsius(), 35.DegreesCelsius());
}
