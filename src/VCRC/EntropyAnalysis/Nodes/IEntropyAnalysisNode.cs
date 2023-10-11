namespace VCRC;

internal interface IEntropyAnalysisNode
{
    public SpecificEnergy CalculateEnergyLoss(Temperature hotSource);
}
