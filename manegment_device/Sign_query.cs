using MySql.Data.MySqlClient;

namespace manegment_device;

public  class Sign_query
{
    private static string ConnectionString
    {
        get
        {
            var builder = new MySqlConnectionStringBuilder(WaterMonitorSettings.Load().DatabaseConnectionString)
            {
                ConnectionTimeout = 5
            };
            return builder.ConnectionString;
        }
    }
    public static bool checking_user_sign(string value)
    {
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(1) FROM user WHERE user_name = @user";
        
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@user", value);
                
                int count = Convert.ToInt32(cmd.ExecuteScalar());
            
                if (count > 0)
                {
                    return true; 
                }
                else
                {
                    return false; 
                }
            }
        }
    }
    public static bool Insert_user(string user_name, string password,string Role)
    {
        using (MySqlConnection conn = new MySqlConnection(ConnectionString))
        {
            conn.Open();
            string query = "INSERT INTO user (user_name, password,Role) VALUES (@user, @pass,@Role)";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@user", user_name);
                cmd.Parameters.AddWithValue("@pass", password);
                cmd.Parameters.AddWithValue("@Role",Role );

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}
