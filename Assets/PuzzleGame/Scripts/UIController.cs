using TMPro;
using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private TextMeshProUGUI _blockTypeText;
    [SerializeField] private TextMeshProUGUI _attacksCountText;

    private string _prefix = "Block type: ";
    private Coroutine _animCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void SetAttacksText(int attacksCount)
    {
        _attacksCountText.text = $"Smashes: {attacksCount.ToString()}";
    }
    
    public void SetBlockText()
    {
        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(AnimateRemoveText());
    }

    public void SetBlockText(BlockType blockType)
    {
        string textAfterPrefix = blockType.ToString();

        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(AnimateAddText(textAfterPrefix));
    }

    private IEnumerator AnimateAddText(string text)
    {
        _blockTypeText.text = _prefix;

        yield return null;

        for (int i = 0; i < text.Length; i++)
        {
            _blockTypeText.text += text[i];

            _blockTypeText.ForceMeshUpdate();

            yield return new WaitForSeconds(0.03f);
        }

        _animCoroutine = null;
    }

    private IEnumerator AnimateRemoveText()
    {
        // Если текста нет или он равен только префиксу — просто выходим
        if (_blockTypeText.text == _prefix)
        {
            _animCoroutine = null;
            yield break;
        }

        // Убедимся, что текст начинается с префикса
        if (!_blockTypeText.text.StartsWith(_prefix))
        {
            _blockTypeText.text = _prefix;
            _animCoroutine = null;
            yield break;
        }

        // Удаляем буквы по одной с конца, оставляя префикс
        while (_blockTypeText.text.Length > _prefix.Length)
        {
            _blockTypeText.text = _blockTypeText.text.Substring(0, _blockTypeText.text.Length - 1);
            _blockTypeText.ForceMeshUpdate();
            yield return new WaitForSeconds(0.03f);
        }

        _animCoroutine = null;
    }
}
