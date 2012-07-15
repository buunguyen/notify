namespace INotify.Test.Stubs
{
    using System.Collections.ObjectModel;

    [TrackClass]
    class Person : NotifyPropertyChangedObject
    {
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