namespace INotify.Future
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// @TODO Work in progress
    /// 
    /// Trying to simplify the design:
    /// 
    /// - Avoid creating a deep hierarchy of <see cref="TrackedObject"/>, instead
    /// one single <see cref="Tracker"/> will take care of everything.
    /// 
    /// - Make it possible to plug in custom change tracking strategies, e.g. POCO
    /// </summary>
    public class Tracker : IDisposable
    {
        public event Changed Changed;
        
        private readonly Dictionary<object, int> _root = 
            new Dictionary<object, int>(
                ObjectReferenceEqualityComparer<object>.Default);

        private readonly Dictionary<INotifyCollectionChanged, List<object>> _collections =
            new Dictionary<INotifyCollectionChanged, List<object>>(
                ObjectReferenceEqualityComparer<INotifyCollectionChanged>.Default);

        private readonly Dictionary<object, Dictionary<string, object>> _properties =
            new Dictionary<object, Dictionary<string, object>>(
                ObjectReferenceEqualityComparer<object>.Default);

        public Tracker Track(params object[] objects) 
        {
            if (objects == null || objects.Length == 0 || objects.Any(o => !IsValidType(o)))
                throw new ArgumentException("Must provide one or more objects of valid type");

            foreach (var o in objects)
                if (!_root.ContainsKey(o))
                    InternalTrack(o);

            return this;
        }

        private bool InternalTrack(object o)
        {
            if (!IsValidType(o)) return false;

            if (o is INotifyCollectionChanged)
            {
                var collection = (INotifyCollectionChanged) o;
                collection.CollectionChanged += CollectionChanged;
                foreach (var element in (IEnumerable)collection)
                {
                    if (InternalTrack(element))
                    {
                        if (!_collections.ContainsKey(collection))
                            _collections.Add(collection, new List<object>());
                        _collections[collection].Add(element);
                    }
                }
            }
            else
            {
                ((INotifyPropertyChanged)o).PropertyChanged += PropertyChanged;

                var attribute = GetClassAttribute(o);
                var bindingFlags = GetBindingFlags(attribute);
                var properties = o.GetType().GetProperties(bindingFlags);

                foreach (var property in properties)
                {
                    if (IsEligibleProperty(property, attribute))
                    {
                        var propValue = property.GetValue(o, null);
                        if (InternalTrack(propValue))
                        {
                            if (!_properties.ContainsKey(o))
                                _properties.Add(o, new Dictionary<string, object>());
                            _properties[o][property.Name] = propValue;
                        }
                    }
                }
            }
            return true;
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
        
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var attribute = GetClassAttribute(sender);
            var bindingFlags = GetBindingFlags(attribute);
            var property = sender.GetType().GetProperty(e.PropertyName, bindingFlags);

            // The != null check is necessary because PropertyChanged might fire
            // for property which might not be included under bindingFlags.
            if (property != null && IsEligibleProperty(property, attribute))
            {
                UntrackProperty(sender, property.Name);
                var propValue = property.GetValue(sender, null);
                if (InternalTrack(propValue))
                {
                    if (!_properties.ContainsKey(sender))
                        _properties.Add(sender, new Dictionary<string, object>());
                    _properties[sender][property.Name] = propValue;
                }
                OnChange();
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = (INotifyCollectionChanged) sender;
            if (e.Action == NotifyCollectionChangedAction.Reset)
                UntrackCollectionElements(collection);
            else
            {
                if (e.OldItems != null)
                    foreach (var oldItem in e.OldItems)
                        UntrackElement(collection, oldItem);

                if (e.NewItems != null)
                {
                    foreach (var newElement in e.NewItems)
                    {
                        if (InternalTrack(newElement))
                        {
                            if (!_collections.ContainsKey(collection))
                                _collections.Add(collection, new List<object>());
                            _collections[collection].Add(newElement);
                        }
                    }
                }
            }
            OnChange();
        }

        private void UntrackCollectionElements(INotifyCollectionChanged collection)
        {
            throw new NotImplementedException();
        }

        private void UntrackProperty(object o, string name)
        {
            throw new NotImplementedException();
        }

        private void UntrackElement(INotifyCollectionChanged collection, object element)
        {
            throw new NotImplementedException();
        }

        private static bool IsValidType(object o)
        {
            return o is INotifyPropertyChanged || 
                   (o is INotifyCollectionChanged && o is IEnumerable);
        }

        private void OnChange()
        {
            if (Changed != null) Changed(null);
        }

        public void Dispose()
        {
            Changed = null;
           // _objects.ForEach(o => o.Dispose());
           // _objects.Clear();
        }

        internal sealed class ObjectReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
        {
            public static new readonly IEqualityComparer<T> Default = 
                new ObjectReferenceEqualityComparer<T>();

            public override bool Equals(T x, T y)
            {
                return ReferenceEquals(x, y);
            }

            public override int GetHashCode(T obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
