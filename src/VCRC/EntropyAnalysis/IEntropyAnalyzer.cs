namespace VCRC;

internal interface IEntropyAnalyzer
{
    public IEntropyAnalysisResult PerformAnalysis(
        Temperature indoor,
        Temperature outdoor
    );
}
