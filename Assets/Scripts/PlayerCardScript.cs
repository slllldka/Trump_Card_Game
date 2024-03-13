using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerCardScript : MonoBehaviour
{
    public int card = -1;
    public int overStack;
    public bool isMouseOver;

    public BoxCollider2D bc;
    public SpriteRenderer sprite;

    // Start is called before the first frame update
    private void Awake() {
        overStack = 1;
        isMouseOver = false;
        bc = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(card >= 0) {
            sprite.sprite = GameManager.gm.cardSprites[card];
        }
    }

    public void OnMouseUp() {
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            if (sprite.color == Color.white) {
                if (isMouseOver) {
                    bool destroyed = false;
                    int cardNum = GameManager.gm.player.cards.Count;
                    for (int i = 0; i < cardNum; i++) {
                        if (gameObject.name.Equals("PlayerCard" + i)) {
                            GameManager.gm.player.cards.RemoveAt(i);
                            GameManager.gm.player.cardObjects.RemoveAt(i);
                            if (!GameManager.OneCard.isPvP) {
                                GameManager.OneCard.usedCards.Insert(0, card);
                                GameManager.OneCard.openedCard.card = GameManager.OneCard.usedCards[0];
                            }
                            destroyed = true;
                        } else {
                            if (destroyed) {
                                GameManager.gm.player.cardObjects[i - 1].name = "PlayerCard" + (i - 1);
                                GameManager.gm.player.cardObjects[i - 1].transform.position = GameManager.OneCard.cardPos[0, 0] + GameManager.OneCard.cardVec[0] * (i - 1);
                            }
                        }
                    }
                    
                    if (GameManager.OneCard.isPvP) {
                        GameManager.gm.player.endTurnOneCard(true, card);
                    } else {
                        if (card % GameManager.cardsPerSuit + 1 == 7) {
                            GameObject.Find("Deck").GetComponent<SpriteRenderer>().color = Color.gray;
                            for (int i = 0; i < GameManager.gm.player.cards.Count; i++) {
                                GameManager.gm.player.cardObjects[i].GetComponent<PlayerCardScript>().sprite.color = Color.gray;
                            }
                            GameManager.OneCard.setAllSuitBtnsInteractable(true);
                        } else {
                            StartCoroutine(GameManager.OneCard.cardEffect(card / GameManager.cardsPerSuit, card % GameManager.cardsPerSuit + 1));
                        }
                    }

                    gameObject.transform.position += new Vector3(0, 0, -10f);
                    sprite.color = Color.gray;
                    StartCoroutine(GameManager.OneCard.cardAction(gameObject.transform.position, GameManager.OneCard.openedCard.gameObject.transform.position, 0.1f, 0, gameObject));
                }
            }
        }
    }

    public void OnMouseOver() {
        isMouseOver = true;
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            if (sprite.color == Color.white) {
                if (overStack == 1) {
                    overStack--;
                    bc.size += new Vector2(0, 0.3f);
                    bc.offset -= new Vector2(0, 0.3f);
                    gameObject.transform.position += new Vector3(0, 0.3f, 0);
                }
            }
        }
    }
    public void OnMouseExit() {
        isMouseOver = false;
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            if (sprite.color == Color.white) {
                if (overStack == 0) {
                    bc.size -= new Vector2(0, 0.3f);
                    bc.offset += new Vector2(0, 0.3f);
                    gameObject.transform.position -= new Vector3(0, 0.3f, 0);
                    overStack++;
                }
            }
        }
    }
}
