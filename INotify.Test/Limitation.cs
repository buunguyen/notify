// ReSharper disable InconsistentNaming
namespace INotify.Test
{
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

        [TestMethod]
        public void Should_fire_for_indexer()
        {
            var dummy = new IndexerDummy();
            Tracker.Track(dummy);
            dummy[0] = "see my change?";
            Tracker.Track(dummy);
            Assert.IsTrue(HasChange);
        }

        [TestMethod]
        public void Should_not_fire_if_class_is_excluded()
        {
            // well, this attribute doesn't exist yet, this test case is to remind me!
        }
    }
}
// ReSharper restore InconsistentNaming
