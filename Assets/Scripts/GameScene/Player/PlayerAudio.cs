using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] AudioClip GoodLuckClip;
    [SerializeField] AudioClip[] ThrowClips;
    [SerializeField] AudioClip HitClip;
    [SerializeField] AudioClip WinClip;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayThrowClip()
    {
        int random = Random.Range(0, 2);

        if (random == 0)
        {
            audioSource.clip = ThrowClips[0];
            audioSource.Play();
        }
        else
        {
            audioSource.clip = ThrowClips[1];
            audioSource.Play();
        }
    }

    public void PlayHitClip()
    {
        audioSource.clip = HitClip;
        audioSource.Play();
    }

    public void PlayWinClip()
    {
        audioSource.clip = WinClip;
        audioSource.Play();
    }

    public void PlayGoodLuck()
    {
        audioSource.clip = GoodLuckClip;
        audioSource.Play();
    }
}