using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace manegment_device;

/// <summary>Water Monitor dashboard integrated into the existing signed-in Management Device app.</summary>
public sealed class WaterMonitorDashboardForm : Form
{
    private static readonly Color Navy = Color.FromArgb(23, 59, 97);
    private static readonly Color Blue = Color.FromArgb(23, 105, 170);
    private static readonly Color Muted = Color.FromArgb(113, 128, 150);
    private static readonly Color Border = Color.FromArgb(229, 235, 242);
    private static readonly Color Canvas = Color.FromArgb(244, 247, 251);
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(26), AutoScroll = true, BackColor = Canvas };
    private readonly Label _sectionLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Muted };
    private readonly Label _connectionLabel = new() { AutoSize = true, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.DarkOrange };
    private readonly WaterMonitorSettings _settings;
    private Dashboard_query _repository;
    private WaterMqttService _mqtt;
    private readonly System.Windows.Forms.Timer _reconnectTimer = new() { Interval = 10000 };
    private readonly Dictionary<string, bool> _reportedOnline = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> _lastSeen = new(StringComparer.OrdinalIgnoreCase);
    private string _page = "Tổng quan";

    public WaterMonitorDashboardForm()
    {
        _settings = WaterMonitorSettings.Load();
        _repository = new Dashboard_query(_settings);
        _mqtt = CreateMqttService();
        Text = "Water Monitor · Management Device";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 660);
        Size = new Size(1320, 850);
        BackColor = Canvas;
        Font = new Font("Segoe UI", 10);
        BuildShell();
        Shown += async (_, _) =>
        {
            ShowOverview();
            await ConnectMqttQuietlyAsync();
            _reconnectTimer.Start();
        };
        _reconnectTimer.Tick += async (_, _) => await ConnectMqttQuietlyAsync();
        FormClosing += async (_, _) =>
        {
            _reconnectTimer.Stop();
            await _mqtt.DisposeAsync();
        };
    }

    private WaterMqttService CreateMqttService()
    {
        var service = new WaterMqttService(_settings);
        service.ConnectionChanged += (connected, message) => OnUi(() =>
        {
            _connectionLabel.Text = connected ? "● MQTT · " + message : "● MQTT · Chưa kết nối";
            _connectionLabel.ForeColor = connected ? Color.SeaGreen : Color.DarkOrange;
            if (_page == "Thiết bị") ShowDevices();
        });
        service.DeviceStatusChanged += (id, online) => OnUi(() =>
        {
            _reportedOnline[id] = online;
            if (_page == "Thiết bị") ShowDevices();
            if (_page == "Tổng quan") ShowOverview();
        });
        service.TelemetryReceived += telemetry => OnUi(() => HandleTelemetry(telemetry));
        return service;
    }

    private void OnUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) BeginInvoke(action); else action();
    }

    private async Task ConnectMqttQuietlyAsync()
    {
        if (_mqtt.IsConnected) return;
        try { await _mqtt.ConnectAsync(); }
        catch (Exception ex)
        {
            _connectionLabel.Text = "● MQTT · Chưa kết nối";
            _connectionLabel.ForeColor = Color.DarkOrange;
            if (_page == "Cài đặt") _sectionLabel.Text = "Water Monitor   /   Cài đặt · " + ex.Message;
        }
    }

    private void HandleTelemetry(WaterTelemetry telemetry)
    {
        _lastSeen[telemetry.DeviceId] = DateTime.Now;
        _reportedOnline[telemetry.DeviceId] = true;
        try
        {
            if (telemetry.Ph is not null && telemetry.Tds is not null && telemetry.Temperature is not null)
                _repository.InsertTelemetry(telemetry.DeviceId, telemetry.Ph.Value, telemetry.Tds.Value, telemetry.Temperature.Value);
        }
        catch (Exception ex)
        {
            _connectionLabel.Text = "● MQTT nhận được · lỗi lưu DB: " + ex.Message;
            _connectionLabel.ForeColor = Color.Firebrick;
        }
        if (_page == "Tổng quan") ShowOverview();
        else if (_page == "Thiết bị") ShowDevices();
        else if (_page == "Lịch sử") ShowHistory();
    }

    private void BuildShell()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 225, BackColor = Navy, Padding = new Padding(14) };
        sidebar.Controls.Add(new Label { Text = "💧  WATER\n     MONITOR", Dock = DockStyle.Top, Height = 86, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 15), TextAlign = ContentAlignment.MiddleLeft });
        var menu = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 300, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Navy };
        sidebar.Controls.Add(menu);
        AddNav("Tổng quan", ShowOverview);
        AddNav("Lịch sử", ShowHistory);
        AddNav("Thiết bị", ShowDevices);
        void AddNav(string text, Action action)
        {
            var button = new Button { Text = "   " + text, Width = 195, Height = 50, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Navy, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10) };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 79, 115);
            button.Click += (_, _) => action();
            menu.Controls.Add(button);
        }

        var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White, Padding = new Padding(22, 0, 20, 0) };
        _sectionLabel.Dock = DockStyle.Left;
        _sectionLabel.TextAlign = ContentAlignment.MiddleLeft;
        header.Controls.Add(_sectionLabel);
        _connectionLabel.Dock = DockStyle.Right;
        _connectionLabel.TextAlign = ContentAlignment.MiddleRight;
        header.Controls.Add(_connectionLabel);
        var user = new Label { Text = string.IsNullOrWhiteSpace(CurrentUser.Username) ? "Ứng dụng nội bộ" : CurrentUser.Username, Dock = DockStyle.Right, Width = 130, TextAlign = ContentAlignment.MiddleRight, ForeColor = Muted };
        header.Controls.Add(user);
        var workspace = new Panel { Dock = DockStyle.Fill };
        workspace.Controls.Add(_content);
        workspace.Controls.Add(header);
        Controls.Add(workspace);
        Controls.Add(sidebar);
    }

    private void SetPage(string name)
    {
        _page = name;
        _sectionLabel.Text = "Water Monitor   /   " + name;
        _content.Controls.Clear();
        _content.AutoScrollPosition = Point.Empty;
    }

    private void ShowOverview()
    {
        SetPage("Tổng quan");
        var stack = PageStack("Tổng quan chất lượng nước", "Số liệu thiết bị từ database · trạng thái kết nối MQTT");
        var devices = new List<Device>();
        var readings = new List<(Device Device, Information_device? Reading)>();
        var databaseUnavailable = false;
        Control? offlineNotice = null;
        try
        {
            devices = _repository.GetAllDevices();
        }
        catch (Exception ex)
        {
            databaseUnavailable = true;
            Console.WriteLine("Dashboard đang ở chế độ offline: " + ex.Message);
        }

        foreach (var device in devices)
        {
            try { readings.Add((device, LoadLatest(device))); }
            catch (Exception ex)
            {
                databaseUnavailable = true;
                Console.WriteLine("Không tải được số đo " + device.Ma_may + ": " + ex.Message);
                readings.Add((device, null));
            }
        }

        var latestReading = readings.Where(x => x.Reading is not null)
            .OrderByDescending(x => x.Reading!.ThoiGian).FirstOrDefault();
        var latest = latestReading.Reading;
        if (databaseUnavailable)
        {
            offlineNotice = OfflineNoticeCard("Đăng nhập offline · Database chưa kết nối. Bật MySQL để xem và quản lý dữ liệu thiết bị.");
            stack.Controls.Add(offlineNotice);
        }

        var metrics = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 140, WrapContents = false, BackColor = Canvas };
        metrics.Controls.Add(Metric("pH gần nhất", latest is null ? "—" : latest.Nong_do_pH.ToString("0.00"), "Tham khảo 6.5–8.5"));
        metrics.Controls.Add(Metric("TDS", latest is null ? "—" : $"{latest.Tds:0} ppm", "Tổng chất rắn hòa tan"));
        metrics.Controls.Add(Metric("Nhiệt độ", latest is null ? "—" : $"{latest.Nhiet_do:0.0} °C", latest?.ThoiGian.ToString("dd/MM HH:mm") ?? "Chưa có phép đo"));
        metrics.Controls.Add(Metric("Thiết bị", $"{devices.Count(d => d.Trang_thai)} / {devices.Count}", "Được bật trong quản lý"));
        stack.Controls.Add(metrics);

        var trendCard = CardPanel(); trendCard.Dock = DockStyle.Top; trendCard.Height = 270; trendCard.Padding = new Padding(16); trendCard.Margin = new Padding(0, 8, 0, 0);
        var selectedDevice = latestReading.Device;
        trendCard.Controls.Add(new Label { Text = latest is null ? "Xu hướng pH · chưa có dữ liệu" : $"Xu hướng pH · {selectedDevice!.Ma_may} · 24 giờ gần nhất", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI Semibold", 12), ForeColor = Navy });
        IReadOnlyList<Information_device> trend = Array.Empty<Information_device>();
        if (latest is not null)
        {
            try { trend = _repository.GetHistory(selectedDevice!.Ma_may, DateTime.Now.AddHours(-24), DateTime.Now.AddMinutes(1), 250); }
            catch (Exception ex) { databaseUnavailable = true; Console.WriteLine("Không tải được xu hướng: " + ex.Message); }
        }
        trendCard.Controls.Add(new ReadingTrendChart(trend) { Dock = DockStyle.Fill });
        stack.Controls.Add(trendCard);

        var recentCard = CardPanel(); recentCard.Dock = DockStyle.Top; recentCard.Height = 360; recentCard.Padding = new Padding(14); recentCard.Margin = new Padding(0, 8, 0, 0);
        recentCard.Controls.Add(new Label { Text = "Thiết bị và lần đo mới nhất", Dock = DockStyle.Top, Height = 32, Font = new Font("Segoe UI Semibold", 12), ForeColor = Navy });
        var grid = MakeGrid();
        grid.Columns.Add(TextColumn("Ma_may", "Mã thiết bị"));
        grid.Columns.Add(TextColumn("Ho_nuoi_name", "Hồ nuôi"));
        grid.Columns.Add(TextColumn("LiveStatus", "Trạng thái MQTT"));
        grid.Columns.Add(TextColumn("Current_IP", "IP"));
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Latest", HeaderText = "pH / TDS / °C", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        grid.DataSource = readings.Select(x => new { x.Device.Ma_may, x.Device.Ho_nuoi_name, LiveStatus = GetLiveStatus(x.Device), x.Device.Current_IP, Latest = FormatLatest(x.Reading) }).ToList();
        recentCard.Controls.Add(grid);
        stack.Controls.Add(recentCard);
        offlineNotice?.BringToFront();
        _content.Controls.Add(stack);
    }

    private static string FormatLatest(Information_device? reading) => reading is null
        ? "Chưa có dữ liệu"
        : $"{reading.Nong_do_pH:0.00} · {reading.Tds:0} ppm · {reading.Nhiet_do:0.0} °C  ({reading.ThoiGian:dd/MM HH:mm})";

    private Information_device? LoadLatest(Device device)
    {
        var reading = _repository.GetLatestReading(device.Ma_may);
        if (reading is not null && !_lastSeen.ContainsKey(device.Ma_may)) _lastSeen[device.Ma_may] = reading.ThoiGian;
        return reading;
    }

    private string GetLiveStatus(Device device)
    {
        if (!device.Trang_thai) return "Đã tắt";
        if (_reportedOnline.TryGetValue(device.Ma_may, out var reported) && !reported) return "Offline";
        if (_lastSeen.TryGetValue(device.Ma_may, out var seen))
            return DateTime.Now - seen < TimeSpan.FromMinutes(11) ? "Online · vừa gửi" : "Mất tín hiệu";
        if (_reportedOnline.TryGetValue(device.Ma_may, out reported) && reported) return "Online · MQTT";
        return "Chưa có trạng thái";
    }

    private void ShowHistory()
    {
        SetPage("Lịch sử");
        var stack = PageStack("Lịch sử đo", "Lọc số đo đã nhận từ ESP32 theo thiết bị và khoảng thời gian");
        var devices = new List<Device>();
        try { devices = _repository.GetAllDevices(); }
        catch (Exception ex) { stack.Controls.Add(ErrorCard("Không tải được thiết bị: " + ex.Message)); }
        var filter = CardPanel(); filter.Dock = DockStyle.Top; filter.Height = 100; filter.Padding = new Padding(16);
        filter.Controls.Add(new Label { Text = "Thiết bị", Left = 16, Top = 10, Width = 100, ForeColor = Navy });
        var deviceBox = new ComboBox { Left = 16, Top = 36, Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = devices, DisplayMember = nameof(Device.Ma_may) };
        filter.Controls.Add(deviceBox);
        filter.Controls.Add(new Label { Text = "Từ ngày", Left = 245, Top = 10, Width = 100, ForeColor = Navy });
        var from = new DateTimePicker { Left = 245, Top = 36, Width = 155, Value = DateTime.Today.AddDays(-7), Format = DateTimePickerFormat.Short };
        filter.Controls.Add(from);
        filter.Controls.Add(new Label { Text = "Đến ngày", Left = 410, Top = 10, Width = 100, ForeColor = Navy });
        var to = new DateTimePicker { Left = 410, Top = 36, Width = 155, Value = DateTime.Today, Format = DateTimePickerFormat.Short };
        filter.Controls.Add(to);
        var grid = MakeGrid();
        grid.AutoGenerateColumns = true;
        var query = ActionButton("Lọc dữ liệu", 585, 34); filter.Controls.Add(query);
        var resultCard = CardPanel(); resultCard.Dock = DockStyle.Fill; resultCard.Padding = new Padding(12); resultCard.Margin = new Padding(0, 14, 0, 0); resultCard.Controls.Add(grid);
        query.Click += (_, _) =>
        {
            if (deviceBox.SelectedItem is not Device selected) return;
            try
            {
                var end = to.Value.Date.AddDays(1);
                grid.DataSource = _repository.GetHistory(selected.Ma_may, from.Value.Date, end)
                    .Select(x => new { x.ThoiGian, pH = x.Nong_do_pH, TDS = x.Tds, NhietDo = x.Nhiet_do }).ToList();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Lỗi tải lịch sử", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        stack.Controls.Add(resultCard);
        stack.Controls.Add(filter);
        _content.Controls.Add(stack);
        if (deviceBox.Items.Count > 0) query.PerformClick();
    }

    private void ShowDevices()
    {
        SetPage("Thiết bị");
        var stack = PageStack("Quản lý thiết bị", "Thêm, sửa, xóa thiết bị và theo dõi trạng thái MQTT");
        var card = CardPanel(); card.Dock = DockStyle.Fill; card.Padding = new Padding(12);
        var grid = MakeGrid();
        grid.Columns.Add(TextColumn(nameof(Device.Ma_may), "Mã thiết bị"));
        grid.Columns.Add(TextColumn(nameof(Device.Ho_nuoi_name), "Hồ nuôi"));
        grid.Columns.Add(TextColumn(nameof(Device.Current_IP), "IP"));
        grid.Columns.Add(TextColumn(nameof(Device.LiveStatus), "Trạng thái"));
        grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(Device.Trang_thai), HeaderText = "Được quản lý", Width = 105 });
        card.Controls.Add(grid);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 10, 0, 0), BackColor = Canvas, WrapContents = false };
        var devices = new List<Device>();
        var add = ActionButton("＋  Thêm"); add.Click += (_, _) => AddDevice(); actions.Controls.Add(add);
        var edit = ActionButton("Sửa"); edit.Click += (_, _) => EditDevice(Selected()); actions.Controls.Add(edit);
        var delete = ActionButton("Xóa"); delete.Click += (_, _) => DeleteDevice(Selected()); actions.Controls.Add(delete);
        var toggle = ActionButton("Bật / tắt"); toggle.Click += (_, _) => ToggleDevice(Selected()); actions.Controls.Add(toggle);
        var calibrate = ActionButton("Hiệu chỉnh"); calibrate.Click += async (_, _) => await CalibrateAsync(Selected()); actions.Controls.Add(calibrate);
        var refresh = ActionButton("Tải lại"); refresh.Click += (_, _) => ShowDevices(); actions.Controls.Add(refresh);
        var search = new TextBox { Width = 190, Height = 34, Margin = new Padding(6, 4, 0, 0), PlaceholderText = "Tìm mã máy hoặc IP" };
        search.TextChanged += (_, _) => BindDevices(); actions.Controls.Add(search);
        stack.Controls.Add(card); stack.Controls.Add(actions);
        _content.Controls.Add(stack);
        try
        {
            devices = _repository.GetAllDevices().Select(d =>
            {
                LoadLatest(d);
                d.LiveStatus = GetLiveStatus(d);
                return d;
            }).ToList();
            BindDevices();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Không tải được danh sách thiết bị: " + ex.Message);
            add.Enabled = edit.Enabled = delete.Enabled = toggle.Enabled = false;
            stack.Controls.Add(OfflineNoticeCard("Database chưa kết nối. Các thao tác thiết bị sẽ khả dụng khi MySQL được bật."));
        }
        Device? Selected() => grid.CurrentRow?.DataBoundItem as Device;
        void BindDevices()
        {
            var keyword = search.Text.Trim();
            grid.DataSource = devices.Where(d => string.IsNullOrEmpty(keyword) ||
                d.Ma_may.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                d.Current_IP.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }

    private void AddDevice()
    {
        try
        {
            var lakes = _repository.GetLakes();
            using var dialog = new DeviceEditorDialog(lakes, null);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            bool ok = string.IsNullOrWhiteSpace(dialog.NewLakeName)
                ? _repository.AddDevice(dialog.DeviceId, dialog.LakeId, dialog.IpAddress)
                : _repository.ADD_lake_device(dialog.NewLakeName, dialog.DeviceId, dialog.IpAddress);
            if (!ok) throw new InvalidOperationException("Không thêm được thiết bị vào database.");
            ShowDevices();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Không thể thêm thiết bị", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void EditDevice(Device? device)
    {
        if (device is null) { MessageBox.Show(this, "Chọn một thiết bị trước."); return; }
        try
        {
            using var dialog = new DeviceEditorDialog(_repository.GetLakes(), device);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (!_repository.UpdateDevice(device.Ma_may, dialog.LakeId, dialog.IpAddress, dialog.IsEnabled))
                throw new InvalidOperationException("Không cập nhật được thiết bị.");
            ShowDevices();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Không thể sửa thiết bị", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ToggleDevice(Device? device)
    {
        if (device is null) { MessageBox.Show(this, "Chọn một thiết bị trước."); return; }
        try
        {
            if (!_repository.UpdateDevice(device.Ma_may, device.Ho_nuoi_id, device.Current_IP, !device.Trang_thai))
                throw new InvalidOperationException("Không đổi được trạng thái quản lý.");
            ShowDevices();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Không thể cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DeleteDevice(Device? device)
    {
        if (device is null) { MessageBox.Show(this, "Chọn một thiết bị trước."); return; }
        if (MessageBox.Show(this, $"Xóa {device.Ma_may} và toàn bộ lịch sử đo liên quan?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            if (!_repository.delete_device(device.Ma_may)) throw new InvalidOperationException("Không xóa được thiết bị.");
            _lastSeen.Remove(device.Ma_may); _reportedOnline.Remove(device.Ma_may);
            ShowDevices();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Không thể xóa thiết bị", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task CalibrateAsync(Device? device)
    {
        if (device is null) { MessageBox.Show(this, "Chọn một thiết bị trước."); return; }
        using var dialog = new CalibrationDialog(device.Ma_may);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            await ConnectMqttQuietlyAsync();
            await _mqtt.PublishCalibrationAsync(device.Ma_may, dialog.PhOffset, dialog.TdsFactor, dialog.TempOffset);
            MessageBox.Show(this, "Đã gửi lệnh hiệu chuẩn qua MQTT. Thiết bị sẽ lưu cấu hình vào NVS khi nhận được.", "Đã gửi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Không gửi được lệnh MQTT", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static Panel PageStack(string title, string subtitle)
    {
        var stack = new Panel { Dock = DockStyle.Fill, BackColor = Canvas, AutoScroll = true };
        var heading = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Canvas, Padding = new Padding(0, 4, 0, 4) };
        // Add the subtitle first so the title remains the first docked row and has
        // enough vertical room for its 20 pt font at higher display scaling.
        heading.Controls.Add(new Label { Text = subtitle, Dock = DockStyle.Top, Height = 28, ForeColor = Muted, TextAlign = ContentAlignment.MiddleLeft });
        heading.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 44, Font = new Font("Segoe UI Semibold", 20), ForeColor = Navy, TextAlign = ContentAlignment.MiddleLeft });
        stack.Controls.Add(heading);
        return stack;
    }

    private static Control Metric(string title, string value, string detail)
    {
        var card = CardPanel(); card.Size = new Size(220, 120); card.Margin = new Padding(0, 0, 12, 0); card.Padding = new Padding(14);
        card.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 24, ForeColor = Muted });
        card.Controls.Add(new Label { Text = detail, Dock = DockStyle.Bottom, Height = 24, ForeColor = Muted, Font = new Font("Segoe UI", 8.5f) });
        card.Controls.Add(new Label { Text = value, Dock = DockStyle.Fill, ForeColor = Navy, Font = new Font("Segoe UI Semibold", 22), TextAlign = ContentAlignment.MiddleLeft });
        return card;
    }

    private static DataGridView MakeGrid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
        GridColor = Border, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        ColumnHeadersHeight = 40, EnableHeadersVisualStyles = false
    };

    private static DataGridViewTextBoxColumn TextColumn(string property, string title) =>
        new() { DataPropertyName = property, HeaderText = title, SortMode = DataGridViewColumnSortMode.Automatic };
    private static Panel CardPanel() => new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0) };
    private static Control ErrorCard(string message)
    {
        var card = CardPanel(); card.Dock = DockStyle.Top; card.Height = 70; card.Padding = new Padding(14);
        card.Controls.Add(new Label { Text = message, Dock = DockStyle.Fill, ForeColor = Color.Firebrick, TextAlign = ContentAlignment.MiddleLeft });
        return card;
    }
    private static Control OfflineNoticeCard(string message)
    {
        var card = CardPanel(); card.Dock = DockStyle.Top; card.Height = 72; card.Padding = new Padding(16);
        card.BackColor = Color.FromArgb(255, 249, 230);
        card.Controls.Add(new Label { Text = "●  " + message, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(132, 88, 0), TextAlign = ContentAlignment.MiddleLeft });
        return card;
    }
    private static Button ActionButton(string text, int left = 0, int top = 0, int width = 135) =>
        new() { Text = text, Left = left, Top = top, Height = 38, Width = width, FlatStyle = FlatStyle.Flat, BackColor = Blue, ForeColor = Color.White, Cursor = Cursors.Hand, Font = new Font("Segoe UI Semibold", 9), Margin = new Padding(0, 0, 8, 0) };
}

internal sealed class DeviceEditorDialog : Form
{
    private readonly ComboBox _lakes = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _id = new();
    private readonly TextBox _ip = new();
    private readonly TextBox _newLake = new();
    private readonly CheckBox _enabled = new() { Text = "Cho phép thiết bị trong hệ thống", Checked = true, AutoSize = true };
    private readonly bool _isNew;
    public string DeviceId => _id.Text.Trim();
    public string IpAddress => string.IsNullOrWhiteSpace(_ip.Text) ? "0.0.0.0" : _ip.Text.Trim();
    public string NewLakeName => _newLake.Text.Trim();
    public int LakeId => (_lakes.SelectedItem as LakeOption)?.Id ?? 0;
    public bool IsEnabled => _enabled.Checked;

    public DeviceEditorDialog(List<LakeOption> lakes, Device? device)
    {
        _isNew = device is null;
        Text = _isNew ? "Thêm thiết bị" : "Sửa thiết bị";
        FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(500, _isNew ? 390 : 330); MaximizeBox = false; MinimizeBox = false;
        Font = new Font("Segoe UI", 9.5f); BackColor = Color.White;
        AddLabel("Mã thiết bị (deviceId)", 24, 20); _id.SetBounds(24, 44, 440, 28); Controls.Add(_id);
        AddLabel("Địa chỉ IP (có thể để 0.0.0.0)", 24, 82); _ip.SetBounds(24, 106, 440, 28); Controls.Add(_ip);
        AddLabel("Hồ nuôi", 24, 144); _lakes.SetBounds(24, 168, 440, 28); _lakes.DataSource = lakes; Controls.Add(_lakes);
        if (_isNew)
        {
            AddLabel("Hoặc tạo hồ mới (để trống nếu chọn hồ ở trên)", 24, 208);
            _newLake.SetBounds(24, 232, 440, 28); Controls.Add(_newLake);
        }
        else
        {
            _enabled.SetBounds(24, 212, 440, 28); Controls.Add(_enabled);
        }
        var save = new Button { Text = _isNew ? "Thêm thiết bị" : "Lưu thay đổi", Bounds = new Rectangle(24, ClientSize.Height - 56, 150, 36), DialogResult = DialogResult.None, BackColor = Color.FromArgb(23, 105, 170), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(DeviceId)) { MessageBox.Show(this, "Nhập mã thiết bị."); return; }
            if (_isNew && LakeId == 0 && string.IsNullOrWhiteSpace(NewLakeName)) { MessageBox.Show(this, "Chọn hồ hoặc nhập tên hồ mới."); return; }
            DialogResult = DialogResult.OK; Close();
        };
        Controls.Add(save); AcceptButton = save;
        if (device is not null)
        {
            _id.Text = device.Ma_may; _id.ReadOnly = true; _ip.Text = device.Current_IP; _enabled.Checked = device.Trang_thai;
            _lakes.SelectedItem = lakes.FirstOrDefault(x => x.Id == device.Ho_nuoi_id);
        }
        else if (lakes.Count == 0)
        {
            _newLake.Text = "Hồ mới";
        }
    }

    private void AddLabel(string text, int x, int y) => Controls.Add(new Label { Text = text, Left = x, Top = y, Width = 440, Height = 20, ForeColor = Color.FromArgb(23, 59, 97) });
}

internal sealed class CalibrationDialog : Form
{
    private readonly TextBox _ph = new() { Text = "0.00" };
    private readonly TextBox _tds = new() { Text = "1.00" };
    private readonly TextBox _temp = new() { Text = "0.00" };
    public float PhOffset { get; private set; }
    public float TdsFactor { get; private set; }
    public float TempOffset { get; private set; }

    public CalibrationDialog(string deviceId)
    {
        Text = "Hiệu chuẩn · " + deviceId; FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(460, 310); MaximizeBox = false; MinimizeBox = false;
        AddField("Độ lệch pH", _ph, 24, 25); AddField("Hệ số TDS (> 0)", _tds, 24, 90); AddField("Độ lệch nhiệt độ", _temp, 24, 155);
        var send = new Button { Text = "Gửi lệnh MQTT", Bounds = new Rectangle(24, 235, 160, 36), BackColor = Color.FromArgb(23, 105, 170), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        send.Click += (_, _) =>
        {
            var culture = CultureInfo.InvariantCulture;
            if (!float.TryParse(_ph.Text.Replace(',', '.'), NumberStyles.Float, culture, out var ph) ||
                !float.TryParse(_tds.Text.Replace(',', '.'), NumberStyles.Float, culture, out var factor) || factor <= 0 ||
                !float.TryParse(_temp.Text.Replace(',', '.'), NumberStyles.Float, culture, out var temp))
            { MessageBox.Show(this, "Nhập số hợp lệ; hệ số TDS phải lớn hơn 0."); return; }
            PhOffset = ph; TdsFactor = factor; TempOffset = temp; DialogResult = DialogResult.OK; Close();
        };
        Controls.Add(send); AcceptButton = send;
    }

    private void AddField(string label, TextBox box, int x, int y)
    {
        Controls.Add(new Label { Text = label, Left = x, Top = y, Width = 400, Height = 20, ForeColor = Color.FromArgb(23, 59, 97) });
        box.SetBounds(x, y + 24, 400, 28); Controls.Add(box);
    }
}

internal sealed class ReadingTrendChart : Control
{
    private readonly IReadOnlyList<Information_device> _readings;

    public ReadingTrendChart(IReadOnlyList<Information_device> readings)
    {
        _readings = readings;
        DoubleBuffered = true;
        BackColor = Color.White;
        MinimumSize = new Size(200, 120);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var plot = new Rectangle(52, 14, Math.Max(20, Width - 72), Math.Max(20, Height - 48));
        using var gridPen = new Pen(Color.FromArgb(232, 237, 243));
        using var labelBrush = new SolidBrush(Color.FromArgb(113, 128, 150));
        using var font = new Font("Segoe UI", 8);
        foreach (var pH in new[] { 0, 7, 14 })
        {
            var y = plot.Bottom - (float)pH / 14 * plot.Height;
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            g.DrawString(pH.ToString(CultureInfo.InvariantCulture), font, labelBrush, 12, y - 8);
        }
        if (_readings.Count == 0)
        {
            g.DrawString("Chưa có số đo trong 24 giờ gần nhất", font, labelBrush, plot.Left + 10, plot.Top + 12);
            return;
        }

        var start = _readings.Min(x => x.ThoiGian).Ticks;
        var end = _readings.Max(x => x.ThoiGian).Ticks;
        var span = Math.Max(1, end - start);
        PointF Map(Information_device item)
        {
            var x = plot.Left + (float)(item.ThoiGian.Ticks - start) / span * plot.Width;
            var ph = Math.Clamp(item.Nong_do_pH, 0, 14);
            var y = plot.Bottom - ph / 14f * plot.Height;
            return new PointF(x, y);
        }
        var points = _readings.Select(Map).ToArray();
        using var linePen = new Pen(Color.FromArgb(23, 105, 170), 2.5f);
        if (points.Length > 1) g.DrawLines(linePen, points);
        using var dotBrush = new SolidBrush(Color.FromArgb(23, 105, 170));
        foreach (var point in points) g.FillEllipse(dotBrush, point.X - 3, point.Y - 3, 6, 6);
        g.DrawString(_readings[0].ThoiGian.ToString("HH:mm"), font, labelBrush, plot.Left, plot.Bottom + 6);
        var lastLabel = _readings[^1].ThoiGian.ToString("HH:mm");
        var labelWidth = g.MeasureString(lastLabel, font).Width;
        g.DrawString(lastLabel, font, labelBrush, plot.Right - labelWidth, plot.Bottom + 6);
    }
}
