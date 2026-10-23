using System.Globalization;
using System.Text;

namespace EntityRepository;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
        IRepository<Posting> postings = new GenericRepository<Posting>();
        postings.Add(new Posting { Id = 1, TrackingNumber = "UA000001", Weight = 2.5m,
            SenderAddress = "Київ, вул. Польова, 10", RecipientAddress = "Львів, вул. Зелена, 20", DeliveryStatus = "Прийнято" });
        postings.Add(new Posting { Id = 2, TrackingNumber = "UA000002", Weight = 4,
            SenderAddress = "Одеса, вул. Морська, 3", RecipientAddress = "Київ, вул. Польова, 10", DeliveryStatus = "У дорозі" });
        postings.Add(new Posting { Id = 3, TrackingNumber = "UA000003", Weight = 1,
            SenderAddress = "Львів, вул. Зелена, 20", RecipientAddress = "Одеса, вул. Морська, 3", DeliveryStatus = "Доставлено" });
        Console.WriteLine("Відправлення за Id=1:");
        Console.WriteLine(postings.GetById(1));
        Check(postings.GetById(1)?.TrackingNumber == "UA000001", "Пошук відправлення");
        Check(postings.Update(new Posting { Id = 1, TrackingNumber = "UA000001", Weight = 2.5m,
            SenderAddress = "Київ, вул. Польова, 10", RecipientAddress = "Львів, вул. Зелена, 20", DeliveryStatus = "Доставлено" }), "Оновлення відправлення");
        Check(postings.GetById(1)?.DeliveryStatus == "Доставлено", "Новий статус");
        Check(postings.Delete(2) && postings.GetById(2) is null, "Видалення відправлення");
        Console.WriteLine("Відправлення після оновлення Id=1 та видалення Id=2:");
        PrintAll(postings);

        IRepository<ContactPerson> contacts = new GenericRepository<ContactPerson>();
        contacts.Add(new ContactPerson { Id = 1, Name = "Олена Коваль", PhoneNumber = "+380000000001", Email = "olena@example.com" });
        contacts.Add(new ContactPerson { Id = 2, Name = "Андрій Мельник", PhoneNumber = "+380000000002", Email = "andrii@example.com" });
        contacts.Add(new ContactPerson { Id = 3, Name = "Ірина Бондар", PhoneNumber = "+380000000003", Email = "iryna@example.com" });
        Console.WriteLine("\nКонтакт за Id=1:");
        Console.WriteLine(contacts.GetById(1));
        Check(contacts.GetById(1)?.Name == "Олена Коваль", "Пошук контакту");
        Check(contacts.Update(new ContactPerson { Id = 1, Name = "Олена Коваль", PhoneNumber = "+380000000004", Email = "olena.new@example.com" }), "Оновлення контакту");
        Check(contacts.GetById(1)?.Email == "olena.new@example.com", "Нова адреса email");
        Check(contacts.Delete(3) && contacts.GetById(3) is null, "Видалення контакту");
        Console.WriteLine("Контакти після оновлення Id=1 та видалення Id=3:");
        PrintAll(contacts);

        Check(postings.GetAll().Count == 2 && contacts.GetAll().Count == 2, "Кількість сутностей");
        Check(postings.GetById(3) is not null && contacts.GetById(3) is null, "Ізоляція видалення");
        Check(postings.GetById(1)?.TrackingNumber == "UA000001", "Ізоляція оновлення");
        var anotherPostingRepository = new GenericRepository<Posting>();
        Check(ReferenceEquals(anotherPostingRepository.GetById(1), postings.GetById(1)), "Спільне статичне сховище одного типу");
        Check(anotherPostingRepository.GetAll().Count == 2, "Дані доступні іншому екземпляру");
        Check(postings.GetById(999) is null && !postings.Delete(999), "Відсутній Id");
        Check(!postings.Update(new Posting { Id = 999 }), "Оновлення відсутньої сутності");
        Expect<ArgumentException>(() => postings.Add(new Posting { Id = 1 }));
        Expect<ArgumentNullException>(() => postings.Add(null!));
        Expect<ArgumentNullException>(() => contacts.Update(null!));
        var snapshot = postings.GetAll();
        postings.Add(new Posting { Id = 4, TrackingNumber = "UA000004" });
        Check(snapshot.Count == 2 && anotherPostingRepository.GetById(4) is not null, "Копія переліку та спільне сховище");
        postings.Delete(4);
        Console.WriteLine("\nПеревірки пройдено: CRUD, спільний список одного типу, ізоляція різних типів і помилкові дані.");
    }

    private static void PrintAll<T>(IRepository<T> repository) where T : Model
    {
        foreach (T entity in repository.GetAll()) Console.WriteLine(entity);
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
