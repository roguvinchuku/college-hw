using System.Text;

namespace SharedResources;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        int[] fullArray = new int[1000];
        Random random = new();
        for (int i = 0; i < fullArray.Length; i++)
            fullArray[i] = random.Next(1, 11); // Верхня межа 11 не входить у діапазон.

        PartitionResult result = NumberDistributor.Distribute(fullArray);
        List<int> evenList = result.EvenList;
        List<int> oddList = result.OddList;
        Console.WriteLine("Три потоки завершили розподіл масиву з 1000 елементів.");
        Console.WriteLine($"Парні елементи ({evenList.Count}):");
        Console.WriteLine(string.Join(", ", evenList));
        Console.WriteLine($"Непарні елементи ({oddList.Count}):");
        Console.WriteLine(string.Join(", ", oddList));
        Console.WriteLine($"Разом: {evenList.Count + oddList.Count} елементів.");
    }
}

