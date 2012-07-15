namespace INotify.Test.Stubs
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    class IndexerDummy : NotifyPropertyChangedObject
    {
        public object this[int index]
        {
            get { return null; }
            set { NotifyPropertyChanged("Items[i]"); }
        }
    }

    class SameInstanceInPropertiesDummy : NotifyPropertyChangedObject
    {
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

    class DualDummy : ObservableCollection<object>, INotifyPropertyChanged
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
}
