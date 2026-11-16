using UnityEngine;
public class PlayerPrefsSave : ISaveService
{
    public int GetInt(string key, int fallback = 0) => PlayerPrefs.GetInt(key, fallback);
    public void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();                // write now: the phone may close us any time
    }
}
