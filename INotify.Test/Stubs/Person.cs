namespace INotify.Test.Stubs
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    [TrackClass] class Person : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        private string _name = string.Empty;
        [TrackMember] public string Name
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

        private int _age;
        public int Age
        {
            get { return _age; }
            set
            {
                if (_age != value)
                {
                    _age = value;
                    NotifyPropertyChanged("Age");
                }
            }
        }

        private Person _spouse;
        [TrackMember] internal Person Spouse
        {
            get { return _spouse; }
            set
            {
                if (_spouse != value)
                {
                    _spouse = value;
                    NotifyPropertyChanged("Spouse");
                }
            }
        }

        private ObservableCollection<Person> _friends;
        [TrackMember] internal virtual ObservableCollection<Person> Friends
        {
            get
            {
                return _friends;
            }
            set
            {
                if (_friends != value)
                {
                    _friends = value;
                    NotifyPropertyChanged("Friends");
                }
            }
        }
    }
}