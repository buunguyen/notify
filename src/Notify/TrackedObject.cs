namespace Notify
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

        /// <summary>
        /// Factory method to create a correct subclass of <see cref="TrackedObject"/>.
        /// </summary>
        /// <param name="obj">The object to be tracked.</param>
        /// <returns>An instance of a subclass of <see cref="TrackedObject"/>.</returns>
        internal static TrackedObject Create(object obj)
        {
            if (!IsValidObjectType(obj))
                throw new ArgumentException("null or invalid object type");

            TrackedObject trackedObject;
            if (obj is INotifyCollectionChanged && obj is INotifyPropertyChanged)
                trackedObject = new DualTrackedObject(obj);
            else if (obj is INotifyCollectionChanged)
                trackedObject = new CollectionChangedTrackObject(obj);
            else 
                trackedObject = new PropertyChangedTrackedObject(obj);

            trackedObject.RegisterTrackedObject();
            return trackedObject;
        }

        protected static bool IsValidObjectType(object obj)
        {
            return obj is INotifyPropertyChanged || 
                   (obj is INotifyCollectionChanged && obj is IEnumerable);
        }

        internal abstract void RegisterTrackedObject();

        internal abstract void UnregisterTrackedObject();

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
