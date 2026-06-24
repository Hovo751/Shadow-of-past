using UnityEngine;
using UnityEngine.Rendering;

public class Camera : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    void Update()
    {
        if (player1 == null) return;
        if (player2 == null) return;
        transform.position = Vector3.Lerp(transform.position, (player1.position + player2.position) / 2f + new Vector3(0, 4, 0), 0.1f);
        transform.position = new Vector3(transform.position.x, 4, -20);
    }

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }
}
