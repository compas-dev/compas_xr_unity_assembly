using System.Collections.Generic;

public class User
{
    public Dictionary<string, Device> devices;

    public User()
    {
        devices = new Dictionary<string, Device>();
    }
}

[System.Serializable]
public class Device
{
    public List<string> dates;

    public Device()
    {
        dates = new List<string>();
    }
}