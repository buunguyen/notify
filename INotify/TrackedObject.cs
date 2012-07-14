namespace INotify
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a tracked object and wraps the original object to be tracked.
    /// 
    /// If the wrapped object is an <see cref="INotifyCollectionChanged"/>, do the followings:
    /// <list type="bullet">
    /// <item>    
    /// <description>
    /// Handle <see cref="INotifyCollectionChanged.CollectionChanged"/> and fire <see cref="Changed"/>.
    /// </description>
    /// </item>
    /// <item>    
    /// <description>
    /// Create a <see cref="TrackedObject"/> for every existing or newly added element in the collection
    /// and registers to its <see cref="Changed"/> event. Keep track of these elements in 
    /// <see cref="_associatedPropertiesOrElements"/> so that they can be cleaned up in the next step.
    /// </description>
    /// </item>
    /// <item>    
    /// <description>
    /// If there are deleted elements, dispose the associated <see cref="TrackedObject"/>s in <see cref="_associatedPropertiesOrElements"/>
    /// so that they can be garbage collected.
    /// </description>
    /// </item>
    /// </list>
    /// 
    /// If the wrapped object is an <see cref="INotifyPropertyChanged"/>, do the followings:
    /// <list type="bullet">
    /// <item>
    /// <desription>
    /// Handle <see cref="INotifyPropertyChanged.PropertyChanged"/> and fires <see cref="Changed"/>;
    /// </desription>
    /// </item>
    /// <item>
    /// <description>
    /// Create a <see cref="TrackedObject"/> for each eligible property (see <see cref="IsEligibleProperty"/>)
    /// and registers to its <see cref="Changed"/> event. Keep track of these properties in 
    /// <see cref="_associatedPropertiesOrElements"/> so that they can be cleaned up in the next step.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If an eligible property is assigned with a new object, create a <see cref="TrackedObject"/> for 
    /// the new value and registers to its <see cref="Changed"/> event; at the same time, dispose
    /// old value in <see cref="_associatedPropertiesOrElements"/> so that it can be garbaged collected.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    /// 
    /// In summary, each <see cref="TrackedObject"/>:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Listen to change event of the wrapped <see cref="INotifyCollectionChanged"/> or <see cref="INotifyPropertyChanged"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Store its elements and associated <see cref="TrackedObject"/> (for <see cref="INotifyCollectionChanged"/>)
    /// or its properties and associated <see cref="TrackedObject"/> (for <see cref="INotifyPropertyChanged"/>)
    /// in <see cref="_associatedPropertiesOrElements"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Listen to change event of every wrapped property/element (as <see cref="TrackedObject"/>) 
    /// in <see cref="_associatedPropertiesOrElements"/>. Effectively, this enables changes to bubble up 
    /// to the root object.
    /// </description>
    /// </item>
    /// </list>
    /// <remarks>
    /// Memory leak may happen if the tracked collection contains duplicated elements.
    /// But then, you don't store duplicated view models ever, do you?
    /// </remarks>
    internal class TrackedObject : IDisposable
    {
        public event Changed Changed;
        private readonly object _obj;
        private readonly Dictionary<object, TrackedObject> _associatedPropertiesOrElements =
            new Dictionary<object, TrackedObject>();

        public TrackedObject(object obj)
        {
            if (!IsValidObjectType(obj))
                throw new ArgumentException("null or invalid object type");
            
            _obj = obj;
            var collection = _obj as INotifyCollectionChanged;
            if (collection != null)
            {
                RegisterCollection(collection);
                return;
            }
            RegisterObject((INotifyPropertyChanged)_obj);
        }

        #region Helper
        private bool IsValidObjectType(object obj)
        {
            return obj is INotifyPropertyChanged || obj is INotifyCollectionChanged;
        }

        private TrackClassAttribute GetClassAttribute(object obj)
        {
            var attrs = obj.GetType().GetCustomAttributes(typeof(TrackClassAttribute), false);
            return (TrackClassAttribute)(attrs.Length == 0 ? null : attrs[0]);
        }

        private static BindingFlags GetBindingFlags(TrackClassAttribute attribute)
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
        #endregion

        #region Handles INotifyPropertyChanged
        private void RegisterObject(INotifyPropertyChanged obj)
        {
            var attribute = GetClassAttribute(obj);
            var bindingFlags = GetBindingFlags(attribute);
            var properties = obj.GetType().GetProperties(bindingFlags);

            foreach (var property in properties)
                if (IsEligibleProperty(property, attribute))
                    RegisterProperty(obj, property);
            
            obj.PropertyChanged += OnPropertyChanged;
        }

        private void UnRegisterObject(INotifyPropertyChanged obj)
        {
            obj.PropertyChanged -= OnPropertyChanged;
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

        private void RegisterProperty(object obj, PropertyInfo property)
        {
            var propValue = property.GetValue(obj, null);
            if (IsValidObjectType(propValue))
            {
                ClearReachableTrackedObjectIfExists(property.Name);
                var trackedObject = new TrackedObject(propValue);
                _associatedPropertiesOrElements.Add(property.Name, trackedObject);
                trackedObject.Changed += OnChange;
            }
        }
        #endregion

        #region Handles INotifyCollectionChanged
        private void RegisterCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += OnCollectionChanged;
            foreach (var element in (IEnumerable)collection)
                RegisterCollectionElement(element);
        }

        private void UnRegisterCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearAssociatedElements();
            }
            else
            {
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                        ClearReachableTrackedObjectIfExists(oldItem);

                if (args.NewItems != null)
                    foreach (var newElement in args.NewItems)
                        RegisterCollectionElement(newElement);
            }
            OnChange();
        }

        private void RegisterCollectionElement(object element)
        {
            if (IsValidObjectType(element))
            {
                ClearReachableTrackedObjectIfExists(element);
                var trackedObject = new TrackedObject(element);
                _associatedPropertiesOrElements.Add(element, trackedObject);
                trackedObject.Changed += OnChange;
            }
        }

        private void ClearAssociatedElements()
        {
            foreach (var key in _associatedPropertiesOrElements.Keys.ToArray())
                ClearReachableTrackedObjectIfExists(key);
        }
        #endregion

        private void ClearReachableTrackedObjectIfExists(object propertyNameOrCollectionElement)
        {
            if (_associatedPropertiesOrElements.ContainsKey(propertyNameOrCollectionElement))
            {
                TrackedObject trackedObject = _associatedPropertiesOrElements[propertyNameOrCollectionElement];
                _associatedPropertiesOrElements.Remove(propertyNameOrCollectionElement);
                trackedObject.Dispose();
            }
        }

        private void OnChange(Tracker tracker = null)
        {
            if (Changed != null) Changed(null);
        }

        public void Dispose()
        {
            Changed = null;
            foreach (var o in _associatedPropertiesOrElements.Values) 
                o.Dispose();

            var collection = _obj as INotifyCollectionChanged;
            if (collection != null)
            {
                UnRegisterCollection(collection);
                return;
            }
            UnRegisterObject((INotifyPropertyChanged)_obj);
        }
    }
}
