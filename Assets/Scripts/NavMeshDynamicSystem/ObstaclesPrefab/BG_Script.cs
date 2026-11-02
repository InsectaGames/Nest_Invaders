using UnityEngine;

public class BG_Script : MonoBehaviour, IObserver<Color>
{

    SpriteRenderer spr;



    private void Awake()
    {
        spr = this.GetComponent<SpriteRenderer>();
        GameObject.FindGameObjectWithTag("DNSys").GetComponent<DayNightSystem>().AddObserver(this);
    }

    public void UpdateObserver(Color data)
    {
        spr.color = data;
    }
}
