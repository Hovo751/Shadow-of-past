using TMPro;
using UnityEngine;

public class ComboCounter : MonoBehaviour
{
    public TextMeshProUGUI player1ComboText;
    public TextMeshProUGUI player2ComboText;
    public Player player1;
    public Player player2;

    private void Update()
    {
        int player1Combo = 0;
        int player2Combo = 0;

        if (player1 != null)
        {
            player1Combo = player1.changebleStats.Combo;
        }
        if (player2 != null)
        {
            player2Combo = player2.changebleStats.Combo;
        }
        if (player1Combo > 0)
        {
            player1ComboText.text = player1Combo.ToString();
        }
        else
        {
            player1ComboText.text = "";
        }
        if (player2Combo > 0)
        {
            player2ComboText.text = player2Combo.ToString();
        }
        else
        {
            player2ComboText.text = "";
        }
    }
}
