When building WPF and WinForms applications, I often run into the need to know when there is a change in an data-binding data source.

For example, imagine an application with a screen for users to edit some settings.
By default, the Save button is disabled because no change was made yet. Whenever a user makes a change to the screen, data binding triggers a change to the underlying data source and the Save button should be enabled.

For a simple screen bound to a simple underlying data source, it is easy to just listen to the ``PropertyChanged`` or ``CollectionChanged``
events and enable the Save button accordingly. For more complicated screens with multiple tabs and nested subviews bound to multiple data sources, 
this task becomes tedious and error-prone.  (Try writing code to track changes to a grand grand grand child of an element which is just added to a collection reachable via a property of a root object! There you go.)

This library is built specifically to address this problem. Create a ``Tracker`` instance, tell it to track one or more objects (which must implement either 
``INotifyCollectionChanged`` or ``INotifyPropertyChanged``), then wait for it to *notify* you when there is a change in the objects/collections, 
their children, grand children and so on... 

```csharp
var tracker = new Tracker().Track(root1, root2);
tracker.Changed += _ => EnableSave();

// ...sometimes later
tracker.Dispose(); // stop bothering me
```
By default, the library automatically tracks all public properties of ```INotifyPropertyChanged```. 
The library also allow you to explicitly control which objects and properties to track simply 
by applying some attributes. The unit test includes detailed usage of the library for 
both scenarios.  Go take a look and have fun being notified of changes.