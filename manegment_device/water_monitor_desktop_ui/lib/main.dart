import 'package:flutter/material.dart';

void main() => runApp(const WaterMonitorApp());

const navy = Color(0xFF173B61);
const blue = Color(0xFF1769AA);
const muted = Color(0xFF718096);
const border = Color(0xFFE5EBF2);

class WaterMonitorApp extends StatelessWidget {
  const WaterMonitorApp({super.key});

  @override
  Widget build(BuildContext context) => MaterialApp(
        title: 'Water Monitor Desktop',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          useMaterial3: true,
          colorScheme: ColorScheme.fromSeed(seedColor: blue),
          scaffoldBackgroundColor: const Color(0xFFF4F7FB),
          cardTheme: CardThemeData(
            color: Colors.white,
            elevation: 0,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
              side: const BorderSide(color: border),
            ),
          ),
        ),
        home: const LoginScreen(),
      );
}

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final user = TextEditingController();
  final password = TextEditingController();

  @override
  void dispose() {
    user.dispose();
    password.dispose();
    super.dispose();
  }

  void enterDemo() => Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(builder: (_) => const MainShell()),
      );

  @override
  Widget build(BuildContext context) => Scaffold(
        body: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 430),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(mainAxisSize: MainAxisSize.min, children: [
                  const _BrandMark(),
                  const SizedBox(height: 18),
                  const Text('Water Monitor',
                      style: TextStyle(
                          fontSize: 24,
                          fontWeight: FontWeight.w600,
                          color: navy)),
                  const SizedBox(height: 6),
                  const Text('Đăng nhập để xem bảng điều khiển',
                      style: TextStyle(color: muted)),
                  const SizedBox(height: 26),
                  TextField(
                      controller: user,
                      decoration: const InputDecoration(
                          labelText: 'Tên đăng nhập',
                          prefixIcon: Icon(Icons.person_outline),
                          border: OutlineInputBorder())),
                  const SizedBox(height: 14),
                  TextField(
                      controller: password,
                      obscureText: true,
                      decoration: const InputDecoration(
                          labelText: 'Mật khẩu',
                          prefixIcon: Icon(Icons.lock_outline),
                          border: OutlineInputBorder())),
                  const SizedBox(height: 20),
                  SizedBox(
                      width: double.infinity,
                      child: FilledButton(
                          onPressed: enterDemo,
                          child: const Padding(
                              padding: EdgeInsets.all(12),
                              child: Text('Vào bản xem thử')))),
                  const SizedBox(height: 12),
                  const Text(
                      'Bản giao diện mẫu · đăng nhập chưa kết nối hệ thống',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: muted, fontSize: 12)),
                ]),
              ),
            ),
          ),
        ),
      );
}

