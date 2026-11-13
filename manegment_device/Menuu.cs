namespace manegment_device;

public partial class Menuu : Form
{
    private readonly Dashboard_query _dashboardQuery;
    private List<Device> _devices = new();

    // Constructor mặc định giữ nguyên (login_form.cs đang gọi `new Menuu()`),
    // bên trong tự tạo Dashboard_query. Overload thứ 2 để tiện test/thay đổi sau này.
    public Menuu() : this(new Dashboard_query())
    {
    }

    public Menuu(Dashboard_query dashboardQuery)
    {
        InitializeComponent();
        _dashboardQuery = dashboardQuery;
    }

    private void Menuu_Load(object sender, EventArgs e)
    {
        UIHelpers.ApplyCircleRegion(pnlLogo);
        UIHelpers.ApplyCircleRegion(btnAddDevice);

        lblNavAdminText.Text = string.IsNullOrWhiteSpace(CurrentUser.Username) ? "Admin" : CurrentUser.Username;

        RefreshDeviceList();
    }

   
    private void RefreshDeviceList()
    {
        try
        {
            _devices = _dashboardQuery.GetAllDevices();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi tải danh sách thiết bị: " + ex.Message);
            _devices = new List<Device>();
        }

        RenderDeviceList(_devices);
    }

    private void RenderDeviceList(IEnumerable<Device> devices)
    {
        flpDeviceList.Controls.Clear();
        foreach (var device in devices)
        {
            flpDeviceList.Controls.Add(BuildDeviceItem(device));
        }
    }

    private Panel BuildDeviceItem(Device device)
    {
        var item = new Panel
        {
            Size = new Size(340, 62),
            Margin = new Padding(0, 0, 0, 8),
            BackColor = Color.FromArgb(250, 251, 253),
            Cursor = Cursors.Hand,
            Tag = device
        };

        var lblName = new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Location = new Point(14, 8),
            Text = device.Ma_may,
            Cursor = Cursors.Hand
        };

        var lblIp = new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(140, 140, 140),
            Location = new Point(14, 26),
            Text = $"IP: {device.Current_IP}",
            Cursor = Cursors.Hand
        };

        var lblStatus = new Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = device.Trang_thai ? Color.FromArgb(56, 176, 87) : Color.FromArgb(200, 80, 80),
            Location = new Point(14, 42),
            Text = device.Trang_thai ? "● Đang hoạt động" : "● Ngắt kết nối",
            Cursor = Cursors.Hand
        };

        item.Controls.Add(lblName);
        item.Controls.Add(lblIp);
        item.Controls.Add(lblStatus);
        item.Paint += Card_Paint;

        void OpenHandler(object? s, EventArgs e) => ShowDeviceDetail(device);
        item.Click += OpenHandler;
        lblName.Click += OpenHandler;
        lblIp.Click += OpenHandler;
        lblStatus.Click += OpenHandler;

        return item;
    }

    private void txtSearchDevice_TextChanged(object sender, EventArgs e)
    {
        string keyword = txtSearchDevice.Text.Trim().ToLower();
        var filtered = string.IsNullOrEmpty(keyword)
            ? _devices
            : _devices.Where(d => d.Ma_may.ToLower().Contains(keyword) || d.Current_IP.Contains(keyword));
        RenderDeviceList(filtered);
    }
    private void ShowDeviceDetail(Device device)
    {
        using var detailForm = new Manager_device();
        detailForm.ShowDialog(this);
    }
    private void Card_Paint(object sender, PaintEventArgs e)
    {
        if (sender is not Control control) return;
        var bounds = new Rectangle(0, 0, control.Width, control.Height);
        UIHelpers.PaintRoundedCard(e.Graphics, bounds, control.BackColor, 12, Color.FromArgb(235, 235, 235));
    }

 
    private void NavHome_Click(object sender, EventArgs e)
    {
        // Đang ở trang Home rồi, không cần làm gì thêm.
    }

    private void NavDevices_Click(object sender, EventArgs e)
    {
        using var managerForm = new Manager_device();
        managerForm.ShowDialog(this);
        RefreshDeviceList();
    }

    private void NavAdmin_Click(object sender, EventArgs e)
    {
        // TODO: mở panel tài khoản / đăng xuất
    }

    private void btnAddDevice_Click(object sender, EventArgs e)
    {
        using var addForm = new ADD_IP_device();
        addForm.ShowDialog(this);
        RefreshDeviceList();
    }

    private void pnlNavDevices_Paint(object sender, PaintEventArgs e)
    {
       
    }
}