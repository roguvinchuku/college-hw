namespace DeliveryCalculator;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        IDeliveryCostService service = args.Contains("--early")
            ? new EarlyBindingDeliveryCostService()
            : new DeliveryCostService(new IDeliveryCostStrategy[]
            {
                new DepartmentDeliveryStrategy(),
                new CourierDeliveryStrategy(),
                new ExpressCourierDeliveryStrategy()
            });
        Application.Run(new MainForm(service));
        return 0;
    }
}

