namespace manegment_device;

public partial class Home : Form
{
    private readonly Panel _homeCard = new();
    private readonly Panel _homeShadow = new();

    public Home()
    {
        InitializeComponent();
        Text = "Home Lab";
        BackColor = Color.FromArgb(255, 192, 0);
        BackgroundImage = null;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(640, 500);
        ClientSize = new Size(700, 520);
        Font = new Font("Segoe UI", 10F);
        DoubleBuffered = true;

        _homeShadow.BackColor = Color.FromArgb(75, 150, 110, 0);
        _homeShadow.Size = new Size(460, 360);
        _homeCard.BackColor = Color.FromArgb(255, 237, 182);
        _homeCard.Size = new Size(460, 360);

        Controls.Remove(Home_Form);
        Controls.Remove(label_welcome);
        Controls.Remove(Loggin_button);
        Controls.Remove(Sign_button);
        StyleHomeContent();
        _homeCard.Controls.Add(Home_Form);
        _homeCard.Controls.Add(label_welcome);
        _homeCard.Controls.Add(Loggin_button);
        _homeCard.Controls.Add(Sign_button);
        Controls.Add(_homeShadow);
        Controls.Add(_homeCard);
        SizeChanged += (_, _) => CenterHomeCard();
        CenterHomeCard();
    }

    private void StyleHomeContent()
    {
        Home_Form.Text = "Home Lab";
        Home_Form.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
        Home_Form.ForeColor = Color.FromArgb(40, 40, 40);
        Home_Form.Location = new Point(36, 32);
        Home_Form.Size = new Size(388, 48);
        Home_Form.TextAlign = ContentAlignment.MiddleCenter;

        label_welcome.Font = new Font("Segoe UI", 11F);
        label_welcome.ForeColor = Color.FromArgb(105, 105, 105);
        label_welcome.Location = new Point(36, 84);
        label_welcome.Size = new Size(388, 28);
        label_welcome.TextAlign = ContentAlignment.MiddleCenter;

        Loggin_button.Location = new Point(36, 156);
        Loggin_button.Size = new Size(388, 50);
        Loggin_button.FlatStyle = FlatStyle.Flat;
        Loggin_button.FlatAppearance.BorderSize = 0;
        Loggin_button.BackColor = Color.FromArgb(250, 180, 40);
        Loggin_button.ForeColor = Color.White;
        Loggin_button.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        Loggin_button.Cursor = Cursors.Hand;
        Loggin_button.UseVisualStyleBackColor = false;

        Sign_button.Location = new Point(36, 222);
        Sign_button.Size = new Size(388, 50);
        Sign_button.FlatStyle = FlatStyle.Flat;
        Sign_button.FlatAppearance.BorderColor = Color.FromArgb(220, 210, 180);
        Sign_button.FlatAppearance.BorderSize = 1;
        Sign_button.BackColor = Color.FromArgb(248, 244, 232);
        Sign_button.ForeColor = Color.FromArgb(70, 70, 70);
        Sign_button.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        Sign_button.Cursor = Cursors.Hand;
        Sign_button.UseVisualStyleBackColor = false;
    }

    private void CenterHomeCard()
    {
        _homeCard.Location = new Point((ClientSize.Width - _homeCard.Width) / 2, (ClientSize.Height - _homeCard.Height) / 2);
        _homeShadow.Location = new Point(_homeCard.Left + 8, _homeCard.Top + 8);
        _homeCard.BringToFront();
    }
    
    private void Loggin_button_Click(object sender, EventArgs e)
    {
        using var log = new login_form(new Login_query());
        Hide();
        log.ShowDialog();
        Show();
    }
    private void Sign_button_Click_1(object sender, EventArgs e)
    {
        using var sign_form = new Sign_form();
        Hide();
        var result = sign_form.ShowDialog();
        Show();
        if (result == DialogResult.OK)
        {
            using var log = new login_form(new Login_query());
            Hide();
            log.ShowDialog();
            Show();
        }
    }
}
