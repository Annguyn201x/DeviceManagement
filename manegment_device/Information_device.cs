namespace manegment_device;

public class Information_device
{
    private int ID;
    private float nong_do_pH;
    private float tds;
    private float nhiet_do;
    private DateTime thoigian;

    public int id
    {
        get
        {
            return ID;
        }
        set
        {
            ID = value;
        }
    }
    public float  Nong_do_pH { get; set; }
    public float Tds { get; set; }
    public float Nhiet_do { get; set; }
    public DateTime ThoiGian { get; set; }

    public Information_device()
    {
        
    }

    public Information_device(int id, float nong_do_pH, float tds, float nhiet_do, DateTime thoigian)
    {
        this.ID = id;
        this.Nong_do_pH = nong_do_pH;
        this.Tds = tds;
        this.Nhiet_do = nhiet_do;
        this.ThoiGian = thoigian;
    }
    
}
