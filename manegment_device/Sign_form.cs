using System.Text.RegularExpressions;
using Timer = System.Windows.Forms.Timer;

namespace manegment_device;

public partial class Sign_form : Form
{
    private readonly Timer fadeTimer;
    private bool _registered;

    public string RegisteredUsername { get; private set; } = "";

    public Sign_form()
    {
        InitializeComponent();
        ClientSize = new Size(560, 625);
        MinimumSize = new Size(540, 590);
        glassPanel.Size = new Size(450, 490);
        glassPanel.Location = new Point((ClientSize.Width - glassPanel.Width) / 2, (ClientSize.Height - glassPanel.Height) / 2);
        shadowPanel.Size = glassPanel.Size;
        shadowPanel.Location = new Point(glassPanel.Left + 15, glassPanel.Top + 14);
        SizeChanged += (_, _) =>
        {
            glassPanel.Location = new Point((ClientSize.Width - glassPanel.Width) / 2, (ClientSize.Height - glassPanel.Height) / 2);
            shadowPanel.Location = new Point(glassPanel.Left + 15, glassPanel.Top + 14);
        };
        Sign.Left = (glassPanel.Width - Sign.Width) / 2;
        announce3.AutoSize = false;
        announce3.Location = new Point(45, 395);
        announce3.Size = new Size(360, 34);
        announce3.TextAlign = ContentAlignment.MiddleCenter;
        Back.Location = new Point(45, 438);
        Back.Size = new Size(350, 38);
        glassPanel.BringToFront();
        Opacity = 0;
        fadeTimer = new Timer { Interval = 15 };
        fadeTimer.Tick += FadeTimer_Tick;
        FormClosed += (_, _) => fadeTimer.Dispose();

        CR_user.Text = "";
        CR_pass.Text = "";
        Check_CR_pass.Text = "";
        CR_pass.UseSystemPasswordChar = true;
        Check_CR_pass.UseSystemPasswordChar = true;

        Back.Visible = true;
        Back.Text = "Quay lại đăng nhập";
        Back.Enabled = true;
        Back.BringToFront();
        announce3.Visible = false;
        annouce1.AutoSize = false;
        annouce1.Size = new Size(350, 18);
        announce2.AutoSize = false;
        announce2.Size = new Size(350, 18);
        announce4.AutoSize = false;
        announce4.Size = new Size(350, 24);
        annouce1.Visible = false;
        announce2.Visible = false;
        announce4.Visible = false;
    }

    private void FadeTimer_Tick(object? sender, EventArgs e)
    {
        if (Opacity < 1.0) Opacity = Math.Min(1.0, Opacity + 0.05);
        else fadeTimer.Stop();
    }

    private void Sign_form_Load(object sender, EventArgs e) => fadeTimer.Start();

    private void CR_user_TextChanged(object sender, EventArgs e)
    {
        if (_registered) return;
        announce3.Visible = false;
        var value = CR_user.Text.Trim();
        if (value.Length == 0)
        {
            annouce1.Text = "Tên đăng nhập không được để trống.";
            annouce1.Visible = true;
        }
        else
        {
            annouce1.Visible = false;
        }
    }

    private void CR_pass_TextChanged(object sender, EventArgs e)
    {
        if (_registered) return;
        announce3.Visible = false;
        var value = CR_pass.Text;
        if (value.Length == 0)
        {
            announce2.Visible = false;
        }
        else if (value.Length <= 5 || !Regex.IsMatch(value, @"[^\w]"))
        {
            announce2.Text = "Mật khẩu cần trên 5 ký tự và có ít nhất 1 ký tự đặc biệt.";
            announce2.Visible = true;
        }
        else
        {
            announce2.Visible = false;
        }
        ValidateConfirmation();
    }

    private void Check_CR_pass_TextChanged(object sender, EventArgs e)
    {
        announce3.Visible = false;
        ValidateConfirmation();
    }

    private void ValidateConfirmation()
    {
        if (string.IsNullOrEmpty(Check_CR_pass.Text))
        {
            announce4.Visible = false;
            return;
        }

        bool matches = string.Equals(CR_pass.Text, Check_CR_pass.Text, StringComparison.Ordinal);
        announce4.Text = matches ? "Mật khẩu xác nhận khớp." : "Mật khẩu xác nhận chưa khớp.";
        announce4.ForeColor = matches ? Color.SeaGreen : Color.Crimson;
        announce4.Visible = !matches;
    }

