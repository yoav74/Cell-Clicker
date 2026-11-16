using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] TMP_Text label;         // drag your coin text here

    void Start() => Show();

    public void Add(int amount)              // a button: On Click () → CoinCounter.Add, 10
    {
        Services.Save.SetInt("coins", Services.Save.GetInt("coins") + amount);
        Show();
    }

    //void Show() => label.text = Services.Save.GetInt("coins").ToString();

    void Show() => label.text = "Coins: " + Services.Save.GetInt("coins").ToString();
}