class MainShell extends StatefulWidget {
  const MainShell({super.key});
  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> {
  int selected = 0;
  final pages = const [
    OverviewPage(),
    HistoryPage(),
    DevicesPage(),
    SettingsPage()
  ];
  final labels = const ['Tổng quan', 'Lịch sử', 'Thiết bị', 'Cài đặt'];

  @override
  Widget build(BuildContext context) => Scaffold(
        body: Row(children: [
          NavigationRail(
            extended: MediaQuery.sizeOf(context).width > 1050,
            selectedIndex: selected,
            onDestinationSelected: (value) => setState(() => selected = value),
            backgroundColor: navy,
            unselectedIconTheme: const IconThemeData(color: Colors.white70),
            selectedIconTheme: const IconThemeData(color: Colors.cyanAccent),
            unselectedLabelTextStyle: const TextStyle(color: Colors.white70),
            selectedLabelTextStyle: const TextStyle(
                color: Colors.cyanAccent, fontWeight: FontWeight.w600),
            leading: const Padding(
                padding: EdgeInsets.fromLTRB(10, 22, 10, 30),
                child: _BrandMark()),
            destinations: const [
              NavigationRailDestination(
                  icon: Icon(Icons.dashboard_outlined),
                  selectedIcon: Icon(Icons.dashboard),
                  label: Text('Tổng quan')),
              NavigationRailDestination(
                  icon: Icon(Icons.history), label: Text('Lịch sử')),
              NavigationRailDestination(
                  icon: Icon(Icons.devices_outlined),
                  selectedIcon: Icon(Icons.devices),
                  label: Text('Thiết bị')),
              NavigationRailDestination(
                  icon: Icon(Icons.settings_outlined),
                  selectedIcon: Icon(Icons.settings),
                  label: Text('Cài đặt')),
            ],
          ),
          const VerticalDivider(width: 1, thickness: 1, color: border),
          Expanded(
              child: Column(children: [
            _TopBar(section: labels[selected]),
            Expanded(child: IndexedStack(index: selected, children: pages)),
          ])),
        ]),
      );
}

class _TopBar extends StatelessWidget {
  const _TopBar({required this.section});
  final String section;
  @override
  Widget build(BuildContext context) => Container(
        height: 66,
        padding: const EdgeInsets.symmetric(horizontal: 28),
        decoration: const BoxDecoration(
            color: Colors.white,
            border: Border(bottom: BorderSide(color: border))),
        child: Row(children: [
          const Text('Water Monitor', style: TextStyle(color: muted)),
          const Padding(
              padding: EdgeInsets.symmetric(horizontal: 10), child: Text('/')),
          Text(section, style: const TextStyle(fontWeight: FontWeight.w500)),
          const Spacer(),
          const CircleAvatar(
              radius: 16,
              backgroundColor: Color(0xFFDCEAF5),
              child: Text('A', style: TextStyle(color: navy))),
          const SizedBox(width: 9),
          const Text('Admin'),
          const SizedBox(width: 10),
          const Icon(Icons.keyboard_arrow_down, color: muted),
        ]),
      );
}

class _BrandMark extends StatelessWidget {
  const _BrandMark();
  @override
  Widget build(BuildContext context) => Container(
      width: 38,
      height: 38,
      decoration: BoxDecoration(
          color: const Color(0xFF21A6B7),
          borderRadius: BorderRadius.circular(12)),
      child: const Icon(Icons.water_drop, color: Colors.white));
}

class _PageFrame extends StatelessWidget {
  const _PageFrame(
      {required this.title,
      required this.subtitle,
      required this.child,
      this.action});
  final String title, subtitle;
  final Widget child;
  final Widget? action;
  @override
  Widget build(BuildContext context) => LayoutBuilder(
      builder: (context, constraints) => SingleChildScrollView(
            padding: EdgeInsets.all(constraints.maxWidth < 700 ? 16 : 28),
            child: Center(
                child: ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 1350),
                    child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Expanded(
                                    child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                      Text(title,
                                          style: Theme.of(context)
                                              .textTheme
                                              .headlineSmall
                                              ?.copyWith(
                                                  fontWeight: FontWeight.w600,
                                                  color: navy)),
                                      const SizedBox(height: 5),
                                      Text(subtitle,
                                          style: const TextStyle(color: muted))
                                    ])),
                                if (action != null) action!
                              ]),
                          const SizedBox(height: 22),
                          child,
                        ]))),
          ));
}

class OverviewPage extends StatelessWidget {
  const OverviewPage({super.key});
  @override
  Widget build(BuildContext context) => _PageFrame(
        title: 'Tổng quan chất lượng nước',
        subtitle: 'Theo dõi chỉ số cảm biến theo thời gian thực',
        action: const _ConnectionBadge(),
        child: Column(children: [
          LayoutBuilder(
              builder: (context, c) => GridView.count(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      crossAxisCount: c.maxWidth < 650 ? 1 : 3,
                      childAspectRatio: c.maxWidth < 650 ? 2.5 : 1.9,
                      crossAxisSpacing: 14,
                      mainAxisSpacing: 14,
                      children: const [
                        _MetricCard(
                            title: 'Độ pH',
                            value: '7.2',
                            unit: 'pH',
                            detail: 'Ngưỡng tham khảo 6.5–8.5',
                            icon: Icons.water_drop_outlined),
                        _MetricCard(
                            title: 'Tổng chất rắn hòa tan',
                            value: '342',
                            unit: 'ppm',
                            detail: 'TDS · dữ liệu minh họa',
                            icon: Icons.bubble_chart_outlined),
                        _MetricCard(
                            title: 'Nhiệt độ nước',
                            value: '26.4',
                            unit: '°C',
                            detail: 'Cập nhật vừa xong',
                            icon: Icons.thermostat),
                      ])),
          const SizedBox(height: 14),
          LayoutBuilder(
            builder: (context, c) => c.maxWidth < 850
                ? const Column(
                    children: [
                      SizedBox(height: 340, child: _PhChart()),
                      SizedBox(height: 14),
                      SizedBox(height: 340, child: _DeviceSummary()),
                    ],
                  )
                : const SizedBox(
                    height: 340,
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(flex: 2, child: _PhChart()),
                        SizedBox(width: 14),
                        Expanded(child: _DeviceSummary()),
                      ],
                    ),
                  ),
          ),
        ]),
      );
}

