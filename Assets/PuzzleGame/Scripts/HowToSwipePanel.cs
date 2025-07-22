using System;
using UnityEngine;

public class HowToSwipePanel : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject[] _panels;

    private int _index;

    private void Start()
    {
        string key = PlayerPrefs.GetString("howToPanel", "123");

        if (key == "123")
        {
            _panel.SetActive(true);
        }
    }

    private void Update()
    {
        if (_index == 4)
        {
            _index = 10;
            _panel.SetActive(false);
        }
    }

    public void OpenNextPanel()
    {
        _panels[_index].SetActive(false);
        _index++;
        _panels[_index].SetActive(true);
        
        PlayerPrefs.SetString("howToPanel", "abc");
    }
}
