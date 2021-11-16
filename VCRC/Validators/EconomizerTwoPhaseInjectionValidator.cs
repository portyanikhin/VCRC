﻿using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToTemperatureDelta;
using VCRC.Components;

namespace VCRC.Validators
{
    public class EconomizerTwoPhaseInjectionValidator : AbstractValidator<EconomizerTwoPhaseInjection>
    {
        public EconomizerTwoPhaseInjectionValidator()
        {
            RuleFor(economizer => economizer.TemperatureDifference)
                .InclusiveBetween(TemperatureDelta.Zero, 50.Kelvins())
                .WithMessage("Temperature difference at the economizer 'cold' side should be in [0;50] K!");
        }
    }
}