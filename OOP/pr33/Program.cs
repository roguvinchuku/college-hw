using System.Globalization;
using System.Text;

namespace GenericMethods;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
        Demonstrate(new[] { 5, 2, 9, 2, -1 }, 2, 100);
        Demonstrate(new[] { "груша", "яблуко", "абрикос", "груша" }, "груша", "слива");
        CheckEdgeCases();
        Console.WriteLine("Перевірки пройдено.");
    }

    private static void Demonstrate<T>(T[] array, T present, T missing) where T : IComparable<T>
    {
        Console.WriteLine($"Тип елементів: {typeof(T).Name}");
        Console.WriteLine($"Початковий масив: {string.Join(", ", array)}");
        int expectedIndex = -1;
        for (int i = 0; i < array.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(array[i], present)) { expectedIndex = i; break; }
        }
        int index = ArrayHelper.IndexOf(array, present);
        Check(index == expectedIndex, "Індекс першого входження");
        Check(ArrayHelper.IndexOf<T>(array, missing) == -1, "Відсутнє значення");
        Console.WriteLine($"IndexOf({present}): {index}; IndexOf({missing}): -1");
        T min = ArrayHelper.Min(array);
        T max = ArrayHelper.Max<T>(array);
        foreach (T item in array)
        {
            Check(Comparer<T>.Default.Compare(min, item) <= 0, "Мінімум");
            Check(Comparer<T>.Default.Compare(max, item) >= 0, "Максимум");
        }
        Console.WriteLine($"Min: {min}; Max: {max}");
        T[] original = (T[])array.Clone();
        ArrayHelper.Reverse(array);
        for (int i = 0; i < array.Length; i++)
            Check(EqualityComparer<T>.Default.Equals(array[i], original[original.Length - 1 - i]), "Розворот на місці");
        Console.WriteLine($"Після Reverse: {string.Join(", ", array)}\n");
    }

    private static void CheckEdgeCases()
    {
        int[] empty = [];
        Check(ArrayHelper.IndexOf(empty, 1) == -1, "Пошук у порожньому масиві");
        ArrayHelper.Reverse(empty);
        int[] single = [7];
        ArrayHelper.Reverse(single);
        Check(single[0] == 7 && ArrayHelper.Min(single) == 7 && ArrayHelper.Max(single) == 7, "Один елемент");
        int[] even = [1, 2, 3, 4];
        ArrayHelper.Reverse(even);
        Check(even[0] == 4 && even[1] == 3 && even[2] == 2 && even[3] == 1, "Парна довжина");
        string?[] nullable = ["рядок", null, null];
        Check(ArrayHelper.IndexOf(nullable, null) == 1, "Пошук null");
        ArrayHelper.Reverse(nullable);
        Check(nullable[0] is null && nullable[2] == "рядок", "Розворот із null");
        Expect<ArgumentException>(() => ArrayHelper.Min(empty));
        Expect<ArgumentException>(() => ArrayHelper.Max(empty));
        Expect<ArgumentNullException>(() => ArrayHelper.IndexOf<int>(null!, 1));
        Expect<ArgumentNullException>(() => ArrayHelper.Reverse<int>(null!));
        Expect<ArgumentNullException>(() => ArrayHelper.Min<int>(null!));
        Expect<ArgumentNullException>(() => ArrayHelper.Max<int>(null!));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception($"Помилка перевірки: {message}");
    }

    private static void Expect<TException>(Action action) where TException : Exception
    {
        try { action(); } catch (TException) { return; }
        throw new Exception($"Очікувався виняток {typeof(TException).Name}.");
    }
}
