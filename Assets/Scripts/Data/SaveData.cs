
[System.Serializable]
public class SaveData
{
    public int Highscore;
    public bool SoundOn;

    public SaveData(CoreData data)
    {
        this.Highscore = data.Highscore;
        this.SoundOn = data.SoundOn;
    }
}
