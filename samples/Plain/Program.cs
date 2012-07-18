namespace Sample
{
    using System;
    using Notify;

    internal class Program
    {
        private static void Main()
        {
            var screen = new Screen();
            for (int i = 0; i < 10; i++) {
                screen.Add(new Shape {Location = new Point(i, i), Color = Color.Red});
            }

            using (var tracker = new Tracker().Track(screen)) {
                tracker.Changed += _ => Console.WriteLine("Changed!");
                screen.Add(new Shape()); // "Changed!" x 2 (1 for new element, 1 for Count property change)
                screen[0].Location = new Point(1, 1); // "Changed!"
                screen[1].Color = Color.Blue; // "Changed!"
                screen.RemoveAt(2); // "Changed!" x 2
            }
        }
    }
}