using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Shop", menuName = "Scriptable Objects/Shop_item")]
public class Shop_item : ScriptableObject
{   
    
    [SerializeField] public string tagItemID;
    [SerializeField] public Sprite shop_skin_img;
    [SerializeField] public string skin_name;
    [SerializeField] public float price;

}
