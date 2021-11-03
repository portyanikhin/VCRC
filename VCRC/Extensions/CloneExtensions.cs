using Force.DeepCloner;

namespace VCRC.Extensions
{
    public static class CloneExtensions
    {
        /// <summary>
        ///     Performs deep (full) copy of the <see cref="Refrigerant" /> instance.
        /// </summary>
        /// <param name="instance">The <see cref="Refrigerant" /> instance.</param>
        /// <returns>Deep copy of the <see cref="Refrigerant" /> instance.</returns>
        public static Refrigerant Clone(this Refrigerant instance) => instance.DeepClone();
    }
}