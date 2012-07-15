namespace INotify.Test.Stubs
{
    using System;
    using System.ComponentModel;

    internal class NotifyPropertyChangedObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}
