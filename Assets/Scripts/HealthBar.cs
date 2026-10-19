using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Player player;
    public Slider sl;

    private void Update()
    {
        sl.value = player.changebleStats.Health / 10000.0f;
    }
}
