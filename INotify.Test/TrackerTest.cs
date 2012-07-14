// ReSharper disable InconsistentNaming
namespace INotify.Test
{
    using System;
    using System.Collections.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Stubs;

    [TestClass]
    public class TrackerTest
    {
        private Tracker Tracker;

        private bool _hasChange;
        private bool HasChange
        {
            get
            {
                var tmp = _hasChange;
                if (_hasChange) _hasChange = false;
                return tmp;
            }
            set { _hasChange = value; }
        }
    
        [TestInitialize]
        public void Init()
        {
            Tracker = new Tracker();
            Tracker.Changed += _ => HasChange = true;
        }

        [TestCleanup]
        public void Cleaup()
        {
            Tracker.Dispose();
        }

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
        public void Should_not_fire_after_disposing()
        {
            var person = new Person();
            var tracker = new Tracker().Track(person);
            tracker.Dispose();
            person.Name += "(changed)";
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
        public void Should_fire_for_collection_addition()
        {
            var person = new Person { Friends = new ObservableCollection<Person>()};
            Tracker.Track(person);
            person.Friends.Add(new Person());
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_collection_removal()
        {
            var person = new Person {
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);
            person.Friends.RemoveAt(0);
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_collection_clear()
        {
            var person = new Person {
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);
            person.Friends.Clear();
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

        [TestMethod]
        public void Should_fire_for_collection_element_assign()
        {
            var person = new Person
            {
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);
            person.Friends[0] = new Person();
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_fire_for_nested_property_in_collection_element()
        {
            var person = new Person {
                Friends = new ObservableCollection<Person> { new Person() }
            };
            Tracker.Track(person);
            person.Friends[0].Name += "(changed)";
            Assert.IsTrue(HasChange);
        }
    }
}
// ReSharper restore InconsistentNaming
