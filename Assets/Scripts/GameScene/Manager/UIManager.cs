using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // UI
    [SerializeField] RectTransform RedReady;
    [SerializeField] RectTransform BlueReady;
    // 숨김, 표지 상태 RectTransform PosX 좌표 
    [SerializeField] float RRtargetPosX = 150f;
    [SerializeField] float RRhiddenPosX = -150f;
    [SerializeField] float BRtargetPosX = -150f;
    [SerializeField] float BRhiddenPosX = 150f;
    [SerializeField] float Animduration = 0.5f;
    // 레디, 비레디 ui위치 값 
    [SerializeField] float UnReadyOffset = 1f;
    [SerializeField] float ReadyOffset = 0.5f;

    [SerializeField] GameObject MenuPanel;
    [SerializeField] GameObject FirstAttackPanel;
    [SerializeField] GameObject RedFirstPanel;
    [SerializeField] GameObject BlueFirstPanel;

    [SerializeField] GameObject RedHitCountPanel;
    [SerializeField] GameObject BlueHitCountPanel;
    [SerializeField] TextMeshProUGUI RedHitCountText;
    [SerializeField] TextMeshProUGUI BlueHitCountText;

    [SerializeField] GameObject ItemPanel;

    // bool 
    private bool isMenuPanelVisibled = false;
    private bool isAvoidPanelVisibled = false;
    private bool isRedUIShown = false;
    private bool isBlueUIShown = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Start()
    {
        // 메뉴 비활성화 
        MenuPanel.gameObject.SetActive(false);
        // 회피 패널 비활성화
        RedHitCountPanel.SetActive(false);
        BlueHitCountPanel.SetActive(false);
        // 아이템 패널 비활성화
        ItemPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.currentDirection != GameManager.GameDirection.GameEnd)
        {
            // 게임 종료 상태 제외 토글 불러오기 가능 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenuPanel();
            }

            if (GameManager.Instance.currentDirection != GameManager.GameDirection.Coin)
            {
                // 아이템 패널 활성화
                ItemPanel.SetActive(true);
            }
        }
        else
        {
            StartCoroutine(VisiblePanelAfterGameEnd());
            // 회피 패널 비활성화
            RedHitCountPanel.SetActive(false);
            BlueHitCountPanel.SetActive(false);
            // 아이템 패널 비활성화
            ItemPanel.SetActive(false);
        }
    }

    #region 선공 패널 UI 페이드 효과
    public void VisibleFirstAttackPanel(int Direction)
    {
        if (FirstAttackPanel == null || RedFirstPanel == null || BlueFirstPanel == null) return;

        if (Direction == 0)
        {
            RedFirstPanel.gameObject.SetActive(true);
            StartCoroutine(UnvisibleFirstAttackPanel(RedFirstPanel));
        }
        else
        {
            BlueFirstPanel.gameObject.SetActive(true);
            StartCoroutine(UnvisibleFirstAttackPanel(BlueFirstPanel));
        }
    }

    IEnumerator UnvisibleFirstAttackPanel(GameObject panel)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
        FirstAttackPanel.gameObject.SetActive(false);
    }
    #endregion

    #region 레디 패널 효과
    public void ShowReadyUI()
    {
        if (TurnManager.Instance.isRedReady && !isRedUIShown)
        {
            isRedUIShown = true;
            RedReady.anchoredPosition = new Vector3(RRhiddenPosX, RedReady.anchoredPosition.y);
            RedReady.DOAnchorPosX(RRtargetPosX, Animduration).SetEase(Ease.OutBack);
        }

        if (TurnManager.Instance.isBlueReady && !isBlueUIShown)
        {
            isBlueUIShown = true;
            BlueReady.anchoredPosition = new Vector3(BRhiddenPosX, BlueReady.anchoredPosition.y);
            BlueReady.DOAnchorPosX(BRtargetPosX, Animduration).SetEase(Ease.OutBack);
        }
    }

    public void HideReadyUI()
    {
        RedReady.DOAnchorPosX(RRhiddenPosX, Animduration).SetEase(Ease.InBack);
        BlueReady.DOAnchorPosX(BRhiddenPosX, Animduration).SetEase(Ease.InBack);

        isRedUIShown = false;
        isBlueUIShown = false;
    }
    #endregion

    #region 스탯 패널 흔들림 효과(데미지)
    public void ShakeMyUI(RectTransform myUIPanel)
    {
        if (myUIPanel != null)
        {
            myUIPanel.DOKill(true);
            myUIPanel.DOShakePosition(0.5f, 20f);
        }
    }
    #endregion

    #region 회피 패널 활성/업데이트/비활성화
    public void VisibleHitPanel(TeamSide teamSide)
    {
        if (isAvoidPanelVisibled) return;
        isAvoidPanelVisibled = true;

        if (teamSide == TeamSide.Red)
        {
            RedHitCountPanel.SetActive(true);
        }
        else
        {
            BlueHitCountPanel.SetActive(true);
        }
    }

    public void UpdatePlayerHitData(PlayerType playerType, int hit)
    {
        if (playerType == PlayerType.Red)
        {
            RedHitCountText.text = hit.ToString();
        }
        else
        {
            BlueHitCountText.text = hit.ToString();
        }
    }

    public void UnVisibleHitPanel(TeamSide teamSide)
    {
        if (!isAvoidPanelVisibled) return;
        isAvoidPanelVisibled = false;

        if (teamSide == TeamSide.Red)
        {
            RedHitCountPanel.SetActive(false);
        }
        else
        {
            BlueHitCountPanel.SetActive(false);
        }
    }
    #endregion

    #region 아이템 패널 시각 효과
    public void UpdateItemPanelUI(GameObject panel, bool value)
    {
        value = !value;

        // 전달받은 패널 이미지 컴포넌트 참조
        Image image = panel.GetComponent<Image>();
        if (image == null) return;

        Color tempColor = image.color;

        if (value)
        {
            tempColor.a = 0.5f;
        }
        else
        {
            tempColor.a = 1f;
        }

        image.color = tempColor;
    }

    #endregion

    // 메뉴 패널 토글(메뉴 상태에 따라 마우스 커서 활성화/비활성화)
    void ToggleMenuPanel()
    {
        if (MenuPanel != null)
        {
            bool isActive = !MenuPanel.activeSelf;
            MenuPanel.SetActive(isActive);

            if (isActive)
            {
                MouseEvent.Instance.ShowCursor();
            }
            else
            {
                MouseEvent.Instance.HideCursor();
            }
        }
    }

    // 게임 종료 후 메뉴 패널 활성화
    IEnumerator VisiblePanelAfterGameEnd()
    {
        if (isMenuPanelVisibled) yield break;
        yield return new WaitForSeconds(3f);
        isMenuPanelVisibled = true;
        MenuPanel.gameObject.SetActive(true);
        MouseEvent.Instance.ShowCursor();
    }

}
