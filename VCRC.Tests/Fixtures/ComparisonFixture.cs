﻿// ReSharper disable All

namespace VCRC.Tests;

public sealed class ComparisonFixture
{
    public double Tolerance { get; set; } = 1e-5;
    public ComparisonType Type { get; } = ComparisonType.Relative;
}