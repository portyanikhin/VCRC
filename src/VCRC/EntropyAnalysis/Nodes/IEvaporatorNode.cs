namespace VCRC;

internal interface IEvaporatorNode : IMainHeatExchangerNode
{
    SpecificEnergy CalculateEnergyLoss(
        Temperature coldSource,
        Temperature hotSource
    );
}
