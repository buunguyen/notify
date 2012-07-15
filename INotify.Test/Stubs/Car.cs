namespace INotify.Test.Stubs
{
    class Car : NotifyPropertyChangedObject
    {
        private string _model = string.Empty;
        [TrackMember(IsExcluded = false)] internal string Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    _model = value;
                    NotifyPropertyChanged("Model");
                }
            }
        }
        private string _make = string.Empty;
        [TrackMember(IsExcluded = true)] public string Make
        {
            get { return _make; }
            set
            {
                if (_make != value)
                {
                    _make = value;
                    NotifyPropertyChanged("Make");
                }
            }
        }

        private int _year;
        public int Year
        {
            get { return _year; }
            set
            {
                if (_year != value)
                {
                    _year = value;
                    NotifyPropertyChanged("Year");
                }
            }
        }
    }
}
