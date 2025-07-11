using UnityEngine;

public class WinController : MonoBehaviour
{
    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            UIController.Instance.ShowWinPanel();
        }
    }
}
