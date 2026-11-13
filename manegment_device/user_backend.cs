using System;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace manegment_device;

public class user_backend : Sign_query
{

    private string Create_password = "";
    private string Create_user = "";
    public string Role { get; private set; } = "member";
    public string Create_User { 
        get { return Create_user; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Tên đăng nhập không được để trống.");
            }
            Create_user = value.Trim();
        }
    }
    
    public string Create_Password
    {
        get { return Create_password; }
        set
        {
            // Mật khẩu không trống, ít nhất 5 kí tự , có ít nhất 1  kí tự đặc biệt
            if (!string.IsNullOrEmpty(value) && value.Length > 5 && Regex.IsMatch(value, @"[^\w]"))
            {
                Create_password = Hash.HashPassword(value);
            }
            else
            {
                throw new ArgumentException("Mật khẩu phải chứa trên 5 kí tự và có ít nhất 1 kí tự đặc biệt.");

            }
        }
    }
}
