using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop_slot : MonoBehaviour
{
    [SerializeField] private GameObject _bought_gray;
    [SerializeField] private Button buy_button;
    [SerializeField] private GameObject sold_out;
    [SerializeField] private Image skin;
    [SerializeField] private TMP_Text Skin_name;
    [SerializeField] private TMP_Text price_text;

    [Header("Item_info")]
    [SerializeField] private Shop_item shp_it;

    private void Start()
    {
        if (PlayerPrefs.GetInt(shp_it.tagItemID) == 1)
        {
            buy_button.interactable = false;
            _bought_gray.SetActive(true);
            sold_out.SetActive(true);
            Skin_name.text = shp_it.skin_name;
            price_text.text = "Owned";
            skin.sprite = shp_it.shop_skin_img;
        }
        else
        {
            buy_button.interactable = true;
            _bought_gray.SetActive(false);
            sold_out.SetActive(false);
            Skin_name.text = shp_it.skin_name;
            price_text.text = "€ " + shp_it.price;
            skin.sprite = shp_it.shop_skin_img;
        }
    }
   
    public void BuyItem(string tagItem)
    {
        PlayerPrefs.SetInt(tagItem, 1);
        buy_button.interactable = false;
        _bought_gray.SetActive(true);
        sold_out.SetActive(true);
        price_text.text = "Owned";
        skin.sprite = shp_it.shop_skin_img;
    }
}