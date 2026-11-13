using MySql.Data.MySqlClient;

namespace manegment_device;

public class Dashboard_query : business_query_device, IDashboard
{
    private readonly string constring;

    public Dashboard_query() : this(WaterMonitorSettings.Load()) { }

    public Dashboard_query(WaterMonitorSettings settings) : base(settings)
    {
        constring = settings.DatabaseConnectionString;
    }

    public List<Device> GetAllDevices()
    {
        var devices = new List<Device>();

        using var connection = new MySqlConnection(constring);
        connection.Open();
        const string query = @"SELECT t.ma_may, t.ho_nuoi_id, t.current_ip, t.trang_thai, h.ten_ho
                               FROM thiet_bi t JOIN ho_nuoi h ON h.id=t.ho_nuoi_id ORDER BY t.ma_may";

        using var cmd = new MySqlCommand(query, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            devices.Add(new Device(
                maMay: reader.GetString(0),
                hoNuoiId: reader.GetInt32(1),
                currentIp: reader.IsDBNull(2) ? "0.0.0.0" : reader.GetString(2),
                trangThai: reader.GetBoolean(3),
                hoNuoiName: reader.IsDBNull(4) ? "" : reader.GetString(4)));
        }

        return devices;
    }

    public Information_device? GetLatestReading(string maMay)
    {
        using var connection = new MySqlConnection(constring);
        connection.Open();
        const string query = @"SELECT id, nong_do_ph, tds, nhiet_do, thoi_gian_do
                                FROM thong_so
                                WHERE ma_may = @MaMay
                                ORDER BY id DESC
                                LIMIT 1";

        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@MaMay", maMay);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapReading(reader) : null;
    }

    public List<Information_device> GetHistory(string maMay, int take = 25)
    {
        var readings = new List<Information_device>();

        using var connection = new MySqlConnection(constring);
        connection.Open();
        const string query = @"SELECT id, nong_do_ph, tds, nhiet_do, thoi_gian_do
                                FROM thong_so
                                WHERE ma_may = @MaMay
                                ORDER BY id DESC
                                LIMIT @Take";

        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@MaMay", maMay);
        cmd.Parameters.AddWithValue("@Take", take);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            readings.Add(MapReading(reader));
        }

        readings.Reverse(); // trả về cũ -> mới, tiện cho việc vẽ biểu đồ
        return readings;
    }

    private static Information_device MapReading(MySqlDataReader reader)
    {
        // Cột thoi_gian_do trong DB là TIMESTAMP (ngày + giờ), nhưng
        // Information_device.ThoiGian đang là TimeSpan nên chỉ giữ được
        // phần giờ:phút:giây - phần ngày sẽ bị mất khi dùng TimeOfDay.
        // Nếu cần chính xác theo ngày (vd. lịch sử nhiều ngày), nên đổi
        // ThoiGian sang DateTime.
        var timestamp = reader.GetDateTime(4);

        return new Information_device(
            id: reader.GetInt32(0),
            nong_do_pH: (float)reader.GetDecimal(1),
            tds: reader.GetInt32(2),
            nhiet_do: (float)reader.GetDecimal(3),
            thoigian: timestamp);
    }

    public List<LakeOption> GetLakes()
    {
        var lakes = new List<LakeOption>();
        using var connection = new MySqlConnection(constring);
        connection.Open();
        using var cmd = new MySqlCommand("SELECT id, ten_ho FROM ho_nuoi ORDER BY ten_ho", connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) lakes.Add(new LakeOption(reader.GetInt32(0), reader.GetString(1)));
        return lakes;
    }

    public List<Information_device> GetHistory(string maMay, DateTime from, DateTime to, int take = 1000)
    {
        var readings = new List<Information_device>();
        using var connection = new MySqlConnection(constring);
        connection.Open();
        const string query = @"SELECT id, nong_do_ph, tds, nhiet_do, thoi_gian_do
                               FROM thong_so
                               WHERE ma_may = @MaMay AND thoi_gian_do >= @From AND thoi_gian_do < @To
                               ORDER BY thoi_gian_do DESC LIMIT @Take";
        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@MaMay", maMay);
        cmd.Parameters.AddWithValue("@From", from);
        cmd.Parameters.AddWithValue("@To", to);
        cmd.Parameters.AddWithValue("@Take", take);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) readings.Add(MapReading(reader));
        readings.Reverse();
        return readings;
    }

    public void InsertTelemetry(string maMay, float ph, float tds, float temperature)
    {
        using var connection = new MySqlConnection(constring);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using (var exists = new MySqlCommand("SELECT COUNT(*) FROM thiet_bi WHERE ma_may=@id", connection, transaction))
            {
                exists.Parameters.AddWithValue("@id", maMay);
                if (Convert.ToInt32(exists.ExecuteScalar()) == 0)
                    throw new InvalidOperationException($"Thiết bị {maMay} chưa được đăng ký trong hệ thống.");
            }
            const string sql = "INSERT INTO thong_so (ma_may, nong_do_ph, tds, nhiet_do, thoi_gian_do) VALUES (@id,@ph,@tds,@temp,CURRENT_TIMESTAMP)";
            using (var cmd = new MySqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@id", maMay);
                cmd.Parameters.AddWithValue("@ph", ph);
                cmd.Parameters.AddWithValue("@tds", (int)Math.Round(tds));
                cmd.Parameters.AddWithValue("@temp", temperature);
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public bool UpdateDevice(string maMay, int lakeId, string ip, bool enabled) =>
        update_device(maMay, lakeId, ip, enabled);

    public bool AddDevice(string maMay, int lakeId, string ip) =>
        Add_Device_To_Existing_Lake(maMay, lakeId, ip);
}

public sealed record LakeOption(int Id, string Name)
{
    public override string ToString() => Name;
}
