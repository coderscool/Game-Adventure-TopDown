using UnityEngine;

public class YSort : MonoBehaviour
{
    public Transform sortPoint;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
    }

    void LateUpdate()
    {
        sr.sortingOrder = Mathf.RoundToInt(-sortPoint.position.y * 100);
        //Debug.Log(sr.sortingOrder);
    }
}
