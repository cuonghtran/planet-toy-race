using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveLoadSystem : MonoBehaviour
{
    // Save data from scriptable object CoreData to disk
    public static void SaveGame(CoreData player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "PlanetToyRace.dat");
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(player);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    // Load data from disk to scriptable object CoreData
    public static void LoadGame(CoreData player)
    {
        string path = Path.Combine(Application.persistentDataPath, "PlanetToyRace.dat");

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;

            player.ChangeFromSaveToCoreData(data);
            stream.Close();
        }
        else
        {
            Debug.Log("Save file not found in " + path);
            throw new FileNotFoundException("Save file not found in " + path);
        }
    }
}
