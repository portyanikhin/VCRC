using System;
using FluentAssertions;

namespace VCRC.Tests.Components;

public class TestEquals<T> where T : IEquatable<T>
{
    public TestEquals(in T origin, in T same, in T other) =>
        (Origin, Same, Other) = (origin, same, other);

    private T Origin { get; }

    private T Same { get; }

    private T Other { get; }

    public void Start()
    {
        Origin.Should().Be(Origin);
        Origin.Should().BeSameAs(Origin);
        Origin.Should().Be(Same);
        Origin.Should().NotBeSameAs(Same);
        Origin.Should().NotBe(Other);
        Origin.Should().NotBeNull();
        Origin.Equals(new object()).Should().BeFalse();
    }
}