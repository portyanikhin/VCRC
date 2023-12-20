namespace VCRC;

internal interface IEntropyAnalysisNode
{
    SpecificEnergy CalculateEnergyLoss(Temperature hotSource);
}
