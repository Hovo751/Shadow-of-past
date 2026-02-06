using UnityEngine;

public class MoveBG : MonoBehaviour
{
    public Transform cm;
    private void Update()
    {
        transform.position = new Vector3(cm.position.x * transform.position.z, transform.position.y, transform.position.z);
    }
}
