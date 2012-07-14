namespace INotify
{
    /// <summary>
    /// Delegate used to notify of a change in a tracked object.
    /// </summary>
    /// <param name="tracker">The sending tracker object.</param>
    public delegate void Changed(Tracker tracker);
}