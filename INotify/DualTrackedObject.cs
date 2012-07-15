namespace INotify
{
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// A composite object wrapping both <see cref="CollectionChangedTrackObject"/>
    /// and <see cref="PropertyChangedTrackedObject"/> to support tracked object
    /// implementing both <see cref="INotifyPropertyChanged"/> and <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    internal class DualTrackedObject : TrackedObject
    {
        private TrackedObject _propertyChangedObject;
        private TrackedObject _collectionChangedObject;

        public DualTrackedObject(object tracked) : base(tracked) 
        {
        }

        internal override void RegisterTrackedObject()
        {
            _propertyChangedObject = new PropertyChangedTrackedObject(Tracked);
            _propertyChangedObject.RegisterTrackedObject();
            _propertyChangedObject.Changed += OnChange;

            _collectionChangedObject = new CollectionChangedTrackObject(Tracked);
            _collectionChangedObject.RegisterTrackedObject();
            _collectionChangedObject.Changed += OnChange;
        }

        internal override void UnregisterTrackedObject()
        {
            _propertyChangedObject.UnregisterTrackedObject();
            _collectionChangedObject.UnregisterTrackedObject();
        }
    }
}
