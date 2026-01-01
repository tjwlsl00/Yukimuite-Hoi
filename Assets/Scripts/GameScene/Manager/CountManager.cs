using UnityEngine;
using System.Collections.Generic;

public enum PlayerType { Red, Blue }
public enum MoveDirection { Left, Right }

public class CountManager : MonoBehaviour
{
    // 싱글톤
    public static CountManager Instance;

    [SerializeField] int PenaltyCount = 3;

    // 플레이어 카운트 데이터
    private class PlayerCountData
    {
        public int HitCount = 0;

        public void Reset()
        {
            HitCount = 0;
        }
    }

    // 플레이어 타입 키 딕셔너리
    private Dictionary<PlayerType, PlayerCountData> playerRecords = new Dictionary<PlayerType, PlayerCountData>();

    void Awake()
    {
        // 싱글톤
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 데이터 초기화
        playerRecords.Add(PlayerType.Red, new PlayerCountData());
        playerRecords.Add(PlayerType.Blue, new PlayerCountData());
    }

    #region 카운팅 처리 / 패널티
    public void AddCount(PlayerType playerType)
    {
        if (!playerRecords.ContainsKey(playerType)) return;
        PlayerCountData data = playerRecords[playerType];

        data.HitCount ++;

        // 패널티 체크 
        CheckPenalty(playerType, data.HitCount);

        // 카운트UI 업데이트
        UIManager.Instance.UpdatePlayerHitData(playerType, data.HitCount);
    }

    public void CheckPenalty(PlayerType playerType, int count)
    {
        if (count >= PenaltyCount)
        {
            Debug.Log("패널티 적용! 카운트 초기화 됩니다.");

            if (playerType == PlayerType.Red)
            {
                TurnManager.Instance.isRedStun = true;
            }
            else
            {
                TurnManager.Instance.isBlueStun = true;
            }

            // 패널티 적용 후 데이터 초기화 
            playerRecords[playerType].Reset();
        }
    }
    #endregion

}
