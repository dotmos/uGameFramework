﻿using System;

namespace UniRx.InternalUtil
{
    // ImmutableList is sometimes useful, use for public.
    public class ImmutableList<T>
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        T[] data;

        public T[] Data
        {
            get { return data; }
        }

        ImmutableList()
        {
            data = new T[0];
        }

        public ImmutableList(T[] data)
        {
            this.data = data;
        }

        public ImmutableList<T> Add(T value)
        {
            T[] newData = new T[data.Length + 1];
            Array.Copy(data, newData, data.Length);
            newData[data.Length] = value;
            return new ImmutableList<T>(newData);
        }

        public ImmutableList<T> Remove(T value)
        {
            int i = IndexOf(value);
            if (i < 0) return this;

            int length = data.Length;
            if (length == 1) return Empty;

            T[] newData = new T[length - 1];

            Array.Copy(data, 0, newData, 0, i);
            Array.Copy(data, i + 1, newData, i, length - i - 1);

            return new ImmutableList<T>(newData);
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                // ImmutableList only use for IObserver(no worry for boxed)
                if (object.Equals(data[i], value)) return i;
            }
            return -1;
        }
    }
}