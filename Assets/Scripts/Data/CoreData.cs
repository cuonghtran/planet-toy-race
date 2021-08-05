
using UnityEngine;

[CreateAssetMenu(fileName = "CoreData", menuName = "Core Data")]
public class CoreData : ScriptableObject
{
    public int Highscore;
    public bool SoundOn = true;

    public void ChangeFromSaveToCoreData(SaveData data)
    {
        Highscore = data.Highscore;
        SoundOn = data.SoundOn;
    }
}
