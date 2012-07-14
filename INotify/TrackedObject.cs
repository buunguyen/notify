namespace INotify
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// Represents a tracked object and wraps the original object. 
    /// Depending on the specific subclass, <see cref="TrackedObject"/> basically:
    /// <list type="bullet">
    /// <item>
    ///     <description>
    ///         Listens to change event of the wrapped <see cref="INotifyCollectionChanged"/> or <see cref="INotifyPropertyChanged"/>.
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         Stores its elements and associated <see cref="TrackedObject"/> (for <see cref="INotifyCollectionChanged"/>)
    ///         or its properties and associated <see cref="TrackedObject"/> (for <see cref="INotifyPropertyChanged"/>)
    ///         in <see cref="AssociatedPropertiesOrElements"/>.
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         Listens to change event of every <see cref="TrackedObject"/> stored in <see cref="AssociatedPropertiesOrElements"/>. 
    ///         Effectively, this enables changes to bubble up to the root object.
    ///     </description>
    /// </item>
    /// </list>
    /// </summary>
    internal abstract class TrackedObject : IDisposable
    {
        public event Changed Changed;
        protected readonly object Tracked;

        protected TrackedObject(object tracked)
        {
            Tracked = tracked;
        }

        internal static TrackedObject Create(object obj)
        {
            if (!IsValidObjectType(obj))
                throw new ArgumentException("null or invalid object type");
            var trackedObject = obj is INotifyCollectionChanged
                                    ? (TrackedObject)new CollectionTrackedObject(obj)
                                    : new NormalTrackedObject(obj);
            trackedObject.RegisterTrackedObject();
            return trackedObject;
        }

        protected static bool IsValidObjectType(object obj)
        {
            return obj is INotifyPropertyChanged || obj is INotifyCollectionChanged;
        }

        protected abstract void RegisterTrackedObject();

        protected abstract void UnregisterTrackedObject();

        protected void OnChange(Tracker tracker = null)
        {
            if (Changed != null) Changed(null);
        }

        public void Dispose()
        {
            Changed = null;
            UnregisterTrackedObject();
        }
    }
}
