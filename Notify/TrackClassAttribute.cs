namespace Notify
{
    using System;

    /// <summary>
    /// This is an optional attribute. If this attribute is not applied to a tracked object, 
    /// all public properties of that object are automatically tracked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TrackClassAttribute : Attribute
    {
        private bool _includeBaseProperties = true;
        
        /// <summary>
        /// If <c>true</c> (default), track up the inheritance hierarchy.
        /// If <c>false</c>, only declared properties are tracked.
        /// </summary>
        public bool IncludeBaseProperties
        {
            get { return _includeBaseProperties; }
            set { _includeBaseProperties = value; }
        }

        private bool _requireExplicitMarking = true;

        /// <summary>
        /// If <c>true</c> (default), only properties marked with <see cref="TrackMemberAttribute"/> are tracked.
        /// If <c>false</c>, public properties are tracked.
        /// </summary>
        public bool RequireExplicitMarking
        {
            get { return _requireExplicitMarking; }
            set { _requireExplicitMarking = value; }
        }
    }
}