class _ConnectionBadge extends StatelessWidget {
  const _ConnectionBadge();
  @override
  Widget build(BuildContext context) => const Chip(
      avatar: Icon(Icons.circle, size: 9, color: Color(0xFF188866)),
      label: Text('Bản xem thử'),
      backgroundColor: Color(0xFFEAF7F1),
      side: BorderSide.none);
}

class _MetricCard extends StatelessWidget {
  const _MetricCard(
      {required this.title,
      required this.value,
      required this.unit,
      required this.detail,
      required this.icon});
  final String title, value, unit, detail;
  final IconData icon;
  @override
  Widget build(BuildContext context) => Card(
      child: Padding(
          padding: const EdgeInsets.all(18),
          child:
              Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Expanded(
                  child: Text(title, style: const TextStyle(color: muted))),
              Icon(icon, color: blue)
            ]),
            const Spacer(),
            RichText(
                text: TextSpan(style: const TextStyle(color: navy), children: [
              TextSpan(
                  text: value,
                  style: const TextStyle(
                      fontSize: 30, fontWeight: FontWeight.w600)),
              TextSpan(
                  text: ' $unit',
                  style: const TextStyle(fontSize: 14, color: muted))
            ])),
            const SizedBox(height: 8),
            Text(detail, style: const TextStyle(fontSize: 12, color: muted))
          ])));
}

class _PhChart extends StatelessWidget {
  const _PhChart();
  @override
  Widget build(BuildContext context) => Card(
      child: Padding(
          padding: const EdgeInsets.all(18),
          child:
              Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            const Text('Biến thiên pH',
                style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            const Text('ESP32_01 · 24 giờ gần nhất · Dữ liệu minh họa',
                style: TextStyle(color: muted, fontSize: 12)),
            const SizedBox(height: 16),
            Expanded(
                child: CustomPaint(
                    painter: _ChartPainter(), child: const SizedBox.expand()))
          ])));
}

class _ChartPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final grid = Paint()
      ..color = border
      ..strokeWidth = 1;
    final line = Paint()
      ..color = blue
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round;
    final labels = TextPainter(textDirection: TextDirection.ltr);
    const vals = ['9.0', '8.0', '7.0', '6.0', '5.0'];
    for (var i = 0; i < 5; i++) {
      final y = i * (size.height - 24) / 4 + 4;
      canvas.drawLine(Offset(34, y), Offset(size.width, y), grid);
      labels.text = TextSpan(
          text: vals[i], style: const TextStyle(color: muted, fontSize: 11));
      labels.layout();
      labels.paint(canvas, Offset(0, y - 7));
    }
    final pts = [
      Offset(40, size.height * .62),
      Offset(size.width * .18, size.height * .48),
      Offset(size.width * .34, size.height * .54),
      Offset(size.width * .50, size.height * .34),
      Offset(size.width * .67, size.height * .49),
      Offset(size.width * .83, size.height * .43),
      Offset(size.width - 5, size.height * .25)
    ];
    final path = Path()..moveTo(pts.first.dx, pts.first.dy);
    for (var i = 1; i < pts.length; i++) {
      final p0 = pts[i - 1], p1 = pts[i];
      final mid = (p0.dx + p1.dx) / 2;
      path.cubicTo(mid, p0.dy, mid, p1.dy, p1.dx, p1.dy);
    }
    canvas.drawPath(path, line);
    for (final p in pts) {
      canvas.drawCircle(p, 3.5, Paint()..color = blue);
    }
    const times = ['08:00', '12:00', '16:00', '20:00'];
    for (var i = 0; i < times.length; i++) {
      labels.text = TextSpan(
          text: times[i], style: const TextStyle(color: muted, fontSize: 10));
      labels.layout();
      labels.paint(
          canvas, Offset(34 + i * (size.width - 55) / 3, size.height - 14));
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class _DeviceSummary extends StatelessWidget {
  const _DeviceSummary();
  @override
  Widget build(BuildContext context) => Card(
      child: Padding(
          padding: const EdgeInsets.all(18),
          child:
              Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            const Text('Thiết bị đang theo dõi',
                style: TextStyle(fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            const Text('Danh sách thiết bị mẫu',
                style: TextStyle(color: muted, fontSize: 12)),
            const SizedBox(height: 12),
            const _DeviceRow(name: 'ESP32_01', place: 'Khu vực bể A'),
            const Divider(height: 1),
            const _DeviceRow(name: 'ESP32_02', place: 'Khu vực bể B')
          ])));
}

class _DeviceRow extends StatelessWidget {
  const _DeviceRow({required this.name, required this.place});
  final String name, place;
  @override
  Widget build(BuildContext context) => Padding(
      padding: const EdgeInsets.symmetric(vertical: 14),
      child: Row(children: [
        const Icon(Icons.memory, color: blue),
        const SizedBox(width: 10),
        Expanded(
            child:
                Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(name, style: const TextStyle(fontWeight: FontWeight.w500)),
          Text(place, style: const TextStyle(color: muted, fontSize: 12))
        ])),
        const Chip(
            label: Text('Hoạt động'),
            labelStyle: TextStyle(color: Color(0xFF188866), fontSize: 11),
            backgroundColor: Color(0xFFEAF7F1),
            side: BorderSide.none,
            visualDensity: VisualDensity.compact)
      ]));
}

