namespace INotify
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// Represents a tracked object and wraps the original object.
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
                                    ? (TrackedObject)new CollectionChangedTrackObject(obj)
                                    : new PropertyChangedTrackedObject(obj);
            trackedObject.RegisterTrackedObject();
            return trackedObject;
        }

        protected static bool IsValidObjectType(object obj)
        {
            return obj is INotifyPropertyChanged || 
                   (obj is INotifyCollectionChanged && obj is IEnumerable);
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
