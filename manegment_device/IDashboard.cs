namespace manegment_device;

public interface IDashboard
{

    List<Device> GetAllDevices();
    Information_device? GetLatestReading(string maMay);
    List<Information_device> GetHistory(string maMay, int take = 25);
}
