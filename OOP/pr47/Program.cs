using System.Text;

namespace ParallelFactorials;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        int[] numbers = new int[20];
        Random random = new();
        for (int i = 0; i < numbers.Length; i++)
            numbers[i] = random.Next(1, 13); // Від 1 до 12 включно.

        Console.WriteLine("Масив: " + string.Join(", ", numbers));
        Thread[] threads = new Thread[numbers.Length];
        for (int i = 0; i < numbers.Length; i++)
        {
            // Окрема локальна змінна для кожного потоку, а не спільний індекс i.
            int num = numbers[i];
            threads[i] = new Thread(() =>
            {
                long numF = Factorial(num);
                Console.WriteLine($"Факторіал числа: {num}! = {numF}");
            });
        }

        foreach (Thread thread in threads)
            thread.Start();

        // Join очікує не лише обчислення, а й виведення результату потоком.
        foreach (Thread thread in threads)
            thread.Join();

        Console.WriteLine("Розрахунок закінчено.");
    }

    private static long Factorial(int number)
    {
        long result = 1;
        for (int factor = 2; factor <= number; factor++)
            result *= factor;
        return result;
    }
}
