using System;

public static class ThreadsafeRandom {
    private static Random _global = new Random();
    [ThreadStatic]
    private static Random _local;

    public static int Next() {
        Random inst = _local;
        if (inst == null) {
            int seed;
            lock (_global) seed = _global.Next();
            _local = inst = new Random(seed);
        }
        return inst.Next();
    }

    public static int Range(int minValue, int maxValue) {
        Random inst = _local;
        if (inst == null) {
            int seed;
            lock (_global) seed = _global.Next();
            _local = inst = new Random(seed);
        }
        return inst.Next(minValue, maxValue);
    }

    public static float Range(float minValue, float maxValue) {
        return Range((int)(minValue * 1000), (int)(maxValue * 1000)) * 0.001f;
    }
}