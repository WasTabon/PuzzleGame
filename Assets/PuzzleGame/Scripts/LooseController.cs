using UnityEngine;

public class LooseController : MonoBehaviour
{
    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            UIController.Instance.ShowLoosePanel();
        }
    }
}
