namespace INotify
{
    using System;

    /// <summary>
    /// This is an optional attribute.
    /// It can be used in conjuntion or independent with <see cref="TrackClassAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class TrackMemberAttribute : Attribute
    {
        /// <summary>
        /// Explicitly request <see cref="Tracker"/> to exclude this property from being tracked.
        /// Default is <c>false</c>.
        /// </summary>
        public bool IsExcluded { get; set; }
    }
}
