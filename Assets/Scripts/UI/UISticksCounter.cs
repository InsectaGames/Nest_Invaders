using UnityEngine;
using TMPro;

public class UISticksCounter : MonoBehaviour, IObserver<int>
{
    private TextMeshProUGUI text;
    BaseSystem bs;


    public void UpdateObserver(int data)
    {
        text.text = "Sticks: " + data.ToString();
    }

    private void Start()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Base"))
        {
            bs = obj.GetComponent<BaseSystem>();
            if (bs.GetFaction() == Faction.ALLY)
            {
              bs.AddObserver(this);
            }
        }
    }
}