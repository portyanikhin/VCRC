namespace VCRC;

internal interface IEntropyAnalyzer
{
    IEntropyAnalysisResult PerformAnalysis(
        Temperature indoor,
        Temperature outdoor
    );
}