class HistoryPage extends StatefulWidget {
  const HistoryPage({super.key});
  @override
  State<HistoryPage> createState() => _HistoryPageState();
}

class _HistoryPageState extends State<HistoryPage> {
  String device = 'ESP32_01';
  DateTimeRange range =
      DateTimeRange(start: DateTime(2026, 10, 12), end: DateTime(2026, 10, 13));
  @override
  Widget build(BuildContext context) => _PageFrame(
      title: 'Lịch sử đo',
      subtitle: 'Lọc và xem lại dữ liệu theo thiết bị, khoảng thời gian',
      action: OutlinedButton.icon(
          onPressed: () {},
          icon: const Icon(Icons.download_outlined),
          label: const Text('Xuất báo cáo')),
      child: Column(children: [
        Card(
            child: Padding(
                padding: const EdgeInsets.all(16),
                child: Wrap(
                    spacing: 14,
                    runSpacing: 12,
                    crossAxisAlignment: WrapCrossAlignment.end,
                    children: [
                      SizedBox(
                          width: 220,
                          child: DropdownButtonFormField<String>(
                              initialValue: device,
                              decoration: const InputDecoration(
                                  labelText: 'Thiết bị',
                                  border: OutlineInputBorder()),
                              items: const [
                                DropdownMenuItem(
                                    value: 'ESP32_01',
                                    child: Text('ESP32_01 · Bể A')),
                                DropdownMenuItem(
                                    value: 'ESP32_02',
                                    child: Text('ESP32_02 · Bể B'))
                              ],
                              onChanged: (v) =>
                                  setState(() => device = v ?? device))),
                      OutlinedButton.icon(
                          onPressed: () async {
                            final r = await showDateRangePicker(
                                context: context,
                                firstDate: DateTime(2020),
                                lastDate: DateTime(2035),
                                initialDateRange: range);
                            if (r != null) setState(() => range = r);
                          },
                          icon: const Icon(Icons.date_range),
                          label: Text(
                              '${_date(range.start)} – ${_date(range.end)}')),
                      FilledButton.icon(
                          onPressed: () {},
                          icon: const Icon(Icons.filter_alt_outlined),
                          label: const Text('Lọc dữ liệu'))
                    ]))),
        const SizedBox(height: 14),
        Card(
            child: Padding(
                padding: const EdgeInsets.all(18),
                child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Chỉ số pH theo thời gian',
                          style: TextStyle(fontWeight: FontWeight.w600)),
                      const SizedBox(height: 4),
                      Text('$device · Dữ liệu minh họa',
                          style: const TextStyle(color: muted, fontSize: 12)),
                      const SizedBox(height: 20),
                      SizedBox(
                          height: 320,
                          child: CustomPaint(
                              painter: _ChartPainter(),
                              child: const SizedBox.expand()))
                    ])))
      ]));
}

