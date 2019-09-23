namespace IdentityServer4
{
    /// <summary>
    /// Represents if the entity is capable of being active/passive
    /// </summary>
    public interface IPassiveable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        bool Active { get; set; }
    }
}