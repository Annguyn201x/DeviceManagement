using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace WaterMonitor;

/// <summary>Main window and the four local desktop screens.</summary>
public sealed class MainForm : Form
{
    private static readonly Color Navy = Color.FromArgb(23, 59, 97);
    private static readonly Color Blue = Color.FromArgb(23, 105, 170);
    private static readonly Color Muted = Color.FromArgb(113, 128, 150);
    private static readonly Color Border = Color.FromArgb(229, 235, 242);
    private static readonly Color Canvas = Color.FromArgb(244, 247, 251);
    private readonly BindingList<WaterDevice> _devices = new();
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(28), AutoScroll = true, BackColor = Canvas };
    private readonly Label _sectionLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Muted };

    public MainForm()
    {
        Text = "Water Monitor · Desktop";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 620);
        Size = new Size(1280, 820);
        BackColor = Canvas;
        Font = new Font("Segoe UI", 10);
        _devices.Add(new WaterDevice("ESP32_01", "AA:BB:CC:01:23:45", "Khu vực bể A", "Đang hoạt động"));
        _devices.Add(new WaterDevice("ESP32_02", "AA:BB:CC:01:23:46", "Khu vực bể B", "Đang hoạt động"));
        BuildShell();
        ShowOverview();
    }

    /// <summary>Builds the fixed navigation and shared header.</summary>
    private void BuildShell()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Navy, Padding = new Padding(14) };
        var brand = new Label { Text = "💧  WATER\n     MONITOR", Dock = DockStyle.Top, Height = 82, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 15), TextAlign = ContentAlignment.MiddleLeft };
        sidebar.Controls.Add(brand);
        var menu = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 270, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Navy };
        sidebar.Controls.Add(menu);
        AddNav("Tổng quan", ShowOverview);
        AddNav("Lịch sử", ShowHistory);
        AddNav("Thiết bị", ShowDevices);
        AddNav("Cài đặt", ShowSettings);
        void AddNav(string text, Action action)
        {
            var button = new Button { Text = "   " + text, Width = 188, Height = 48, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Navy, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10) };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 79, 115);
            button.Click += (_, _) => action();
            menu.Controls.Add(button);
        }

        var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White, Padding = new Padding(24, 0, 24, 0) };
        _sectionLabel.Dock = DockStyle.Left;
        _sectionLabel.TextAlign = ContentAlignment.MiddleLeft;
        header.Controls.Add(_sectionLabel);
        var local = new Label { Text = "●  Ứng dụng nội bộ", Dock = DockStyle.Right, Width = 170, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(24, 136, 102) };
        header.Controls.Add(local);
        var workspace = new Panel { Dock = DockStyle.Fill };
        workspace.Controls.Add(_content);
        workspace.Controls.Add(header);
        Controls.Add(workspace);
        Controls.Add(sidebar);
    }

    private void SetPage(string name)
    {
        _sectionLabel.Text = "Water Monitor   /   " + name;
        _content.Controls.Clear();
        _content.AutoScrollPosition = Point.Empty;
    }

    /// <summary>Static preview metrics; replace these with backend values later.</summary>
    private void ShowOverview()
    {
        SetPage("Tổng quan");
        var stack = PageStack("Tổng quan chất lượng nước", "Theo dõi chỉ số cảm biến trong mạng nội bộ");
        var metrics = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 150, WrapContents = false, BackColor = Canvas };
        metrics.Controls.Add(Metric("Độ pH", "7.2", "Ngưỡng tham khảo 6.5–8.5"));
        metrics.Controls.Add(Metric("Tổng chất rắn hòa tan", "342 ppm", "TDS · dữ liệu minh họa"));
        metrics.Controls.Add(Metric("Nhiệt độ nước", "26.4 °C", "Cập nhật vừa xong"));
        stack.Controls.Add(metrics);
        var chart = CardPanel();
        chart.Dock = DockStyle.Fill;
        chart.Padding = new Padding(20);
        chart.Controls.Add(new Label { Text = "Biến thiên pH", Dock = DockStyle.Top, Height = 35, Font = new Font("Segoe UI Semibold", 12), ForeColor = Navy });
        chart.Controls.Add(new Label { Text = "ESP32_01 · 24 giờ gần nhất · Dữ liệu minh họa", Dock = DockStyle.Top, Height = 26, ForeColor = Muted });
        chart.Controls.Add(new TrendPreview { Dock = DockStyle.Fill });
        stack.Controls.Add(chart);
        _content.Controls.Add(stack);
    }

    private static Control Metric(string title, string value, string detail)
    {
        var card = CardPanel();
        card.Size = new Size(280, 125);
        card.Margin = new Padding(0, 0, 14, 0);
        card.Padding = new Padding(16);
        card.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 26, ForeColor = Muted });
        card.Controls.Add(new Label { Text = detail, Dock = DockStyle.Bottom, Height = 24, ForeColor = Muted, Font = new Font("Segoe UI", 9) });
        card.Controls.Add(new Label { Text = value, Dock = DockStyle.Fill, ForeColor = Navy, Font = new Font("Segoe UI Semibold", 23), TextAlign = ContentAlignment.MiddleLeft });
        return card;
    }

    private void ShowHistory()
    {
        SetPage("Lịch sử");
        var stack = PageStack("Lịch sử đo", "Lọc và xem lại dữ liệu theo thiết bị, khoảng thời gian");
        var filter = CardPanel();
        filter.Dock = DockStyle.Top;
        filter.Height = 88;
        filter.Padding = new Padding(18);
        var device = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Left = 16, Top = 22 };
        device.Items.AddRange(_devices.Select(d => d.Name).ToArray());
        if (device.Items.Count > 0) device.SelectedIndex = 0;
        filter.Controls.Add(device);
        var dateRange = new Label { Text = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy") + " – " + DateTime.Today.ToString("dd/MM/yyyy"), Left = 255, Top = 27, AutoSize = true, ForeColor = Muted };
        filter.Controls.Add(dateRange);
        var apply = ActionButton("Lọc dữ liệu", 460, 17);
        apply.Click += (_, _) => MessageBox.Show("Bản xem thử chưa có dữ liệu lịch sử từ backend.", "Lịch sử", MessageBoxButtons.OK, MessageBoxIcon.Information);
        filter.Controls.Add(apply);
        stack.Controls.Add(filter);
        var note = CardPanel(); note.Dock = DockStyle.Top; note.Height = 120; note.Padding = new Padding(20); note.Margin = new Padding(0, 14, 0, 0);
        note.Controls.Add(new Label { Text = "Biểu đồ và bảng lịch sử sẽ hiển thị dữ liệu sau khi tích hợp backend.", Dock = DockStyle.Fill, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter });
        stack.Controls.Add(note);
        _content.Controls.Add(stack);
    }

    /// <summary>Device list with add and calibration actions.</summary>
    private void ShowDevices()
    {
        SetPage("Thiết bị");
        var stack = PageStack("Quản lý thiết bị", "Đăng ký thiết bị đo và cấu hình hiệu chỉnh cảm biến");
        var intro = CardPanel(); intro.Dock = DockStyle.Top; intro.Height = 72; intro.Padding = new Padding(16); intro.Margin = new Padding(0, 0, 0, 14);
        intro.Controls.Add(new Label { Text = "Dữ liệu đang lưu trong phiên xem thử. Backend sẽ gửi thông số đến thiết bị qua MQTT.", Dock = DockStyle.Fill, ForeColor = Navy, TextAlign = ContentAlignment.MiddleLeft });
        stack.Controls.Add(intro);
        var tableCard = CardPanel(); tableCard.Dock = DockStyle.Top; tableCard.Height = 380; tableCard.Padding = new Padding(12);
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, GridColor = Border, SelectionMode = DataGridViewSelectionMode.FullRowSelect, DataSource = _devices };
        grid.ColumnHeadersHeight = 42;
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WaterDevice.Name), HeaderText = "Tên thiết bị" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WaterDevice.DeviceId), HeaderText = "MAC / ID" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WaterDevice.Area), HeaderText = "Khu vực" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WaterDevice.Status), HeaderText = "Trạng thái" });
        tableCard.Controls.Add(grid);
        stack.Controls.Add(tableCard);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 12, 0, 0), BackColor = Canvas };
        var add = ActionButton("＋  Thêm thiết bị"); add.Click += (_, _) => AddDevice(); actions.Controls.Add(add);
        var calibrate = ActionButton("⚙  Hiệu chỉnh"); calibrate.Click += (_, _) => { if (grid.CurrentRow?.DataBoundItem is WaterDevice item) Calibrate(item); }; actions.Controls.Add(calibrate);
        stack.Controls.Add(actions);
        _content.Controls.Add(stack);
    }

    /// <summary>Validates required values and prevents duplicate device IDs.</summary>
    private void AddDevice()
    {
        using var dialog = new Form { Text = "Thêm thiết bị", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(440, 270), MaximizeBox = false, MinimizeBox = false };
        var name = Input(dialog, "Tên thiết bị *", 24, 24);
        var id = Input(dialog, "Mã thiết bị hoặc MAC *", 24, 82);
        var area = Input(dialog, "Khu vực", 24, 140);
        var save = ActionButton("Thêm thiết bị", 24, 202); save.Width = 150;
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(id.Text)) { MessageBox.Show("Vui lòng nhập tên và mã thiết bị.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_devices.Any(d => d.DeviceId.Equals(id.Text.Trim(), StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("Mã thiết bị đã tồn tại.", "Trùng mã", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            _devices.Add(new WaterDevice(name.Text.Trim(), id.Text.Trim(), string.IsNullOrWhiteSpace(area.Text) ? "Chưa phân khu" : area.Text.Trim(), "Chưa kết nối"));
            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        };
        dialog.Controls.Add(save);
        dialog.AcceptButton = save;
        dialog.ShowDialog(this);
    }

    /// <summary>Collects calibration values. MQTT publish is deliberately a placeholder.</summary>
    private void Calibrate(WaterDevice device)
    {
        using var dialog = new Form { Text = "Hiệu chỉnh cảm biến", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(480, 390), MaximizeBox = false, MinimizeBox = false };
        dialog.Controls.Add(new Label { Text = device.Name + "  ·  " + device.Area, Left = 24, Top = 16, Width = 420, Height = 28, ForeColor = Muted });
        var ph = Input(dialog, "Độ lệch pH", 24, 58, "0.00");
        var tds = Input(dialog, "Hệ số TDS", 24, 116, "1.00");
        var temp = Input(dialog, "Độ lệch nhiệt độ", 24, 174, "0.0");
        dialog.Controls.Add(new Label { Text = "Chưa gửi lệnh thật. Kết nối MQTT sẽ được thêm cùng backend.", Left = 24, Top = 228, Width = 425, Height = 44, ForeColor = Muted });
        var send = ActionButton("Gửi thông số", 24, 300); send.Width = 150;
        send.Click += (_, _) =>
        {
            var culture = CultureInfo.InvariantCulture;
            if (!double.TryParse(ph.Text.Replace(',', '.'), NumberStyles.Float, culture, out _) ||
                !double.TryParse(tds.Text.Replace(',', '.'), NumberStyles.Float, culture, out var factor) || factor <= 0 ||
                !double.TryParse(temp.Text.Replace(',', '.'), NumberStyles.Float, culture, out _))
            { MessageBox.Show("Nhập số hợp lệ; hệ số TDS phải lớn hơn 0.", "Thông số chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        };
        dialog.Controls.Add(send);
        dialog.AcceptButton = send;
        if (dialog.ShowDialog(this) == DialogResult.OK)
            MessageBox.Show("Đã kiểm tra thông số. Bản hiện tại chưa gửi MQTT.", "Bản xem thử", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSettings()
    {
        SetPage("Cài đặt");
        var stack = PageStack("Cài đặt", "Thiết lập ứng dụng desktop trong mạng nội bộ");
        var card = CardPanel(); card.Dock = DockStyle.Top; card.Height = 190; card.Padding = new Padding(20);
        card.Controls.Add(new Label { Text = "Kết nối MQTT", Dock = DockStyle.Top, Height = 34, Font = new Font("Segoe UI Semibold", 12), ForeColor = Navy });
        card.Controls.Add(new Label { Text = "Cấu hình broker sẽ được bổ sung khi backend được tích hợp. Ứng dụng hiện không kết nối internet hoặc broker.", Dock = DockStyle.Top, Height = 60, ForeColor = Muted });
        stack.Controls.Add(card);
        _content.Controls.Add(stack);
    }

    private static Panel PageStack(string title, string subtitle)
    {
        var stack = new Panel { Dock = DockStyle.Fill, BackColor = Canvas, AutoScroll = true };
        var heading = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Canvas };
        heading.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 40, Font = new Font("Segoe UI Semibold", 20), ForeColor = Navy });
        heading.Controls.Add(new Label { Text = subtitle, Dock = DockStyle.Top, Height = 30, ForeColor = Muted });
        stack.Controls.Add(heading);
        return stack;
    }

    private static Panel CardPanel() => new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0) };
    private static Button ActionButton(string text, int left = 0, int top = 0) => new() { Text = text, Left = left, Top = top, Height = 38, Width = 145, FlatStyle = FlatStyle.Flat, BackColor = Blue, ForeColor = Color.White, Cursor = Cursors.Hand, Font = new Font("Segoe UI Semibold", 9) };

    private static TextBox Input(Control parent, string label, int left, int top, string initial = "")
    {
        parent.Controls.Add(new Label { Text = label, Left = left, Top = top, Width = 400, Height = 20, ForeColor = Navy });
        var input = new TextBox { Left = left, Top = top + 23, Width = 400, Height = 28, Text = initial };
        parent.Controls.Add(input);
        return input;
    }
}

