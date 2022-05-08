﻿using FluentValidation;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRatio;

namespace VCRC.Components.Validators;

public class CompressorValidator : AbstractValidator<Compressor>
{
    public CompressorValidator()
    {
        RuleFor(compressor => compressor.IsentropicEfficiency)
            .ExclusiveBetween(Ratio.Zero, 100.Percent())
            .WithMessage("Isentropic efficiency of the compressor should be in (0;100) %!");
    }
}