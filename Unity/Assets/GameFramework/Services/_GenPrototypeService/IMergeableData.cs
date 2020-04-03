public interface IMergeableData<T> {
    void MergeDataFrom(T incoming, bool onlyCopyPersistedData = false);
}