    private async void Check_account_Click(object sender, EventArgs e)
    {
        announce3.Visible = false;
        var username = CR_user.Text.Trim();
        var password = CR_pass.Text;
        var confirmation = Check_CR_pass.Text;

        if (string.IsNullOrWhiteSpace(username))
        {
            annouce1.Text = "Tên đăng nhập không được để trống.";
            annouce1.Visible = true;
            CR_user.Focus();
            return;
        }
        if (password.Length <= 5 || !Regex.IsMatch(password, @"[^\w]"))
        {
            announce2.Text = "Mật khẩu cần trên 5 ký tự và có ít nhất 1 ký tự đặc biệt.";
            announce2.Visible = true;
            CR_pass.Focus();
            return;
        }
        if (confirmation.Length == 0 || !string.Equals(password, confirmation, StringComparison.Ordinal))
        {
            announce4.Text = confirmation.Length == 0 ? "Vui lòng tự nhập mật khẩu xác nhận." : "Mật khẩu xác nhận chưa khớp.";
            announce4.ForeColor = Color.Crimson;
            announce4.Visible = true;
            Check_CR_pass.Focus();
            return;
        }

        Check_account.Enabled = false;
        Check_account.Text = "ĐANG TẠO TÀI KHOẢN...";
        annouce1.Visible = false;
        announce2.Visible = false;
        try
        {
            var duplicateCheck = await Task.Run(() =>
            {
                try
                {
                    return (Success: true, Exists: Sign_query.checking_user_sign(username), Error: (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (Success: false, Exists: false, Error: ex);
                }
            });
            if (!duplicateCheck.Success)
            {
                Console.WriteLine("Không kiểm tra được tài khoản: " + duplicateCheck.Error?.Message);
                ShowRegistrationError("Không kết nối được MySQL. Hãy liên hệ quản trị viên kiểm tra máy chủ database.");
                return;
            }

            bool exists = duplicateCheck.Exists;
            if (exists)
            {
                annouce1.Text = "Tên đăng nhập này đã được sử dụng.";
                annouce1.Visible = true;
                CR_user.Focus();
                return;
            }

            string passwordHash = await Task.Run(() => Hash.HashPassword(password));
            var insertResult = await Task.Run(() =>
            {
                try
                {
                    return (Success: true, Inserted: Sign_query.Insert_user(username, passwordHash, "member"), Error: (Exception?)null);
                }
                catch (Exception ex)
                {
                    return (Success: false, Inserted: false, Error: ex);
                }
            });
            if (!insertResult.Success)
            {
                Console.WriteLine("Không tạo được tài khoản: " + insertResult.Error?.Message);
                ShowRegistrationError("Không lưu được tài khoản vào MySQL. Kiểm tra kết nối và quyền truy cập.");
                return;
            }
            if (!insertResult.Inserted)
            {
                ShowRegistrationError("Không thể tạo tài khoản. Database không lưu được thông tin.");
                return;
            }

            RegisteredUsername = username;
            _registered = true;
            announce3.Text = "Đăng ký thành công. Bấm nút dưới để về đăng nhập.";
            announce3.ForeColor = Color.MediumSeaGreen;
            announce3.Visible = true;
            Back.Text = "Trở về đăng nhập";
            CR_user.Enabled = false;
            CR_pass.Enabled = false;
            Check_CR_pass.Enabled = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi đăng ký: " + ex);
            ShowRegistrationError("Không kết nối được database. Hãy liên hệ quản trị viên kiểm tra máy chủ.");
        }
        finally
        {
            Check_account.Enabled = !_registered;
            Check_account.Text = _registered ? "ĐÃ TẠO TÀI KHOẢN" : "TẠO TÀI KHOẢN";
        }
    }

    private void ShowRegistrationError(string message)
    {
        announce3.Text = message;
        announce3.ForeColor = Color.Crimson;
        announce3.Visible = true;
        announce3.BringToFront();
    }

    private void Back_Click(object sender, EventArgs e)
    {
        DialogResult = _registered ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    private void eye1_CheckedChanged(object sender, EventArgs e) =>
        CR_pass.UseSystemPasswordChar = !eye1.Checked;

    private void checkBox1_CheckedChanged(object sender, EventArgs e) =>
        Check_CR_pass.UseSystemPasswordChar = !eye2.Checked;

    // Designer event handlers retained for the existing form layout.
    public void delete_space(object sender, EventArgs e) { }
    private void announce3_Click(object sender, EventArgs e) { }
    private void annouce1_Click(object sender, EventArgs e) { }
    private void Back_TextChanged(object sender, EventArgs e) { }
    private void announce2_Click(object sender, EventArgs e) { }
    private void announce4_Click(object sender, EventArgs e) { }
    private void Sign_Click(object sender, EventArgs e) { }
    private void glassPanel_Paint(object sender, PaintEventArgs e) { }
}
