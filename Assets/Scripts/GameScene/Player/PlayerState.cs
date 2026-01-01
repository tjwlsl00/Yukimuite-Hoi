using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] public int currentHP;
    [SerializeField] public int maxHP = 100;
    [SerializeField] RectTransform PlayerStatePanel;

    private Player player;
    private PlayerAnimation playerAnimation;
    private PlayerAudio playerAudio;
    private PlayerUI playerUI;

    // bool(스킬 효과)
    public bool Item1Effect = false;
    public bool Item2Effect = false;

    // 외부 
    [SerializeField] GameObject enemyObj;
    private PlayerState enemyState;

    void Awake()
    {
        player = GetComponent<Player>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerUI = GetComponent<PlayerUI>();
        playerAudio = GetComponent<PlayerAudio>();

        enemyState = enemyObj.GetComponent<PlayerState>();
    }

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (Item2Effect)
        {
            enemyState.TakeDamage(amount * 2);
            Debug.Log("적에게 데미지 반사");
        }
        else
        {
            if (enemyState.Item1Effect)
            {
                currentHP -= amount * 2;
                Debug.Log("2배 눈덩이에 맞음");
            }
            else
            {
                currentHP -= amount;
                Debug.Log("눈덩이에 맞음");
            }

            // 스턴 카운트 
            if (this.player.teamSide == TeamSide.Red)
            {
                CountManager.Instance.AddCount(PlayerType.Red);
            }
            else
            {
                CountManager.Instance.AddCount(PlayerType.Blue);
            }

            // 데미지 애니메이션, UI 업데이트 
            playerAnimation.PlayTakeDamage();
            playerAudio.PlayHitClip();
            playerUI.UpdateGaze();
            UIManager.Instance.ShakeMyUI(PlayerStatePanel);

            if (currentHP <= 0)
            {
                Debug.Log("죽음");
                playerAnimation.PlayDefeat();

                // 게임 상태 GameEnd
                GameManager.Instance.currentDirection = GameManager.GameDirection.GameEnd;
            }
        }
    }
}