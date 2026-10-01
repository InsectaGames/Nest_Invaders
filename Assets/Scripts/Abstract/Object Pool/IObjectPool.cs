public interface IObjectPool<T>
{
    public T Get();
    public void Release(T obj);
    public void ReleaseAll();
}