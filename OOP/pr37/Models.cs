namespace EntityRepository;

public class Model
{
    public int Id { get; set; }
}

public class Posting : Model
{
    public string TrackingNumber { get; set; } = "";
    public decimal Weight { get; set; }
    public string SenderAddress { get; set; } = "";
    public string RecipientAddress { get; set; } = "";
    public string DeliveryStatus { get; set; } = "";

    public override string ToString() =>
        $"Id={Id}; трек: {TrackingNumber}; вага: {Weight} кг; " +
        $"від: {SenderAddress}; до: {RecipientAddress}; статус: {DeliveryStatus}";
}

public class ContactPerson : Model
{
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";

    public override string ToString() =>
        $"Id={Id}; ім'я: {Name}; телефон: {PhoneNumber}; email: {Email}";
}
