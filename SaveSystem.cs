using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public static class SaveSystem 
{
    public static void SaveObject(object Object, string dataPath)
    {
        var formatter = new BinaryFormatter();
        var path = UnityEngine.Application.persistentDataPath;
        var stream = new FileStream(path + dataPath, FileMode.Create);
        formatter.Serialize(stream, Object);
        stream.Close();
    }

    public static T LoadObject<T>(string dataPath) 
        //where T : class
    {
        var path = UnityEngine.Application.persistentDataPath + dataPath;
        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Open);
            var Object = (T)formatter.Deserialize(stream);
            stream.Close();
            return Object;
        }
        else
        {
            //return null;
            return default(T);
        }
    }
}
