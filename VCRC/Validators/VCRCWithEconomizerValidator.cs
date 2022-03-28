﻿using FluentValidation;

namespace VCRC.Validators;

public class VCRCWithEconomizerValidator : AbstractValidator<VCRCWithEconomizer>
{
    public VCRCWithEconomizerValidator()
    {
        RuleFor(vcrc => vcrc.Economizer.Pressure)
            .GreaterThan(vcrc => vcrc.Evaporator.Pressure)
            .WithMessage("Intermediate pressure should be greater than evaporating pressure!");
        RuleFor(vcrc => vcrc.Economizer.Pressure)
            .LessThan(vcrc => vcrc.Condenser.Pressure)
            .WithMessage("Intermediate pressure should be less than condensing pressure!");
        RuleFor(vcrc => vcrc.Point9.Temperature)
            .LessThan(vcrc => vcrc.Point7.Temperature)
            .WithMessage("Wrong temperature difference at economizer 'hot' side!");
        RuleFor(vcrc => vcrc.Point10.Temperature)
            .LessThan(vcrc => vcrc.Point7.Temperature)
            .WithMessage("Too high temperature difference at economizer 'cold' side!");
    }
}