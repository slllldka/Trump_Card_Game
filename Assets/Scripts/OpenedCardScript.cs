using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenedCardScript : MonoBehaviour
{
    public int card = -1;
    public SpriteRenderer sprite;

    void Awake() {
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            GameManager.OneCard.openedCard = this;
            sprite = GetComponent<SpriteRenderer>();
            if (!GameManager.OneCard.isPvP) {
                GameManager.OneCard.usedCards.Insert(0, GameManager.OneCard.drawCard());
            }
            card = GameManager.OneCard.usedCards[0];
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update() {
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            if (GameManager.OneCard.usedCards.Count > 0) {
                sprite.sprite = GameManager.gm.cardSprites[card];
            }
        }
    }
}
