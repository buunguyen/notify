namespace INotify
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Creates an instance of this class to track changes to as many objects as needed.
    /// It automatically drills down and track changes to all objects reachable from the 
    /// explicitly tracked objects.
    /// Typically in an MVVM application, it's convenient to track just the root view model.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe. If you want to access instances of this class
    /// from multiple threads, you need to synchronize properly.
    /// </remarks>
    public class Tracker : IDisposable
    {
        private readonly List<TrackedObject> _objects = new List<TrackedObject>();
        public event Changed Changed;

        /// <summary>
        /// Tracks one or more objects and all of their properties, including collections, recursively.
        /// Can be invoked multiple times for different objects.
        /// </summary>
        /// <param name="objects">Objects to be tracked.
        /// These objects must not be <c>null</c> and must implements one of these 2 interfaces:
        /// <list type="bullet">
        ///     <item><description><see cref="INotifyPropertyChanged"/></description></item>
        ///     <item><description><see cref="INotifyCollectionChanged"/></description></item>
        /// </list>
        /// </param>
        /// <returns>This tracker object.</returns>
        public Tracker Track(params object[] objects) 
        {
            if (objects == null || objects.Length == 0)
                throw new ArgumentException("No object to track");

            var toBeTracked = objects.Select(o => new TrackedObject(o)).ToList();
            toBeTracked.ForEach(o => {
                o.Changed += _ => {
                    if (Changed != null) Changed(this);
                };
                _objects.Add(o);
            });
            return this;
        }

        /// <summary>
        /// Cleanup the tracker and all tracked objects.
        /// </summary>
        public void Dispose()
        {
            Changed = null;
            _objects.ForEach(o => o.Dispose());
            _objects.Clear();
        }
    }
}
