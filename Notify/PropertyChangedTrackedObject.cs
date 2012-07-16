namespace Notify
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Wraps <see cref="INotifyPropertyChanged"/>. Responsibility:
    /// <list type="bullet">
    /// <item>
    ///     <desription>
    ///         Handle <see cref="INotifyPropertyChanged.PropertyChanged"/> and fire <see cref="Changed"/>.
    ///     </desription>
    /// </item>
    /// <item>
    ///     <description>
    ///         Create a <see cref="TrackedObject"/> for each eligible property (see <see cref="IsEligibleProperty"/>)
    ///         and handle its <see cref="Changed"/> event. Keep track of these properties in 
    ///         <see cref="_registeredProperties"/> so that they can be cleaned up later.
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         If an eligible property is assigned with a new object, create a <see cref="TrackedObject"/> for 
    ///         the new value and listen to its <see cref="Changed"/> event; at the same time, dispose
    ///         old value in <see cref="_registeredProperties"/> so that it can be garbaged collected.
    ///     </description>
    /// </item>
    /// </list>
    /// </summary>
    internal class PropertyChangedTrackedObject : TrackedObject
    {
        private readonly Dictionary<object, TrackedObject> _registeredProperties =
            new Dictionary<object, TrackedObject>();

        internal PropertyChangedTrackedObject(object tracked)
            : base(tracked)
        {
        }

        internal override void RegisterTrackedObject()
        {
            var attribute = GetClassAttribute(Tracked);
            var bindingFlags = GetBindingFlags(attribute);
            var properties = Tracked.GetType().GetProperties(bindingFlags);

            foreach (var property in properties)
                if (IsEligibleProperty(property, attribute))
                    RegisterProperty(Tracked, property);
            
            ((INotifyPropertyChanged)Tracked).PropertyChanged += OnPropertyChanged;
        }

        internal override void UnregisterTrackedObject()
        {
            foreach (var key in _registeredProperties.Keys.ToArray())
                RemoveProperty(key);

            ((INotifyPropertyChanged)Tracked).PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var attribute = GetClassAttribute(sender);
            var bindingFlags = GetBindingFlags(attribute);
            var property = sender.GetType().GetProperty(args.PropertyName, bindingFlags);

            // The != null check is necessary because PropertyChanged might fire
            // for property which might not be included under bindingFlags.
            if (property != null && IsEligibleProperty(property, attribute))
            {
                RegisterProperty(sender, property);
                OnChange();
            }
        }

        private TrackClassAttribute GetClassAttribute(object obj)
        {
            var attrs = obj.GetType().GetCustomAttributes(typeof(TrackClassAttribute), false);
            return (TrackClassAttribute)(attrs.Length == 0 ? null : attrs[0]);
        }

        protected static BindingFlags GetBindingFlags(TrackClassAttribute attribute)
        {
            const BindingFlags instanceAnyVisibilityFlag = BindingFlags.Instance |
                                                           BindingFlags.NonPublic |
                                                           BindingFlags.Public;
            var bindingFlags = (attribute == null || attribute.IncludeBaseProperties)
                                   ? instanceAnyVisibilityFlag
                                   : instanceAnyVisibilityFlag | BindingFlags.DeclaredOnly;
            return bindingFlags;
        }

        private bool IsEligibleProperty(PropertyInfo property, TrackClassAttribute trackClassAttr)
        {
            var attrs = property.GetCustomAttributes(typeof(TrackMemberAttribute), false);
            var trackMemberAttr = (TrackMemberAttribute)(attrs.Length == 0 ? null : attrs[0]);

            if (trackMemberAttr != null)
                return !trackMemberAttr.IsExcluded;

            return ((trackClassAttr == null || !trackClassAttr.RequireExplicitMarking) &&
                   property.GetGetMethod(true).IsPublic);
        }

        private void RegisterProperty(object obj, PropertyInfo property)
        {
            // Not support indexer
            if (property.GetIndexParameters().Length > 0) return;

            var propValue = property.GetValue(obj, null);
            if (IsValidObjectType(propValue))
            {
                RemoveProperty(property.Name);
                var trackedObject = Create(propValue);
                _registeredProperties.Add(property.Name, trackedObject);
                trackedObject.Changed += OnChange;
            }
        }

        private void RemoveProperty(object propertyName)
        {
            if (_registeredProperties.ContainsKey(propertyName))
            {
                TrackedObject trackedObject = _registeredProperties[propertyName];
                _registeredProperties.Remove(propertyName);
                trackedObject.Dispose();
            }
        }
    }
}
