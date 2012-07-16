// ReSharper restore InconsistentNaming

namespace INotify.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass] public class BaseTest
    {
        protected Tracker Tracker;

        private bool _hasChange;
        protected bool HasChange
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
        public void Cleanup()
        {
            Tracker.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming

