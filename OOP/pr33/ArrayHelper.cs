namespace GenericMethods;

public static class ArrayHelper
{
    public static int IndexOf<T>(T[] array, T value)
    {
        ArgumentNullException.ThrowIfNull(array);
        for (int i = 0; i < array.Length; i++)
            if (EqualityComparer<T>.Default.Equals(array[i], value))
                return i;
        return -1;
    }

    public static void Reverse<T>(T[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        for (int left = 0, right = array.Length - 1; left < right; left++, right--)
        {
            T temp = array[left];
            array[left] = array[right];
            array[right] = temp;
        }
    }

    public static T Min<T>(T[] array) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(array);
        if (array.Length == 0)
            throw new ArgumentException("Не можна знайти мінімум порожнього масиву.", nameof(array));
        T min = array[0];
        for (int i = 1; i < array.Length; i++)
            if (Comparer<T>.Default.Compare(array[i], min) < 0)
                min = array[i];
        return min;
    }

    public static T Max<T>(T[] array) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(array);
        if (array.Length == 0)
            throw new ArgumentException("Не можна знайти максимум порожнього масиву.", nameof(array));
        T max = array[0];
        for (int i = 1; i < array.Length; i++)
            if (Comparer<T>.Default.Compare(array[i], max) > 0)
                max = array[i];
        return max;
    }
}
