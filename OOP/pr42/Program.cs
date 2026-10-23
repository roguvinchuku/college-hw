using System.Text;

namespace ExceptionHandling;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        NumberInput.ReadNumber(Console.In, Console.Out);
    }
}

