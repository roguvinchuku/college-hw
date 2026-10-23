using System.Globalization;

namespace ExceptionHandling;

public static class NumberInput
{
    public static int? ReadNumber(TextReader reader, TextWriter writer)
    {
        while (true)
        {
            string? input = null;
            try
            {
                writer.Write("Введіть ціле число: ");
                input = reader.ReadLine();
                // Саме Parse: помилкові дані обробляються через винятки.
                int number = Int32.Parse(input!, NumberStyles.Integer, CultureInfo.InvariantCulture);
                writer.WriteLine($"Ви ввели: {number}");
                return number;
            }
            catch (FormatException)
            {
                writer.WriteLine("Введене значення не є цілим числом.");
            }
            catch (OverflowException) when (input?.TrimStart().StartsWith("-", StringComparison.Ordinal) == true)
            {
                writer.WriteLine($"Значення виходить за межі допустимого діапазону: менше за {Int32.MinValue}.");
            }
            catch (OverflowException)
            {
                writer.WriteLine($"Значення виходить за межі допустимого діапазону: більше за {Int32.MaxValue}.");
            }
            catch (ArgumentNullException) when (input is null)
            {
                // Кінець потоку: повторне читання вже не може дати нових даних.
                writer.WriteLine("Введення завершено. Коректне число не отримано.");
                return null;
            }
            catch (Exception ex)
            {
                writer.WriteLine($"Не вдалося отримати число. Тип помилки: {ex.GetType().Name}. Причина: {ex.Message}");
            }
            finally
            {
                writer.WriteLine("Спробу завершено.");
            }
            writer.WriteLine("Спробуйте ввести число знову.");
        }
    }
}
