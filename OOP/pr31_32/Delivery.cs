namespace DeliveryCalculator;

public class VolumeWeight
{
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal VolumetricWeight => Length * Width * Height / 4000m;
    public decimal ChargeableWeight => Math.Max(Weight, VolumetricWeight);

    public void Validate()
    {
        if (Weight <= 0 || Length <= 0 || Width <= 0 || Height <= 0)
            throw new ArgumentException("Вага та розміри повинні бути більшими за нуль.");
    }
}

public enum DeliveryType { Department, Courier, ExpressCourier }

public interface IDeliveryCostStrategy
{
    DeliveryType DeliveryType { get; }
    decimal Calculate(VolumeWeight volumeWeight);
}

public class DepartmentDeliveryStrategy : IDeliveryCostStrategy
{
    public DeliveryType DeliveryType => DeliveryType.Department;
    public decimal Calculate(VolumeWeight volumeWeight) => 40m + 10m * volumeWeight.ChargeableWeight;
}

public class CourierDeliveryStrategy : IDeliveryCostStrategy
{
    public DeliveryType DeliveryType => DeliveryType.Courier;
    public decimal Calculate(VolumeWeight volumeWeight) => 80m + 15m * volumeWeight.ChargeableWeight;
}

public class ExpressCourierDeliveryStrategy : IDeliveryCostStrategy
{
    public DeliveryType DeliveryType => DeliveryType.ExpressCourier;
    public decimal Calculate(VolumeWeight volumeWeight) => 120m + 24m * volumeWeight.ChargeableWeight;
}

public interface IDeliveryCostService
{
    decimal Calculate(VolumeWeight volumeWeight, DeliveryType deliveryType);
}

// Початковий варіант: сервіс безпосередньо залежить від трьох конкретних класів.
public class EarlyBindingDeliveryCostService : IDeliveryCostService
{
    public decimal Calculate(VolumeWeight volumeWeight, DeliveryType deliveryType)
    {
        ArgumentNullException.ThrowIfNull(volumeWeight);
        volumeWeight.Validate();
        switch (deliveryType)
        {
            case DeliveryType.Department:
                return new DepartmentDeliveryStrategy().Calculate(volumeWeight);
            case DeliveryType.Courier:
                return new CourierDeliveryStrategy().Calculate(volumeWeight);
            case DeliveryType.ExpressCourier:
                return new ExpressCourierDeliveryStrategy().Calculate(volumeWeight);
            default:
                throw new ArgumentOutOfRangeException(nameof(deliveryType));
        }
    }
}

// Остаточний варіант: конкретні стратегії передаються ззовні.
public class DeliveryCostService : IDeliveryCostService
{
    private readonly Dictionary<DeliveryType, IDeliveryCostStrategy> strategies = new();

    public DeliveryCostService(IEnumerable<IDeliveryCostStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        foreach (IDeliveryCostStrategy strategy in strategies)
            this.strategies.Add(strategy.DeliveryType, strategy);
    }

    public decimal Calculate(VolumeWeight volumeWeight, DeliveryType deliveryType)
    {
        ArgumentNullException.ThrowIfNull(volumeWeight);
        volumeWeight.Validate();
        if (!strategies.TryGetValue(deliveryType, out IDeliveryCostStrategy? strategy))
            throw new ArgumentOutOfRangeException(nameof(deliveryType), "Спосіб доставки не підтримується.");
        return strategy.Calculate(volumeWeight); // Динамічний виклик через інтерфейс.
    }
}
