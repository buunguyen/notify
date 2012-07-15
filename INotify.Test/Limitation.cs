// ReSharper disable InconsistentNaming
namespace INotify.Test
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Stubs;

    /// <summary>
    /// This contains test cases revealing known limitations of the library.
    /// These test cases all fail. Want to contribute? Simple, make them pass!
    /// </summary>
    [TestClass] public class Limitation : BaseTest
    {
        [Timeout(2000)]
        [TestMethod]
        public void Should_handle_circular_references()
        {
            var p = new Person { Spouse = new Person() };
            p.Spouse.Spouse = p;
            Tracker.Track(p);
        }

        private class Dummy : ObservableCollection<object>, INotifyPropertyChanged
        {
            #region INotifyPropertyChanged
            public new event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
            #endregion

            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        NotifyPropertyChanged("Name");
                    }
                }
            }
        }

        [TestMethod]
        public void Should_handle_object_implements_both_interfaces()
        {
            var d = new Dummy();
            Tracker.Track(d);
            d.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_handle_duplicate_collection_element()
        {
            var p1 = new Person();
            var p2 = p1;
            var ppl = new ObservableCollection<Person> {p1, p2};
            Tracker.Track(ppl);
            ppl.Remove(p1);
            Assert.IsTrue(HasChange);

            p2.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }
    }
}
// ReSharper restore InconsistentNaming
