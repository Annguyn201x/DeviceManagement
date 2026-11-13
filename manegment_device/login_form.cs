namespace manegment_device;

public partial class login_form : Form
{   
    private readonly ILogin_query _loginQuery;
    private readonly Button _backToHomeButton;
    public login_form(ILogin_query Login_query)
    {
   
        InitializeComponent();
        _loginQuery = Login_query; 
        ClientSize = new Size(700, 540);
        MinimumSize = new Size(620, 500);
        glassPanel.Size = new Size(390, 400);
        glassPanel.Location = new Point((ClientSize.Width - glassPanel.Width) / 2, (ClientSize.Height - glassPanel.Height) / 2);
        shadowPanel.Size = glassPanel.Size;
        shadowPanel.Location = new Point(glassPanel.Left + 8, glassPanel.Top + 8);
        SizeChanged += (_, _) =>
        {
            glassPanel.Location = new Point((ClientSize.Width - glassPanel.Width) / 2, (ClientSize.Height - glassPanel.Height) / 2);
            shadowPanel.Location = new Point(glassPanel.Left + 8, glassPanel.Top + 8);
        };
        anounce.Location = new Point(35, 205);
        anounce.Size = new Size(320, 32);
        anounce.AutoSize = false;
        anounce.TextAlign = ContentAlignment.MiddleCenter;
        lblTitle.Location = new Point(100, 22);
        ini_user.Location = new Point(35, 101);
        ini_user.Size = new Size(320, 38);
        lblUser.Location = new Point(35, 82);
        lblPass.Location = new Point(35, 146);
        Password_form.Location = new Point(35, 165);
        Password_form.Size = new Size(245, 38);
        checkBox1.Location = new Point(289, 171);
        btnLogin.Location = new Point(35, 248);
        btnLogin.Size = new Size(320, 42);
        btnRegister.Location = new Point(35, 304);
        btnRegister.Size = new Size(320, 36);
        _backToHomeButton = new Button
        {
            Name = "btnBackToHome",
            Text = "←  Quay lại màn hình trước",
            Location = new Point(35, 354),
            Size = new Size(320, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(90, 90, 90),
            Font = new Font("Segoe UI", 9.5F),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        _backToHomeButton.FlatAppearance.BorderColor = Color.FromArgb(220, 223, 230);
        _backToHomeButton.Click += (_, _) => Close();
        glassPanel.Controls.Add(_backToHomeButton);
        _backToHomeButton.BringToFront();
        glassPanel.BringToFront();
        anounce.Visible = false;
        ini_user.Text = "";
        ini_user.Hint = "username";
        Password_form.Text = "";
        Password_form.Hint = "password";
        Password_form.UseSystemPasswordChar = true;

    }
    


    private void Designer_Click(object sender, EventArgs e)
    {
       
    }

    private async void button_check_loggin_Click(object sender, EventArgs e)
    {
        string userName = ini_user.Text.Trim();
        string password = Password_form.Text;
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
        {
            anounce.Text = "Nhập tên đăng nhập và mật khẩu.";
            anounce.Visible = true;
            return;
        }

        btnLogin.Enabled = false;
        btnRegister.Enabled = false;
        btnLogin.Text = "ĐANG KIỂM TRA...";
        anounce.Visible = false;
        try
        {
            var authentication = await Task.Run(() =>
            {
                try
                {
                    return (Result: _loginQuery.Authenticate(userName, password), Error: (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (Result: (LoginResult?)null, Error: ex);
                }
            });
            if (authentication.Error is not null || authentication.Result is null)
            {
                Console.WriteLine("Lỗi kết nối khi đăng nhập: " + authentication.Error?.Message);
                anounce.Text = "Không kết nối được MySQL. Kiểm tra cấu hình hoặc trạng thái máy chủ.";
                anounce.Visible = true;
                return;
            }

            var result = authentication.Result;
            if (!result.UserExists)
            {
                anounce.Text = "Tài khoản không tồn tại.";
                anounce.Visible = true;
            }
            else if (!result.IsAuthenticated)
            {
                anounce.Text = "Mật khẩu không chính xác.";
                anounce.Visible = true;
            }
            else
            {
                CurrentUser.Username = userName;
                CurrentUser.Role = result.Role;
                using var dashboard = new WaterMonitorDashboardForm();
                Hide();
                dashboard.ShowDialog();
                Show();
            }
        }
        catch (Exception ex)
        {
            anounce.Text = "Không thể kiểm tra tài khoản. Hãy thử lại.";
            anounce.Visible = true;
            Console.WriteLine("Lỗi đăng nhập: " + ex.Message);
        }
        finally
        {
            btnLogin.Enabled = true;
            btnRegister.Enabled = true;
            btnLogin.Text = "ĐĂNG NHẬP";
        }
    }


    private void Password_form_TextChanged(object sender, EventArgs e)
    {
        anounce.Visible = false;
    }

    private void ini_user_TextChanged(object sender, EventArgs e)
    {
        anounce.Visible = false;
    }

    private void anounce_Click_1(object sender, EventArgs e)
    {
        
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBox1.Checked)
        {
            Password_form.UseSystemPasswordChar = false;
        }
        else
        {
            Password_form.UseSystemPasswordChar = true;
        }
    }

    private void login_form_Load(object sender, EventArgs e)
    {
       
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        using var register = new Sign_form();
        Hide();
        var result = register.ShowDialog();
        Show();
        if (result == DialogResult.OK)
        {
            ini_user.Text = register.RegisteredUsername;
            Password_form.Clear();
            Password_form.Focus();
        }
    }

    private void txtUser_Click(object sender, EventArgs e)
    {
       
    }

    private void glassPanel_Paint(object sender, PaintEventArgs e)
    {
        
    }
}
