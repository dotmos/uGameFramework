public interface IGeneratedDataObject<T> {
    void MergeDataFrom(T incoming, bool onlyCopyPersistedData = false);
}
