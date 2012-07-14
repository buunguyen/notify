INotify :: Change tracking library for .NET
=======
When building WPF/WinForms applications, I often run into the need of tracking when there is a change in an object hierarchy.

For example, imagine an application with a screen for users to configure some settings.
By default, the Save button is disabled. Whenever user makes a change to the screen, data binding triggers 
a change to the underlying data source. By now, the Save button should be enabled.

For a simple screen bound to a simple data source, it is very straightfoward to simply listen to the ``PropertyChanged`` or ``CollectionChanged``
events and enable the Save button accordingly. For more complicated screens with multiple tabs and nested subviews bound to multiple data sources, 
the task becomes tedious and error-prone.  (Try writing code to track changes to a grand-grand-grand-child of an element which is just added to a collection reachable via a property of a root object!)

This library is built specifically to address this problem. Just create a ``Tracker`` instance, tell it to track one or more objects (which must implement either 
``INotifyCollectionChanged`` or ``INotifyPropertyChanged``), then wait for it to *notify* you when there is a change in the objects/collections, 
their children, grand children and so on... 

```csharp
var tracker = new Tracker().Track(root1, root2);
tracker.Changed += _ => EnableSave();

// ...sometimes later
tracker.Dispose(); // stop bothering me
```
The unit test includes detailed usage of ``Tracker``.  Go take a look and have fun tracking changes.