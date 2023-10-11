namespace VCRC;

internal interface IEvaporatorNode : IMainHeatExchangerNode
{
    public SpecificEnergy CalculateEnergyLoss(
        Temperature coldSource,
        Temperature hotSource
    );
}
