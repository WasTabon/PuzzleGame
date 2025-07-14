using PuzzleGame.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class BuyController : MonoBehaviour
{
    private string _donateId = "com.fanstycoon.inappa1";
    
    public GameObject loadingButton;
    public AudioClip buySound;
    public TextMeshProUGUI buttonText;
    public GameObject panel;
    public GameObject buyPanel;
    
    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == _donateId)
        {
            Debug.Log("Complete");
            SwipeController.Instance._attackCount += 50;
            UIController.Instance.SetAttacksText(SwipeController.Instance._attackCount);
            MusicController.Instance.PlaySpecificSound(buySound);
            loadingButton.SetActive(false);
            panel.SetActive(true);
        }
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        if (product.definition.id == _donateId)
        {
            loadingButton.SetActive(false);
            Debug.Log($"Failed: {description.message}");
        }
    }
    
    public void OnProductFetched(Product product)
    {
        Debug.Log("Fetched");
        buttonText.text = product.metadata.localizedPriceString;
    }
}