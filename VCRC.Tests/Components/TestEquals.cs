using System;
using FluentAssertions;

namespace VCRC.Tests.Components;

public class TestEquals<T> where T : IEquatable<T>
{
    public TestEquals(in T originObj, in T sameObj, in T otherObj)
    {
        originObj.Should().Be(originObj);
        originObj.Should().BeSameAs(originObj);
        originObj.Should().Be(sameObj);
        originObj.Should().NotBeSameAs(sameObj);
        originObj.Should().NotBe(otherObj);
        originObj.Should().NotBeNull();
        originObj.Equals(new object()).Should().BeFalse();
    }
}