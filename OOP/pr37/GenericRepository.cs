namespace EntityRepository;

public interface IRepository<T> where T : Model
{
    void Add(T entity);
    T? GetById(int id);
    IReadOnlyList<T> GetAll();
    bool Update(T entity);
    bool Delete(int id);
}

public class GenericRepository<T> : IRepository<T> where T : Model
{
    // Один спільний список для всіх екземплярів із тим самим T.
    private static readonly List<T> entities = new();

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (GetById(entity.Id) is not null)
            throw new ArgumentException($"Сутність з Id={entity.Id} вже існує.", nameof(entity));
        entities.Add(entity);
    }

    public T? GetById(int id)
    {
        foreach (T entity in entities)
            if (entity.Id == id) return entity;
        return null;
    }

    // Копія списку захищає склад колекції; самі сутності лишаються посилальними об'єктами.
    public IReadOnlyList<T> GetAll() => entities.ToArray();

    public bool Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].Id != entity.Id) continue;
            entities[i] = entity;
            return true;
        }
        return false;
    }

    public bool Delete(int id)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].Id != id) continue;
            entities.RemoveAt(i);
            return true;
        }
        return false;
    }
}
