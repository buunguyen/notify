// ReSharper disable InconsistentNaming
namespace INotify.Test
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Stubs;

    /// <summary>
    /// This class contains test cases demonstrating features of the library.
    /// </summary>
    [TestClass] public class Features : BaseTest
    {
        #region General

        [TestMethod]
        public void Should_not_fire_when_not_tracking()
        {
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_throw_when_track_with_no_argument()
        {
            Tracker.Track();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_throw_when_track_null_object()
        {
            Tracker.Track(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_throw_when_track_invalid_object()
        {
            Tracker.Track("string");
        }

        #endregion

        #region INotifyPropertyChanged

        [TestMethod]
        public void Should_not_fire_when_start_tracking_an_object()
        {
            var person = new Person();
            Tracker.Track(person);
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_fire_when_property_is_changed()
        {
            var person = new Person();
            Tracker.Track(person);
            person.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_after_disposed()
        {
            var person = new Person();
            var tracker = new Tracker().Track(person);
            tracker.Dispose();
            person.Name += "(changed)";
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_for_replaced_property()
        {
            var spouse = new Person();
            var person = new Person { Spouse = spouse };
            Tracker.Track(person);
            
            person.Spouse = new Person();
            Assert.IsTrue(HasChange);

            person.Spouse.Name += "(changed)";
            Assert.IsTrue(HasChange);

            spouse.Name += "(changed)";
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_be_able_to_track_multiple_objects()
        {
            var person1 = new Person();
            var person2 = new Person();
            
            Tracker.Track(person1, person2);
            
            person1.Name += "(changed)";
            Assert.IsTrue(HasChange);

            person2.Name += "(changed)";
            Assert.IsTrue(HasChange);
            
            var person3 = new Person();
            Tracker.Track(person3);

            person3.Name += "(changed)";
            Assert.IsTrue(HasChange);

            person1.Name += "(changed)";
            Assert.IsTrue(HasChange);
            
            person2.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_for_untracked_property()
        {
            var person = new Person();
            Tracker.Track(person);
            person.Age += 1;
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_every_change()
        {
            var person = new Person();
            Tracker.Track(person);
            
            person.Name += "(changed)";
            Assert.IsTrue(HasChange);

            person.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_when_complex_property_is_changed()
        {
            var person = new Person { Spouse = new Person() };
            Tracker.Track(person);
            person.Spouse = new Person();
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_when_nested_property_is_changed()
        {
            var person = new Person { Spouse = new Person() };
            Tracker.Track(person);
            person.Spouse.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_when_nested_untracked_property_is_changed()
        {
            var person = new Person { Spouse = new Person() };
            Tracker.Track(person);
            person.Spouse.Age += 1;
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_object_without_track_attribute()
        {
            var car = new Car();
            Tracker.Track(car);
            
            car.Model += "(changed)";
            Assert.IsTrue(HasChange);

            car.Make += "(changed)";
            Assert.IsFalse(HasChange);

            car.Year += 1;
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_collection_property_changed()
        {
            var person = new Person {
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);
            person.Friends = new ObservableCollection<Person>();
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_nested_property_in_collection_property()
        {
            var person = new Person {
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);
            person.Friends[0].Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        private class Dummy : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
            #endregion

            private Person _p1;
            public Person P1
            {
                get { return _p1; }
                set
                {
                    if (_p1 != value)
                    {
                        _p1 = value;
                        NotifyPropertyChanged("P1");
                    }
                }
            }

            private Person _p2;
            public Person P2
            {
                get { return _p2; }
                set
                {
                    if (_p2 != value)
                    {
                        _p2 = value;
                        NotifyPropertyChanged("P2");
                    }
                }
            }
        }

        [TestMethod]
        public void Should_handle_multiple_properties_pointing_to_the_same_object()
        {
            var d = new Dummy();
            d.P1 = d.P2 = new Person();
            Tracker.Track(d);

            d.P1 = new Person();
            Assert.IsTrue(HasChange);

            d.P2.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }
        #endregion

        #region INotifyCollectionChanged

        [TestMethod]
        public void Should_fire_for_collection_addition()
        {
            var ppl = new ObservableCollection<Person>();
            Tracker.Track(ppl);
            ppl.Add(new Person());
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_collection_removal()
        {
            var ppl = new ObservableCollection<Person> { new Person() };
            Tracker.Track(ppl);
            ppl.RemoveAt(0);
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_collection_clear()
        {
            var ppl = new ObservableCollection<Person> { new Person() };
            Tracker.Track(ppl);
            ppl.Clear();
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_collection_element_assign()
        {
            var ppl = new ObservableCollection<Person> { new Person() };
            Tracker.Track(ppl);
            ppl[0] = new Person();
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_nested_property_in_collection_element()
        {
            var ppl = new ObservableCollection<Person> { new Person() };
            Tracker.Track(ppl);
            ppl[0].Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_for_replaced_element()
        {
            var person = new Person();
            var ppl = new ObservableCollection<Person> { person };
            Tracker.Track(ppl);
            ppl[0] = new Person();
            Assert.IsTrue(HasChange);

            person.Name += "(changed)";
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_for_removed_element()
        {
            var ppl = new ObservableCollection<Person> { new Person() };
            Tracker.Track(ppl);
            var p = ppl[0];
            ppl.Remove(p);
            Assert.IsTrue(HasChange);

            p.Name += "(changed)";
            Assert.IsFalse(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_new_element()
        {
            var ppl = new ObservableCollection<Person>();
            Tracker.Track(ppl);
            ppl.Add(new Person());
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_other_tracked_object_after_collection_clear()
        {
            var person = new Person
            {
                Spouse = new Person(),
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);

            person.Friends.Clear();
            Assert.IsTrue(HasChange);

            person.Spouse.Name += "(changed)";
            Assert.IsTrue(HasChange);
        }

        #endregion
    }
}
// ReSharper restore InconsistentNaming
