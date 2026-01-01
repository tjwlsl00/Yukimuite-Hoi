using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    private int RedSelectedOption = 0;
    private int BlueSelectedOption = 0;
    public bool isRedReady = false;
    public bool isBlueReady = false;
    public bool isRedStun = false;
    public bool isBlueStun = false;
    private bool isStunTurn = false;
    private bool SwitchTurn = false;
    [SerializeField] float TurnPlayTime = 3f;
    private Coroutine resetTurnCoroutine;

    // 외부
    [SerializeField] GameObject[] players;
    public Player red;
    private PlayerState redState;
    private PlayerAnimation redAnimation;
    private PlayerAudio redAudio;
    public Player blue;
    private PlayerState blueState;
    private PlayerAnimation blueAnimation;
    private PlayerAudio blueAudio;
    // 카메라 
    [SerializeField] float targetFOV = 30f;
    [SerializeField] CinemachineCamera vcamRed;
    [SerializeField] CinemachineCamera vcamBlue;
    // 코인
    [SerializeField] GameObject Coin;

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

        // 외부 (플레이어)
        red = players[0].GetComponent<Player>();
        redState = players[0].GetComponent<PlayerState>();
        redAnimation = players[0].GetComponent<PlayerAnimation>();
        redAudio = players[0].GetComponent<PlayerAudio>();

        blue = players[1].GetComponent<Player>();
        blueState = players[1].GetComponent<PlayerState>();
        blueAnimation = players[1].GetComponent<PlayerAnimation>();
        blueAudio = players[1].GetComponent<PlayerAudio>();
    }

    void Update()
    {
        if (GameManager.Instance.currentDirection == GameManager.GameDirection.GameEnd)
        {
            if (resetTurnCoroutine != null)
            {
                StopCoroutine(resetTurnCoroutine);
                resetTurnCoroutine = null;
            }
        }
        else
        {
            bool shouldSkipDueToStun = (isRedStun && red.currentPlayerDirection == Player.PlayerDirection.Attack) || (isBlueStun && blue.currentPlayerDirection == Player.PlayerDirection.Attack);

            if (shouldSkipDueToStun)
            {
                if (resetTurnCoroutine == null)
                {
                    isStunTurn = true;
                    resetTurnCoroutine = StartCoroutine(ResetTurn(false, true));
                }
            }
        }
    }

    #region 게임 셋업
    public void SetPlayerDirection(int Direction)
    {
        if (Direction == 0)
        {
            Debug.Log("레드가 선공입니다");
            red.currentPlayerDirection = Player.PlayerDirection.Attack;
            blue.currentPlayerDirection = Player.PlayerDirection.Defense;
        }
        else
        {
            Debug.Log("블루가 선공입니다");
            red.currentPlayerDirection = Player.PlayerDirection.Defense;
            blue.currentPlayerDirection = Player.PlayerDirection.Attack;
        }

        // 게임 셋업 준비(코인 비활성화, 게임 상태 레디, 카메라 설정)
        StartCoroutine(SetupGameToStart(Direction));
    }

    IEnumerator SetupGameToStart(int Direction)
    {
        //coin 안보이게 
        yield return new WaitForSeconds(1f);
        Coin.gameObject.SetActive(false);

        // 선공 알림이
        UIManager.Instance.VisibleFirstAttackPanel(Direction);

        // 오디오 효과
        if (Direction == 0)
        {
            redAudio.PlayGoodLuck();
        }
        else
        {
            blueAudio.PlayGoodLuck();
        }

        //동전 비활성화 2초 후 -> 게임 상태 Ready/카메라 세팅
        yield return new WaitForSeconds(2f);
        GameManager.Instance.currentDirection = GameManager.GameDirection.Ready;
        SwitchTurnAndCamera();
    }
    #endregion

    #region 옵션 선택 값 가져오기 -> 레디 체크 후 -> 턴플레이 
    public void GetRedOptionNum(int num)
    {
        isRedReady = true;
        RedSelectedOption = num;

        CheckBothReady();

        // UI 적용
        UIManager.Instance.ShowReadyUI();
    }

    public void GetBlueOptionNum(int num)
    {
        isBlueReady = true;
        BlueSelectedOption = num;

        CheckBothReady();

        // UI 적용
        UIManager.Instance.ShowReadyUI();
    }

    // 준비 상태 체크 
    private void CheckBothReady()
    {
        if (GameManager.Instance.currentDirection == GameManager.GameDirection.GameEnd) return;

        if (isRedReady && isBlueReady)
        {
            Debug.Log("양쪽 모두 레디를 완료했습니다.");
            TurnPlay();
            StartCoroutine(ResetTurn(false, false));
        }
    }
    #endregion

    #region 턴 플레이 / 초기화
    private void TurnPlay()
    {
        if (red.currentPlayerDirection == Player.PlayerDirection.Attack)
        {
            redAnimation.PlayThrow();
            redAudio.PlayThrowClip();
        }
        else
        {
            redAnimation.PlayAvoid(RedSelectedOption);
        }

        if (blue.currentPlayerDirection == Player.PlayerDirection.Attack)
        {
            blueAnimation.PlayThrow();
            blueAudio.PlayThrowClip();
        }
        else
        {
            blueAnimation.PlayAvoid(BlueSelectedOption);
        }
    }

    // 턴 초기화
    public IEnumerator ResetTurn(bool SwitchTurn, bool StunTurn)
    {
        // 턴 정상 진행 후 리셋 
        if (!SwitchTurn && !StunTurn)
        {
            GameManager.Instance.currentDirection = GameManager.GameDirection.Play;

            yield return new WaitForSeconds(TurnPlayTime);

            if (GameManager.Instance.currentDirection == GameManager.GameDirection.GameEnd)
            {
                resetTurnCoroutine = null;
                yield break;
            }

            // 모든 플레이어 아이템 선택 상태, 기존 효과 제거 
            ItemManager.Instance.ResetSelected();
            ItemManager.Instance.DeleteItemEffect();

            // 레디 상태, 옵션 선택 값 초기화
            isRedReady = false;
            isBlueReady = false;
            RedSelectedOption = 0;
            BlueSelectedOption = 0;

            // 공수 전환
            red.ChangeDirection();
            blue.ChangeDirection();

            // 카메라 전환
            SwitchTurnAndCamera();

            // 플레이 -> 레디 상태
            GameManager.Instance.currentDirection = GameManager.GameDirection.Ready;

            // 코루틴 변수 null로 초기화 
            resetTurnCoroutine = null;
        }
        else if (SwitchTurn && !StunTurn)
        {
            // 공수 전환
            red.ChangeDirection();
            blue.ChangeDirection();

            // 카메라 전환
            SwitchTurnAndCamera();

            // 레디 상태, 옵션 선택 값 초기화
            isRedReady = false;
            isBlueReady = false;
            RedSelectedOption = 0;
            BlueSelectedOption = 0;

            // 코루틴 null초기화 
            resetTurnCoroutine = null;
        }
        else if(!SwitchTurn && StunTurn)
        {
            if (isRedStun)
            {
                redAnimation.PlayStun();
            }
            else if (isBlueStun)
            {
                blueAnimation.PlayStun();
            }

            yield return new WaitForSeconds(2f);

            // 공수 전환
            red.ChangeDirection();
            blue.ChangeDirection();

            // 카메라 전환
            SwitchTurnAndCamera();

            isRedStun = false;
            isBlueStun = false;

            isStunTurn = false;
            // 코루틴 null초기화 
            resetTurnCoroutine = null;
        }
    }
    #endregion

    #region 카메라 시점 변경(플레이 중) / 최종 승자 
    // 카메라 시점 변경 / Red 기준
    private void SwitchTurnAndCamera()
    {
        Debug.Log("카메라 전환 함수 호출됨!");

        if (red.currentPlayerDirection == Player.PlayerDirection.Attack)
        {
            vcamRed.Priority = 20;
            vcamBlue.Priority = 10;
        }
        else if (red.currentPlayerDirection == Player.PlayerDirection.Defense)
        {
            vcamRed.Priority = 10;
            vcamBlue.Priority = 20;
        }
    }

    // 최종 승자 체크 
    public void CheckFinalWinner()
    {
        if (redState.currentHP > 0 && blueState.currentHP <= 0)
        {
            StartCoroutine(SwitchFinalCamera(0));
        }
        else if (blueState.currentHP > 0 && redState.currentHP <= 0)
        {
            StartCoroutine(SwitchFinalCamera(1));
        }
    }

    IEnumerator SwitchFinalCamera(int num)
    {
        Debug.Log("최종 카메라");

        yield return new WaitForSeconds(2f);
        if (num == 0)
        {
            redAnimation.PlayWin();
            redAudio.PlayWinClip();

            //정면 시점
            vcamRed.Priority = 10;
            vcamBlue.Priority = 20;

            var lensSettings = vcamBlue.Lens;
            lensSettings.FieldOfView = targetFOV;
            vcamBlue.Lens = lensSettings;
        }
        else
        {
            blueAnimation.PlayWin();
            blueAudio.PlayWinClip();

            //정면 시점
            vcamRed.Priority = 20;
            vcamBlue.Priority = 10;

            var lensSettings = vcamRed.Lens;
            lensSettings.FieldOfView = targetFOV;
            vcamRed.Lens = lensSettings;
        }
    }
    #endregion
}