namespace VCRC;

public static class CloneExtensions
{
    /// <summary>
    ///     Performs deep (full) copy of the refrigerant instance.
    /// </summary>
    /// <param name="instance">The refrigerant instance.</param>
    /// <returns>Deep copy of the refrigerant instance.</returns>
    public static Refrigerant Clone(this Refrigerant instance) =>
        (Refrigerant) ((Fluid) instance).Clone();
}