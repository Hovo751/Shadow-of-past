using UnityEngine;

public class MoveBG : MonoBehaviour
{
    public Transform cm;
    public float yAmp = 0.0f;

    private float startX;
    private float startY;

    private void Start()
    {
        startX = transform.position.x;
        startY = transform.position.y;
    }

    private void Update()
    {
        transform.position = new Vector3(cm.position.x * transform.position.z + startX, startY - (cm.position.y - 4) * transform.position.z * yAmp, transform.position.z);
    }
}
