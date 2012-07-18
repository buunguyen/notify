namespace Sample
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class Screen : ObservableCollection<Shape>
    {
    }

    public class Shape : INotifyPropertyChanged
    {
        private Point _location;
        public Point Location
        {
            get { return _location; }
            set 
            {
                if (_location != value)
                {
                    _location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }

        private Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }

    public class Color
    {
        public static readonly Color Red = new Color();
        public static readonly Color Green = new Color();
        public static readonly Color Blue = new Color();
    }

    public class Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        #region Equality
        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Y == Y && other.X == X;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Point)) return false;
            return Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (Y*397) ^ X;
            }
        }

        public static bool operator ==(Point left, Point right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
