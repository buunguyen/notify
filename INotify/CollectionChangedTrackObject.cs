namespace INotify
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Wraps <see cref="INotifyCollectionChanged"/>. Responsibility:
    /// <list type="bullet">
    /// <item>    
    ///     <description>
    ///         Handle <see cref="INotifyCollectionChanged.CollectionChanged"/> and fire <see cref="Changed"/>.
    ///     </description>
    /// </item>
    /// <item>    
    ///     <description>
    ///         Create a <see cref="TrackedObject"/> for every existing or newly added element in the collection
    ///         and handle its <see cref="Changed"/> event. Keep track of these elements in 
    ///         <see cref="_registeredElements"/> so that they can be cleaned up later.
    ///     </description>
    /// </item>
    /// <item>    
    ///     <description>
    ///         If there are deleted elements, dispose the associated <see cref="TrackedObject"/>s 
    ///         in <see cref="_registeredElements"/>
    ///         so that they can be garbage collected.
    ///     </description>
    /// </item>
    /// </list>
    /// </summary>
    internal class CollectionChangedTrackObject : TrackedObject
    {
        private readonly Dictionary<object, TrackedObject> _registeredElements =
            new Dictionary<object, TrackedObject>(ObjectReferenceEqualityComparer<object>.Default);

        internal CollectionChangedTrackObject(object tracked)
            : base(tracked)
        {
        }

        protected override void RegisterTrackedObject()
        {
            foreach (var element in (IEnumerable)Tracked)
                RegisterElement(element);
            ((INotifyCollectionChanged)Tracked).CollectionChanged += OnCollectionChanged;
        }

        protected override void UnregisterTrackedObject()
        {
            RemoveAllElements();
            ((INotifyCollectionChanged)Tracked).CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
                RemoveAllElements();
            else
            {
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                        RemoveElement(oldItem);

                if (args.NewItems != null)
                    foreach (var newElement in args.NewItems)
                        RegisterElement(newElement);
            }
            OnChange();
        }

        private void RegisterElement(object element)
        {
            if (IsValidObjectType(element))
            {
                RemoveElement(element);
                var trackedObject = Create(element);
                _registeredElements.Add(element, trackedObject);
                trackedObject.Changed += OnChange;
            }
        }

        private void RemoveElement(object element)
        {
            if (_registeredElements.ContainsKey(element))
            {
                TrackedObject trackedObject = _registeredElements[element];
                _registeredElements.Remove(element);
                trackedObject.Dispose();
            }
        }

        private void RemoveAllElements()
        {
            foreach (var key in _registeredElements.Keys.ToArray())
                RemoveElement(key);
        }

        public class ObjectReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
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
