using System.ComponentModel;

namespace manegment_device
{
    partial class Menuu
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlSidebar = new System.Windows.Forms.Panel();
            pnlLogo = new System.Windows.Forms.Panel();
            lblLogo = new System.Windows.Forms.Label();
            lblOrgName = new System.Windows.Forms.Label();
            pnlNavHome = new System.Windows.Forms.Panel();
            lblNavHomeIcon = new System.Windows.Forms.Label();
            lblNavHomeText = new System.Windows.Forms.Label();
            pnlNavDevices = new System.Windows.Forms.Panel();
            lblNavDevicesIcon = new System.Windows.Forms.Label();
            lblNavDevicesText = new System.Windows.Forms.Label();
            pnlNavAdmin = new System.Windows.Forms.Panel();
            lblNavAdminIcon = new System.Windows.Forms.Label();
            lblNavAdminText = new System.Windows.Forms.Label();
            pnlMain = new System.Windows.Forms.Panel();
            pnlCardList = new System.Windows.Forms.Panel();
            lblCardListTitle = new System.Windows.Forms.Label();
            txtSearchDevice = new ReaLTaiizor.Controls.HopeTextBox();
            flpDeviceList = new System.Windows.Forms.FlowLayoutPanel();
            pnlCardAdd = new System.Windows.Forms.Panel();
            lblCardAddTitle = new System.Windows.Forms.Label();
            btnAddDevice = new ReaLTaiizor.Controls.HopeButton();
            lblCardAddSub = new System.Windows.Forms.Label();
            lblDashboardTitle = new System.Windows.Forms.Label();
            pnlSidebar.SuspendLayout();
            pnlLogo.SuspendLayout();
            pnlNavHome.SuspendLayout();
            pnlNavDevices.SuspendLayout();
            pnlNavAdmin.SuspendLayout();
            pnlMain.SuspendLayout();
            pnlCardList.SuspendLayout();
            pnlCardAdd.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = System.Drawing.Color.White;
            pnlSidebar.Controls.Add(pnlLogo);
            pnlSidebar.Controls.Add(lblOrgName);
            pnlSidebar.Controls.Add(pnlNavHome);
            pnlSidebar.Controls.Add(pnlNavDevices);
            pnlSidebar.Controls.Add(pnlNavAdmin);
            pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            pnlSidebar.Location = new System.Drawing.Point(0, 0);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new System.Drawing.Size(230, 750);
            pnlSidebar.TabIndex = 0;
            // 
            // pnlLogo
            // 
            pnlLogo.BackColor = System.Drawing.Color.FromArgb(((int)((byte)250)), ((int)((byte)180)), ((int)((byte)40)));
            pnlLogo.Controls.Add(lblLogo);
            pnlLogo.Location = new System.Drawing.Point(20, 24);
            pnlLogo.Name = "pnlLogo";
            pnlLogo.Size = new System.Drawing.Size(46, 46);
            pnlLogo.TabIndex = 0;
            // 
            // lblLogo
            // 
            lblLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblLogo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            lblLogo.ForeColor = System.Drawing.Color.White;
            lblLogo.Location = new System.Drawing.Point(0, 0);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new System.Drawing.Size(46, 46);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "NL";
            lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblOrgName
            // 
            lblOrgName.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            lblOrgName.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)50)), ((int)((byte)50)), ((int)((byte)50)));
            lblOrgName.Location = new System.Drawing.Point(78, 26);
            lblOrgName.Name = "lblOrgName";
            lblOrgName.Size = new System.Drawing.Size(140, 44);
            lblOrgName.TabIndex = 1;
            lblOrgName.Text = "Hệ thống Quản lý Thiết bị NL";
            // 
            // pnlNavHome
            // 
            pnlNavHome.BackColor = System.Drawing.Color.White;
            pnlNavHome.Controls.Add(lblNavHomeIcon);
            pnlNavHome.Controls.Add(lblNavHomeText);
            pnlNavHome.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlNavHome.Location = new System.Drawing.Point(12, 110);
            pnlNavHome.Name = "pnlNavHome";
            pnlNavHome.Size = new System.Drawing.Size(206, 44);
            pnlNavHome.TabIndex = 2;
            pnlNavHome.Click += NavHome_Click;
            // 
            // lblNavHomeIcon
            // 
            lblNavHomeIcon.AutoSize = true;
            lblNavHomeIcon.BackColor = System.Drawing.Color.Transparent;
            lblNavHomeIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            lblNavHomeIcon.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblNavHomeIcon.Location = new System.Drawing.Point(16, 11);
            lblNavHomeIcon.Name = "lblNavHomeIcon";
            lblNavHomeIcon.Size = new System.Drawing.Size(39, 28);
            lblNavHomeIcon.TabIndex = 0;
            lblNavHomeIcon.Text = "🏠";
            lblNavHomeIcon.Click += NavHome_Click;
            // 
            // lblNavHomeText
            // 
            lblNavHomeText.AutoSize = true;
            lblNavHomeText.BackColor = System.Drawing.Color.Transparent;
            lblNavHomeText.Cursor = System.Windows.Forms.Cursors.Hand;
            lblNavHomeText.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            lblNavHomeText.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)196)), ((int)((byte)130)), ((int)((byte)20)));
            lblNavHomeText.Location = new System.Drawing.Point(61, 11);
            lblNavHomeText.Name = "lblNavHomeText";
            lblNavHomeText.Size = new System.Drawing.Size(57, 23);
            lblNavHomeText.TabIndex = 1;
            lblNavHomeText.Text = "Home";
            lblNavHomeText.Click += NavHome_Click;
            // 
            // pnlNavDevices
            // 
            pnlNavDevices.BackColor = System.Drawing.Color.Transparent;
            pnlNavDevices.Controls.Add(lblNavDevicesIcon);
            pnlNavDevices.Controls.Add(lblNavDevicesText);
            pnlNavDevices.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlNavDevices.Location = new System.Drawing.Point(12, 162);
            pnlNavDevices.Name = "pnlNavDevices";
            pnlNavDevices.Size = new System.Drawing.Size(206, 44);
            pnlNavDevices.TabIndex = 3;
            pnlNavDevices.Click += NavDevices_Click;
            pnlNavDevices.Paint += pnlNavDevices_Paint;
            // 
            // lblNavDevicesIcon
            // 
            lblNavDevicesIcon.AutoSize = true;
            lblNavDevicesIcon.BackColor = System.Drawing.Color.Transparent;
            lblNavDevicesIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            lblNavDevicesIcon.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblNavDevicesIcon.Location = new System.Drawing.Point(16, 11);
            lblNavDevicesIcon.Name = "lblNavDevicesIcon";
            lblNavDevicesIcon.Size = new System.Drawing.Size(39, 28);
            lblNavDevicesIcon.TabIndex = 0;
            lblNavDevicesIcon.Text = "📋";
            lblNavDevicesIcon.Click += NavDevices_Click;
            // 
            // lblNavDevicesText
            // 
            lblNavDevicesText.AutoSize = true;
            lblNavDevicesText.BackColor = System.Drawing.Color.Transparent;
            lblNavDevicesText.Cursor = System.Windows.Forms.Cursors.Hand;
            lblNavDevicesText.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblNavDevicesText.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)90)), ((int)((byte)90)), ((int)((byte)90)));
            lblNavDevicesText.Location = new System.Drawing.Point(61, 11);
            lblNavDevicesText.Name = "lblNavDevicesText";
            lblNavDevicesText.Size = new System.Drawing.Size(67, 23);
            lblNavDevicesText.TabIndex = 1;
            lblNavDevicesText.Text = "Devices";
            lblNavDevicesText.Click += NavDevices_Click;
            // 
            // pnlNavAdmin
            // 
            pnlNavAdmin.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            pnlNavAdmin.BackColor = System.Drawing.Color.Transparent;
            pnlNavAdmin.Controls.Add(lblNavAdminIcon);
            pnlNavAdmin.Controls.Add(lblNavAdminText);
            pnlNavAdmin.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlNavAdmin.Location = new System.Drawing.Point(12, 660);
            pnlNavAdmin.Name = "pnlNavAdmin";
            pnlNavAdmin.Size = new System.Drawing.Size(206, 44);
            pnlNavAdmin.TabIndex = 4;
            pnlNavAdmin.Click += NavAdmin_Click;
            // 
            // lblNavAdminIcon
            // 
            lblNavAdminIcon.AutoSize = true;
            lblNavAdminIcon.BackColor = System.Drawing.Color.Transparent;
            lblNavAdminIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            lblNavAdminIcon.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblNavAdminIcon.Location = new System.Drawing.Point(14, 10);
            lblNavAdminIcon.Name = "lblNavAdminIcon";
            lblNavAdminIcon.Size = new System.Drawing.Size(39, 28);
            lblNavAdminIcon.TabIndex = 0;
            lblNavAdminIcon.Text = "👤";
            lblNavAdminIcon.Click += NavAdmin_Click;
            // 
            // lblNavAdminText
            // 
            lblNavAdminText.AutoSize = true;
            lblNavAdminText.BackColor = System.Drawing.Color.Transparent;
            lblNavAdminText.Cursor = System.Windows.Forms.Cursors.Hand;
            lblNavAdminText.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblNavAdminText.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)90)), ((int)((byte)90)), ((int)((byte)90)));
            lblNavAdminText.Location = new System.Drawing.Point(68, 15);
            lblNavAdminText.Name = "lblNavAdminText";
            lblNavAdminText.Size = new System.Drawing.Size(60, 23);
            lblNavAdminText.TabIndex = 1;
            lblNavAdminText.Text = "Admin";
            lblNavAdminText.Click += NavAdmin_Click;
            // 
            // pnlMain
            // 
            pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)((byte)250)), ((int)((byte)252)), ((int)((byte)255)));
            pnlMain.Controls.Add(pnlCardList);
            pnlMain.Controls.Add(pnlCardAdd);
            pnlMain.Controls.Add(lblDashboardTitle);
            pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlMain.Location = new System.Drawing.Point(230, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new System.Drawing.Size(970, 750);
            pnlMain.TabIndex = 1;
            // 
            // pnlCardList
            // 
            pnlCardList.BackColor = System.Drawing.Color.White;
            pnlCardList.Controls.Add(lblCardListTitle);
            pnlCardList.Controls.Add(txtSearchDevice);
            pnlCardList.Controls.Add(flpDeviceList);
            pnlCardList.Location = new System.Drawing.Point(392, 80);
            pnlCardList.Name = "pnlCardList";
            pnlCardList.Size = new System.Drawing.Size(554, 428);
            pnlCardList.TabIndex = 2;
            pnlCardList.Paint += Card_Paint;
            // 
            // lblCardListTitle
            // 
            lblCardListTitle.AutoSize = true;
            lblCardListTitle.BackColor = System.Drawing.Color.Transparent;
            lblCardListTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            lblCardListTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)50)), ((int)((byte)50)), ((int)((byte)50)));
            lblCardListTitle.Location = new System.Drawing.Point(20, 16);
            lblCardListTitle.Name = "lblCardListTitle";
            lblCardListTitle.Size = new System.Drawing.Size(178, 25);
            lblCardListTitle.TabIndex = 0;
            lblCardListTitle.Text = "Danh sách Máy (IP)";
            // 
            // txtSearchDevice
            // 
            txtSearchDevice.BackColor = System.Drawing.Color.White;
            txtSearchDevice.BaseColor = System.Drawing.Color.White;
            txtSearchDevice.BorderColorA = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)158)), ((int)((byte)255)));
            txtSearchDevice.BorderColorB = System.Drawing.Color.FromArgb(((int)((byte)220)), ((int)((byte)223)), ((int)((byte)230)));
            txtSearchDevice.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            txtSearchDevice.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)60)), ((int)((byte)60)), ((int)((byte)60)));
            txtSearchDevice.Hint = "Tìm kiếm...";
            txtSearchDevice.Location = new System.Drawing.Point(20, 50);
            txtSearchDevice.MaxLength = 32767;
            txtSearchDevice.Multiline = false;
            txtSearchDevice.Name = "txtSearchDevice";
            txtSearchDevice.PasswordChar = '\0';
            txtSearchDevice.ScrollBars = System.Windows.Forms.ScrollBars.None;
            txtSearchDevice.SelectedText = "";
            txtSearchDevice.SelectionLength = 0;
            txtSearchDevice.SelectionStart = 0;
            txtSearchDevice.Size = new System.Drawing.Size(511, 38);
            txtSearchDevice.TabIndex = 1;
            txtSearchDevice.TabStop = false;
            txtSearchDevice.UseSystemPasswordChar = false;
            txtSearchDevice.TextChanged += txtSearchDevice_TextChanged;
            // 
            // flpDeviceList
            // 
            flpDeviceList.AutoScroll = true;
            flpDeviceList.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flpDeviceList.Location = new System.Drawing.Point(20, 96);
            flpDeviceList.Name = "flpDeviceList";
            flpDeviceList.Size = new System.Drawing.Size(511, 314);
            flpDeviceList.TabIndex = 2;
            flpDeviceList.WrapContents = false;
            // 
            // pnlCardAdd
            // 
            pnlCardAdd.BackColor = System.Drawing.Color.White;
            pnlCardAdd.Controls.Add(lblCardAddTitle);
            pnlCardAdd.Controls.Add(btnAddDevice);
            pnlCardAdd.Controls.Add(lblCardAddSub);
            pnlCardAdd.Location = new System.Drawing.Point(32, 80);
            pnlCardAdd.Name = "pnlCardAdd";
            pnlCardAdd.Size = new System.Drawing.Size(354, 616);
            pnlCardAdd.TabIndex = 1;
            pnlCardAdd.Paint += Card_Paint;
            // 
            // lblCardAddTitle
            // 
            lblCardAddTitle.AutoSize = true;
            lblCardAddTitle.BackColor = System.Drawing.Color.Transparent;
            lblCardAddTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            lblCardAddTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)50)), ((int)((byte)50)), ((int)((byte)50)));
            lblCardAddTitle.Location = new System.Drawing.Point(20, 18);
            lblCardAddTitle.Name = "lblCardAddTitle";
            lblCardAddTitle.Size = new System.Drawing.Size(169, 25);
            lblCardAddTitle.TabIndex = 0;
            lblCardAddTitle.Text = "Thêm Thiết bị Mới";
            // 
            // btnAddDevice
            // 
            btnAddDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            btnAddDevice.BorderColor = System.Drawing.Color.FromArgb(((int)((byte)220)), ((int)((byte)223)), ((int)((byte)230)));
            btnAddDevice.ButtonType = ReaLTaiizor.Util.HopeButtonType.Primary;
            btnAddDevice.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddDevice.DangerColor = System.Drawing.Color.FromArgb(((int)((byte)245)), ((int)((byte)108)), ((int)((byte)108)));
            btnAddDevice.DefaultColor = System.Drawing.Color.White;
            btnAddDevice.Font = new System.Drawing.Font("Segoe UI", 22F);
            btnAddDevice.HoverTextColor = System.Drawing.Color.White;
            btnAddDevice.InfoColor = System.Drawing.Color.FromArgb(((int)((byte)144)), ((int)((byte)147)), ((int)((byte)153)));
            btnAddDevice.Location = new System.Drawing.Point(137, 261);
            btnAddDevice.Name = "btnAddDevice";
            btnAddDevice.PrimaryColor = System.Drawing.Color.FromArgb(((int)((byte)250)), ((int)((byte)180)), ((int)((byte)40)));
            btnAddDevice.Size = new System.Drawing.Size(70, 70);
            btnAddDevice.SuccessColor = System.Drawing.Color.FromArgb(((int)((byte)103)), ((int)((byte)194)), ((int)((byte)58)));
            btnAddDevice.TabIndex = 1;
            btnAddDevice.Text = "+";
            btnAddDevice.TextColor = System.Drawing.Color.White;
            btnAddDevice.WarningColor = System.Drawing.Color.FromArgb(((int)((byte)230)), ((int)((byte)162)), ((int)((byte)60)));
            btnAddDevice.Click += btnAddDevice_Click;
            // 
            // lblCardAddSub
            // 
            lblCardAddSub.BackColor = System.Drawing.Color.Transparent;
            lblCardAddSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblCardAddSub.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)140)), ((int)((byte)140)), ((int)((byte)140)));
            lblCardAddSub.Location = new System.Drawing.Point(48, 351);
            lblCardAddSub.Name = "lblCardAddSub";
            lblCardAddSub.Size = new System.Drawing.Size(260, 50);
            lblCardAddSub.TabIndex = 2;
            lblCardAddSub.Text = "Bắt đầu thêm một thiết bị mới vào hệ thống";
            lblCardAddSub.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDashboardTitle
            // 
            lblDashboardTitle.AutoSize = true;
            lblDashboardTitle.BackColor = System.Drawing.Color.White;
            lblDashboardTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            lblDashboardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)((byte)40)), ((int)((byte)40)), ((int)((byte)40)));
            lblDashboardTitle.Location = new System.Drawing.Point(32, 24);
            lblDashboardTitle.Name = "lblDashboardTitle";
            lblDashboardTitle.Size = new System.Drawing.Size(193, 46);
            lblDashboardTitle.TabIndex = 0;
            lblDashboardTitle.Text = "Dashboard";
            // 
            // Menuu
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1200, 750);
            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MinimumSize = new System.Drawing.Size(1000, 600);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Hệ thống Quản lý Thiết bị NL";
            Load += Menuu_Load;
            pnlSidebar.ResumeLayout(false);
            pnlLogo.ResumeLayout(false);
            pnlNavHome.ResumeLayout(false);
            pnlNavHome.PerformLayout();
            pnlNavDevices.ResumeLayout(false);
            pnlNavDevices.PerformLayout();
            pnlNavAdmin.ResumeLayout(false);
            pnlNavAdmin.PerformLayout();
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlCardList.ResumeLayout(false);
            pnlCardList.PerformLayout();
            pnlCardAdd.ResumeLayout(false);
            pnlCardAdd.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlLogo;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Label lblOrgName;
        private System.Windows.Forms.Panel pnlNavHome;
        private System.Windows.Forms.Label lblNavHomeIcon;
        private System.Windows.Forms.Label lblNavHomeText;
        private System.Windows.Forms.Panel pnlNavDevices;
        private System.Windows.Forms.Label lblNavDevicesIcon;
        private System.Windows.Forms.Label lblNavDevicesText;
        private System.Windows.Forms.Panel pnlNavAdmin;
        private System.Windows.Forms.Label lblNavAdminIcon;
        private System.Windows.Forms.Label lblNavAdminText;

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblDashboardTitle;

        private System.Windows.Forms.Panel pnlCardAdd;
        private System.Windows.Forms.Label lblCardAddTitle;
        private ReaLTaiizor.Controls.HopeButton btnAddDevice;
        private System.Windows.Forms.Label lblCardAddSub;

        private System.Windows.Forms.Panel pnlCardList;
        private System.Windows.Forms.Label lblCardListTitle;
        private ReaLTaiizor.Controls.HopeTextBox txtSearchDevice;
        private System.Windows.Forms.FlowLayoutPanel flpDeviceList;
    }
}