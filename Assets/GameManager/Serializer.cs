using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class Serializer
{
    public static string SavePath = "route.save";

    public static Route ReadData(Route newRoute)
    {
        if (File.Exists(SavePath)) using (Stream stream = File.Open(SavePath, FileMode.Open))
            {
                newRoute = (Route)new BinaryFormatter().Deserialize(stream);
                return (newRoute);
            }
        return (null);
    }
    public static void SaveData(Route saveRoute)
    {
        FileStream fs = new FileStream(SavePath, FileMode.Create);
        new BinaryFormatter().Serialize(fs, saveRoute);
        fs.Close();
    }
    public static void ClearData(Route clearRoute)
    {
        File.Delete(SavePath);
    }
}