/// <summary>Simple trend illustration; real sensor history will replace this preview.</summary>
internal sealed class TrendPreview : Control
{
    public TrendPreview() { DoubleBuffered = true; BackColor = Color.White; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var grid = new Pen(Color.FromArgb(235, 239, 244));
        for (var y = 24; y < Height - 20; y += Math.Max(1, (Height - 44) / 4)) g.DrawLine(grid, 42, y, Width - 8, y);
        var points = new[] { new Point(44, Height * 2 / 3), new Point(Width / 5, Height / 2), new Point(Width * 2 / 5, Height * 3 / 5), new Point(Width * 3 / 5, Height / 3), new Point(Width * 4 / 5, Height / 2), new Point(Width - 14, Height / 3) };
        using var line = new Pen(Color.FromArgb(23, 105, 170), 3);
        if (points.Length > 1) g.DrawLines(line, points);
        using var dot = new SolidBrush(Color.FromArgb(23, 105, 170));
        foreach (var point in points) g.FillEllipse(dot, point.X - 4, point.Y - 4, 8, 8);
    }
}

/// <summary>Small local model used by the preview data grid.</summary>
public sealed class WaterDevice
{
    public WaterDevice(string name, string deviceId, string area, string status) { Name = name; DeviceId = deviceId; Area = area; Status = status; }
    public string Name { get; }
    public string DeviceId { get; }
    public string Area { get; }
    public string Status { get; }
}
