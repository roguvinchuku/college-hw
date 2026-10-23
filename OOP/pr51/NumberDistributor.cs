namespace SharedResources;

public class PartitionResult
{
    public List<int> EvenList { get; }
    public List<int> OddList { get; }
    public PartitionResult(List<int> evenList, List<int> oddList)
    {
        EvenList = evenList;
        OddList = oddList;
    }
}

public static class NumberDistributor
{
    public static PartitionResult Distribute(int[] fullArray)
    {
        ArgumentNullException.ThrowIfNull(fullArray);
        List<int> evenList = new();
        List<int> oddList = new();
        int nextIndex = 0;
        object indexLock = new();
        object evenLock = new();
        object oddLock = new();

        // Усі три потоки виконують одну функцію та беруть наступний елемент.
        void ProcessNumbers()
        {
            while (true)
            {
                int value;
                lock (indexLock)
                {
                    // Перевірка межі й збільшення індексу — одна критична секція.
                    if (nextIndex >= fullArray.Length) return;
                    value = fullArray[nextIndex];
                    nextIndex++;
                }

                if (value % 2 == 0)
                {
                    lock (evenLock) evenList.Add(value);
                }
                else
                {
                    lock (oddLock) oddList.Add(value);
                }
            }
        }

        Thread[] threads = new Thread[3];
        for (int i = 0; i < threads.Length; i++)
            threads[i] = new Thread(ProcessNumbers) { Name = $"Worker {i + 1}" };
        foreach (Thread thread in threads) thread.Start();
        // Лише після завершення всіх працівників головний потік читає списки.
        foreach (Thread thread in threads) thread.Join();
        return new PartitionResult(evenList, oddList);
    }
}
