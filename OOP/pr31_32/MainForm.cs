using System.Drawing.Drawing2D;

namespace DeliveryCalculator;

public class MainForm : Form
{
    private readonly IDeliveryCostService service;
    private readonly NumericUpDown weight = CreateInput(2.5m);
    private readonly NumericUpDown length = CreateInput(40m);
    private readonly NumericUpDown width = CreateInput(30m);
    private readonly NumericUpDown height = CreateInput(20m);
    private readonly RadioButton department = CreateOption("Доставка у відділення", 22);
    private readonly RadioButton courier = CreateOption("Кур'єром додому", 45);
    private readonly RadioButton express = CreateOption("Експрес кур'єром додому", 68);
    private readonly Label actualWeight = CreateValue();
    private readonly Label volumetricWeight = CreateValue();
    private readonly Label chargeableWeight = CreateValue();
    private readonly Label deliveryName = CreateValue();
    private readonly Label priceLabel = new()
    {
        Text = "— грн", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter,
        ForeColor = Color.FromArgb(0, 76, 177), Font = new Font("Segoe UI", 26, FontStyle.Bold)
    };
    private readonly ToolStripStatusLabel status = new("Готово до розрахунку");

    public MainForm(IDeliveryCostService service)
    {
        this.service = service;
        Text = "Розрахунок вартості доставки";
        Font = new Font("Segoe UI", 8.25f);
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(590, 400);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(247, 247, 247);

        var parameters = new GroupBox { Text = "Параметри посилки", Location = new Point(10, 12), Size = new Size(266, 182) };
        AddInput(parameters, "Вага, кг", weight, 24);
        AddInput(parameters, "Довжина, см", length, 58);
        AddInput(parameters, "Ширина, см", width, 92);
        AddInput(parameters, "Висота, см", height, 126);
        parameters.Controls.Add(new Label
        {
            Text = "Об'ємна вага = (Д × Ш × В) / 4000", AutoSize = true,
            Location = new Point(14, 159), ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8)
        });
        var method = new GroupBox { Text = "Спосіб доставки", Location = new Point(10, 206), Size = new Size(266, 102) };
        method.Controls.AddRange([department, courier, express]);
        department.Checked = true;

        var calculate = new Button
        {
            Text = "Розрахувати", Location = new Point(10, 320), Size = new Size(128, 35),
            BackColor = Color.FromArgb(25, 119, 226), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false, Cursor = Cursors.Hand
        };
        calculate.FlatAppearance.BorderColor = Color.FromArgb(15, 91, 181);
        calculate.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 101, 205);
        calculate.Click += Calculate;
        var clear = new Button { Text = "Очистити", Location = new Point(148, 320), Size = new Size(128, 35) };
        clear.Click += (_, _) => Clear();

        var result = new GroupBox { Text = "Результат", Location = new Point(288, 12), Size = new Size(286, 354) };
        AddResult(result, "Фактична вага:", actualWeight, 30, true);
        AddResult(result, "Об'ємна вага:", volumetricWeight, 79, true);
        AddResult(result, "Розрахункова вага:", chargeableWeight, 128, true);
        AddResult(result, "Спосіб доставки:", deliveryName, 177, false);
        deliveryName.Location = new Point(128, 187);
        deliveryName.Size = new Size(146, 42);
        deliveryName.TextAlign = ContentAlignment.TopLeft;
        deliveryName.Font = new Font("Segoe UI", 8);

        var pricePanel = new PricePanel { Location = new Point(12, 235), Size = new Size(262, 104), Padding = new Padding(1) };
        var title = new Label
        {
            Text = "Вартість доставки", Dock = DockStyle.Top, Height = 39,
            TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 76, 177)
        };
        priceLabel.BackColor = Color.Transparent;
        title.BackColor = Color.Transparent;
        pricePanel.Controls.Add(priceLabel);
        pricePanel.Controls.Add(title);
        result.Controls.Add(pricePanel);

        var statusBar = new StatusStrip { SizingGrip = false, Font = new Font("Segoe UI", 8), BackColor = Color.FromArgb(240, 240, 240) };
        statusBar.Items.Add(status);
        Controls.AddRange([parameters, method, calculate, clear, result, statusBar]);
        AcceptButton = calculate;
        ResetResults();
    }

    private static NumericUpDown CreateInput(decimal value) => new()
    {
        Minimum = 0.01m, Maximum = 10000m, DecimalPlaces = 2,
        Increment = 0.1m, Value = value, Size = new Size(136, 25),
        ThousandsSeparator = false
    };
    private static RadioButton CreateOption(string text, int top) => new()
    { Text = text, Location = new Point(14, top), AutoSize = true };
    private static Label CreateValue() => new()
    { Text = "—", TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
    private static void AddInput(GroupBox group, string text, NumericUpDown input, int top)
    {
        group.Controls.Add(new Label { Text = text, Location = new Point(14, top + 3), AutoSize = true });
        input.Location = new Point(116, top);
        group.Controls.Add(input);
    }
    private static void AddResult(GroupBox group, string text, Label value, int top, bool separator)
    {
        group.Controls.Add(new Label { Text = text, Location = new Point(18, top + 10), AutoSize = true });
        value.Location = new Point(174, top);
        value.Size = new Size(100, 33);
        group.Controls.Add(value);
        if (separator) group.Controls.Add(new Panel { Location = new Point(12, top + 40), Size = new Size(262, 1), BackColor = Color.FromArgb(224, 224, 224) });
    }
    private DeliveryType SelectedType => express.Checked ? DeliveryType.ExpressCourier : courier.Checked ? DeliveryType.Courier : DeliveryType.Department;
    private string SelectedName => express.Checked ? "Експрес кур'єром додому" : courier.Checked ? "Кур'єром додому" : "Доставка у відділення";
    private void Calculate(object? sender, EventArgs e)
    {
        var parcel = new VolumeWeight { Weight = weight.Value, Length = length.Value, Width = width.Value, Height = height.Value };
        decimal price = service.Calculate(parcel, SelectedType);
        actualWeight.Text = $"{parcel.Weight:0.0#} кг";
        volumetricWeight.Text = $"{parcel.VolumetricWeight:0.0###} кг";
        chargeableWeight.Text = $"{parcel.ChargeableWeight:0.0###} кг";
        deliveryName.Text = express.Checked ? SelectedName.Replace(" додому", Environment.NewLine + "додому") : SelectedName;
        priceLabel.Text = $"{price:0.##} грн";
        status.Text = "Розрахунок виконано";
    }
    private void ResetResults()
    {
        actualWeight.Text = volumetricWeight.Text = chargeableWeight.Text = deliveryName.Text = "—";
        priceLabel.Text = "— грн";
        status.Text = "Готово до розрахунку";
    }
    private void Clear()
    {
        weight.Value = length.Value = width.Value = height.Value = 0.01m;
        department.Checked = true;
        ResetResults();
        weight.Focus();
        weight.Select(0, weight.Text.Length);
    }

    private sealed class PricePanel : Panel
    {
        public PricePanel() { DoubleBuffered = true; }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var fill = new LinearGradientBrush(ClientRectangle, Color.FromArgb(234, 243, 255), Color.FromArgb(222, 235, 252), 90f);
            e.Graphics.FillRectangle(fill, ClientRectangle);
            using var border = new Pen(Color.FromArgb(133, 177, 236));
            e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }
    }
}




