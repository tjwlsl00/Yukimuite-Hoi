using UnityEngine;
using UnityEngine.UI;

public class SnowSplat : MonoBehaviour
{
    [SerializeField] Image splatImage;

    public void VisibleSnowSplat()
    {
        splatImage.gameObject.SetActive(true);
    }
}
