using TMPro;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using PuzzleGame.Scripts;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private RectTransform _ladderButton;
    
    [SerializeField] private TextMeshProUGUI _blockTypeText;
    [SerializeField] private TextMeshProUGUI _attacksCountText;

    [Header("Win Panel Elements")]
    [SerializeField] private GameObject winPanelParent;
    [SerializeField] private Image pickaxeImage;
    [SerializeField] private Image[] stars;
    [SerializeField] private RectTransform winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private Button winButton;

    [Header("Loose Panel Elements")]
    [SerializeField] private GameObject loosePanelParent;
    [SerializeField] private RectTransform loosePanel;
    [SerializeField] private TextMeshProUGUI looseText;
    [SerializeField] private Button looseButton;
    
    [Header("Panel Elements")]
    [SerializeField] private GameObject parentPanel;
    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;

    [Header("Sounds")]
    [SerializeField] private AudioClip blockTextCharSound;
    [SerializeField] private AudioClip panelSlideSound;
    [SerializeField] private AudioClip panelTextCharSound;
    [SerializeField] private AudioClip button1AppearSound;
    [SerializeField] private AudioClip button2AppearSound;

    [SerializeField] private RectTransform _blockTypeBackground;
    
    private string _prefix = "Block type: ";
    private Coroutine _animCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void SetAttacksText(int attacksCount)
    {
        _attacksCountText.text = $"{attacksCount.ToString()}";
    }

    private void UpdateBlockTypeBackgroundWidth()
    {
        float textWidth = _blockTypeText.preferredWidth;
        float padding = textWidth * 0.1f; // 10% запаса

        _blockTypeBackground.sizeDelta = new Vector2(textWidth + padding, _blockTypeBackground.sizeDelta.y);
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

    public void ShowPanel()
    {
        parentPanel.SetActive(true);
        panel.anchoredPosition = new Vector2(Screen.width, 0);

        MusicController.Instance.PlaySpecificSound(panelSlideSound);

        panel.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            StartCoroutine(AnimatePanelContent());
        });

        titleText.text = "";
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
    }

    public void ShowLadderButton()
    {
        _ladderButton.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutBack);
    }

    public void HideLadderButton()
    {
        _ladderButton.DOAnchorPosY(-400f, 0.3f).SetEase(Ease.InBack);
    }
    
    public void ShowWinPanel()
    {
        // Сброс начальных состояний
        pickaxeImage.gameObject.SetActive(false);
        foreach (var star in stars)
            star.gameObject.SetActive(false);

        ShowGenericPanel(
            winPanelParent, 
            winPanel, 
            winText, 
            "YOU WIN", 
            winButton
        );
    }

    public void ShowLoosePanel()
    {
        ShowGenericPanel(
            loosePanelParent, 
            loosePanel, 
            looseText, 
            "You Lost", 
            looseButton
        );
    }
    
    private void ShowGenericPanel(GameObject panelParent, RectTransform panelRect, TextMeshProUGUI textElement, string message, Button button)
    {
        panelParent.SetActive(true);
        panelRect.anchoredPosition = new Vector2(Screen.width, 0);

        MusicController.Instance.PlaySpecificSound(panelSlideSound);

        panelRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            StartCoroutine(AnimateGenericPanelContent(textElement, message, button));
        });

        textElement.text = "";
        button.gameObject.SetActive(false);
    }
    
    private IEnumerator AnimateGenericPanelContent(TextMeshProUGUI textElement, string fullText, Button button)
    {
        // 1. Показываем кирку
        pickaxeImage.gameObject.SetActive(true);
        pickaxeImage.transform.localScale = Vector3.zero;
        pickaxeImage.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.5f);

        // 2. Анимация текста
        for (int i = 0; i < fullText.Length; i++)
        {
            textElement.text += fullText[i];
            textElement.ForceMeshUpdate();

            if (panelTextCharSound != null)
                MusicController.Instance.PlaySpecificSound(panelTextCharSound);

            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.3f);

        // 3. Показываем звезды по одной
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(true);
            stars[i].transform.localScale = Vector3.zero;
            stars[i].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            // Можно воспроизводить звук появления звезды тут, если есть
            if (button1AppearSound != null)
                MusicController.Instance.PlaySpecificSound(button1AppearSound);

            yield return new WaitForSeconds(0.3f);
        }

        // 4. Показываем кнопку
        yield return new WaitForSeconds(0.2f);

        button.gameObject.SetActive(true);
        button.transform.localScale = Vector3.zero;
        button.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        if (button1AppearSound != null)
            MusicController.Instance.PlaySpecificSound(button1AppearSound);
    }

    private IEnumerator AnimatePanelContent()
    {
        string fullText = "No Smashes Left";

        for (int i = 0; i < fullText.Length; i++)
        {
            titleText.text += fullText[i];
            titleText.ForceMeshUpdate();

            if (panelTextCharSound != null)
                MusicController.Instance.PlaySpecificSound(panelTextCharSound);

            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.2f);

        button1.gameObject.SetActive(true);
        button1.transform.localScale = Vector3.zero;
        button1.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        if (button1AppearSound != null)
            MusicController.Instance.PlaySpecificSound(button1AppearSound);

        yield return new WaitForSeconds(0.2f);

        button2.gameObject.SetActive(true);
        button2.transform.localScale = Vector3.zero;
        button2.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        if (button2AppearSound != null)
            MusicController.Instance.PlaySpecificSound(button2AppearSound);
    }

    private IEnumerator AnimateAddText(string text)
    {
        _blockTypeText.text = _prefix;

        yield return null;

        for (int i = 0; i < text.Length; i++)
        {
            _blockTypeText.text += text[i];
            _blockTypeText.ForceMeshUpdate();

            UpdateBlockTypeBackgroundWidth();
            
            if (blockTextCharSound != null)
                MusicController.Instance.PlaySpecificSound(blockTextCharSound);

            yield return new WaitForSeconds(0.03f);
        }

        _animCoroutine = null;
    }

    private IEnumerator AnimateRemoveText()
    {
        if (_blockTypeText.text == _prefix)
        {
            _animCoroutine = null;
            yield break;
        }

        if (!_blockTypeText.text.StartsWith(_prefix))
        {
            _blockTypeText.text = _prefix;
            _animCoroutine = null;
            yield break;
        }

        while (_blockTypeText.text.Length > _prefix.Length)
        {
            _blockTypeText.text = _blockTypeText.text.Substring(0, _blockTypeText.text.Length - 1);
            _blockTypeText.ForceMeshUpdate();
            
            UpdateBlockTypeBackgroundWidth();
            
            yield return new WaitForSeconds(0.03f);
        }

        _animCoroutine = null;
    }
}