String _date(DateTime d) =>
    '${d.day.toString().padLeft(2, '0')}/${d.month.toString().padLeft(2, '0')}/${d.year}';

class DevicesPage extends StatefulWidget {
  const DevicesPage({super.key});
  @override
  State<DevicesPage> createState() => _DevicesPageState();
}

class _DevicesPageState extends State<DevicesPage> {
  // Keep the preview data in one small model. A backend can replace this list later.
  final devices = <_WaterDevice>[
    _WaterDevice(
        name: 'ESP32_01', id: 'AA:BB:CC:01:23:45', area: 'Khu vực bể A'),
    _WaterDevice(
        name: 'ESP32_02', id: 'AA:BB:CC:01:23:46', area: 'Khu vực bể B'),
  ];

  Future<void> addDevice() async {
    final formKey = GlobalKey<FormState>();
    final name = TextEditingController();
    final id = TextEditingController();
    final area = TextEditingController();
    final added = await showDialog<_WaterDevice>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Thêm thiết bị'),
        content: SizedBox(
          width: 420,
          child: Form(
            key: formKey,
            child: Column(mainAxisSize: MainAxisSize.min, children: [
              TextFormField(
                  controller: name,
                  decoration:
                      const InputDecoration(labelText: 'Tên thiết bị *'),
                  validator: _required),
              TextFormField(
                  controller: id,
                  decoration: const InputDecoration(
                      labelText: 'Mã thiết bị hoặc MAC *'),
                  validator: (value) {
                    final error = _required(value);
                    if (error != null) return error;
                    if (devices.any((device) =>
                        device.id.toLowerCase() == value!.trim().toLowerCase()))
                      return 'Mã thiết bị đã tồn tại';
                    return null;
                  }),
              TextFormField(
                  controller: area,
                  decoration: const InputDecoration(labelText: 'Khu vực')),
              const SizedBox(height: 10),
              const Text(
                  'Thiết bị sẽ được lưu trong phiên xem thử. Kết nối MQTT sẽ được bổ sung cùng backend.',
                  style: TextStyle(color: muted, fontSize: 12)),
            ]),
          ),
        ),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(dialogContext),
              child: const Text('Hủy')),
          FilledButton(
              onPressed: () {
                if (formKey.currentState!.validate()) {
                  Navigator.pop(
                      dialogContext,
                      _WaterDevice(
                          name: name.text.trim(),
                          id: id.text.trim(),
                          area: area.text.trim().isEmpty
                              ? 'Chưa phân khu'
                              : area.text.trim()));
                }
              },
              child: const Text('Thêm thiết bị')),
        ],
      ),
    );
    name.dispose();
    id.dispose();
    area.dispose();
    if (added != null && mounted) setState(() => devices.add(added));
  }

  // The MQTT publish is intentionally a UI preview until the broker/backend exists.
  Future<void> calibrate(_WaterDevice device) async {
    final phOffset = TextEditingController(text: '0.00');
    final tdsFactor = TextEditingController(text: '1.00');
    final tempOffset = TextEditingController(text: '0.0');
    final formKey = GlobalKey<FormState>();
    final send = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Hiệu chỉnh cảm biến'),
        content: SizedBox(
            width: 440,
            child: Form(
                key: formKey,
                child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('${device.name} · ${device.area}',
                          style: const TextStyle(color: muted)),
                      const SizedBox(height: 16),
                      TextFormField(
                          controller: phOffset,
                          keyboardType: const TextInputType.numberWithOptions(
                              decimal: true, signed: true),
                          decoration: const InputDecoration(
                              labelText: 'Độ lệch pH',
                              suffixText: 'pH',
                              helperText: 'Cộng vào số đo pH thô'),
                          validator: _number),
                      TextFormField(
                          controller: tdsFactor,
                          keyboardType: const TextInputType.numberWithOptions(
                              decimal: true),
                          decoration: const InputDecoration(
                              labelText: 'Hệ số TDS',
                              suffixText: '×',
                              helperText: 'Nhân với số đo TDS thô'),
                          validator: _positiveNumber),
                      TextFormField(
                          controller: tempOffset,
                          keyboardType: const TextInputType.numberWithOptions(
                              decimal: true, signed: true),
                          decoration: const InputDecoration(
                              labelText: 'Độ lệch nhiệt độ', suffixText: '°C'),
                          validator: _number),
                      const SizedBox(height: 12),
                      const Text(
                          'Bản xem thử chưa gửi lệnh đến thiết bị. Khi có backend, các giá trị này sẽ được đóng gói và gửi qua MQTT.',
                          style: TextStyle(color: muted, fontSize: 12)),
                    ]))),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(dialogContext, false),
              child: const Text('Hủy')),
          FilledButton.icon(
              onPressed: () {
                if (formKey.currentState!.validate())
                  Navigator.pop(dialogContext, true);
              },
              icon: const Icon(Icons.send_outlined),
              label: const Text('Gửi thông số'))
        ],
      ),
    );
    phOffset.dispose();
    tdsFactor.dispose();
    tempOffset.dispose();
    if (send == true && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content:
              Text('Đã kiểm tra thông số. Chưa gửi MQTT trong bản xem thử.')));
    }
  }

  String? _required(String? value) => value == null || value.trim().isEmpty
      ? 'Vui lòng nhập thông tin này'
      : null;
  String? _number(String? value) =>
      double.tryParse((value ?? '').trim().replaceAll(',', '.')) == null
          ? 'Nhập một số hợp lệ'
          : null;
  String? _positiveNumber(String? value) {
    final number = double.tryParse((value ?? '').trim().replaceAll(',', '.'));
    return number == null || number <= 0 ? 'Hệ số phải lớn hơn 0' : null;
  }

  @override
  Widget build(BuildContext context) => _PageFrame(
        title: 'Quản lý thiết bị',
        subtitle:
            'Đăng ký thiết bị và cấu hình hiệu chỉnh cảm biến trong mạng nội bộ',
        action: FilledButton.icon(
            onPressed: addDevice,
            icon: const Icon(Icons.add),
            label: const Text('Thêm thiết bị')),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Card(
              color: const Color(0xFFEAF2FA),
              child: const Padding(
                  padding: EdgeInsets.all(16),
                  child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Icon(Icons.info_outline, color: blue),
                        SizedBox(width: 12),
                        Expanded(
                            child: Text(
                                'Ứng dụng desktop dùng trong mạng nội bộ. Dữ liệu hiện là bản xem thử; thao tác gửi thông số sẽ kết nối MQTT khi backend được tích hợp.',
                                style: TextStyle(color: navy)))
                      ]))),
          const SizedBox(height: 14),
          Card(
              child: SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: DataTable(
                    columns: const [
                      DataColumn(label: Text('Tên thiết bị')),
                      DataColumn(label: Text('Địa chỉ MAC / ID')),
                      DataColumn(label: Text('Khu vực')),
                      DataColumn(label: Text('Trạng thái')),
                      DataColumn(label: Text('Thao tác'))
                    ],
                    rows: devices
                        .map((device) => DataRow(cells: [
                              DataCell(Text(device.name)),
                              DataCell(Text(device.id)),
                              DataCell(Text(device.area)),
                              const DataCell(Chip(
                                  label: Text('Đã đăng ký'),
                                  backgroundColor: Color(0xFFEAF7F1),
                                  side: BorderSide.none)),
                              DataCell(OutlinedButton.icon(
                                  onPressed: () => calibrate(device),
                                  icon: const Icon(Icons.tune, size: 18),
                                  label: const Text('Hiệu chỉnh'))),
                            ]))
                        .toList(),
                  ))),
        ]),
      );
}

class _WaterDevice {
  const _WaterDevice(
      {required this.name, required this.id, required this.area});
  final String name;
  final String id;
  final String area;
}

class SettingsPage extends StatelessWidget {
  const SettingsPage({super.key});
  @override
  Widget build(BuildContext context) {
    return _PageFrame(
      title: 'Cài đặt',
      subtitle: 'Thông tin hiển thị trong bản thiết kế',
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: SizedBox(
            width: 650,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Thông tin tài khoản',
                  style: TextStyle(fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 16),
                const TextField(
                  decoration: InputDecoration(
                    labelText: 'Tên hiển thị',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                const TextField(
                  decoration: InputDecoration(
                    labelText: 'Email',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 16),
                FilledButton(
                  onPressed: null,
                  child: const Text('Lưu giao diện'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
