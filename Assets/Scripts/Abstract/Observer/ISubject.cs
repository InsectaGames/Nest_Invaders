public interface ISubject<T>
{
    // private List<IObserver<T>> _observers;

    public void AddObserver(IObserver<T> obs);
    public void RemoveObserver(IObserver<T> obs);
    public void UpdateObservers(T data);
}