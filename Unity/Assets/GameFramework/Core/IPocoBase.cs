/// <summary>
/// Interface for classes that should work with the eclipse plugin
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPocoBase<T> {
    void ShallowCopy(T from);
}