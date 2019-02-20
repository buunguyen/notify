[![Code Shelter](https://www.codeshelter.co/static/badges/badge-flat.svg)](https://www.codeshelter.co/)

## About Notify
When building WPF and WinForms applications, I often run into the need to know when there is a change in a data-binding data source (which implements
`INotifyCollectionChanged` and/or `INotifyPropertyChanged`).

For example, imagine an application with a screen for users to edit some settings.
By default, the Save button is disabled because no change was made yet. Whenever a user makes a change to the screen, data binding triggers a change to the underlying data source and the Save button should be enabled.

For a simple screen bound to a simple underlying data source, it is easy to just listen to the `PropertyChanged` or `CollectionChanged`
events and enable the Save button accordingly. For more complicated screens with multiple tabs and nested subviews bound to multiple data sources, 
this task becomes tedious and error-prone.  (Try writing code to track changes to a grand grand grand child of an element which is just added to a collection reachable via a property of a root object! There you go.)

This library is built to simplify change tracking for `INotifyCollectionChanged` and `INotifyPropertyChanged` data sources.

## Using Notify
Add reference to `Notify.dll`, e.g. via NuGet
```csharp
Install-Package Notify 
```
Next, create a `Tracker` instance to track your objects and handle its `Changed` event
```csharp
var tracker = new Tracker().Track(root1, root2);
tracker.Changed += _ => EnableSave();

// ...sometimes later
tracker.Dispose(); // stop bothering me
```
That's it! The unit test includes detailed usage of the library. Go take a look and have fun being notified of changes.


## Contact

* Email: [buunguyen@gmail.com](mailto:buunguyen@gmail.com)
* Blog: [www.buunguyen.net](http://www.buunguyen.net/blog)
* Twitter: [@buunguyen](https://twitter.com/buunguyen/)
