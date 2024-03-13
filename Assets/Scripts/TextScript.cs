using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TextScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        if (SceneManager.GetActiveScene().name.Equals("BlackJack")) {
            if (gameObject.name.Equals("BetAmount")) {
                GameManager.BlackJack.betAmountText = gameObject.GetComponent<TextMeshProUGUI>();
                GameManager.BlackJack.betAmountText.text = "Bet Amount";
            } else if (gameObject.name.Equals("CardNum")) {
                GameManager.BlackJack.cardNumText = gameObject.GetComponent<TextMeshProUGUI>();
                GameManager.BlackJack.cardNumText.text = "52";
            } else if (gameObject.name.Equals("ChipNum")) {
                GameManager.BlackJack.chipNumText = gameObject.GetComponent<TextMeshProUGUI>();
                GameManager.BlackJack.chipNumText.text = GameManager.currentChips + " chips";
            } else if (gameObject.name.Equals("Message")) {
                GameManager.BlackJack.messageText = gameObject.GetComponent<TextMeshProUGUI>();
                GameManager.BlackJack.messageText.text = "";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
