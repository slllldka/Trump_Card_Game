using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{

    public int playerPos = 0;
    public List<GameObject> cardObjects;

    // Start is called before the first frame update
    void Start()
    {
        cardObjects = new List<GameObject>();
        GameManager.OneCard.enemys.Add(this);
        GameManager.OneCard.participants.Add(new GameManager.pii(GameManager.OneCard.EnemyCode, GameManager.OneCard.enemys.Count - 1));
        playerPos = GameManager.OneCard.enemys.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startTurnOneCard() {
        for (int i = 0; i < cardObjects.Count; i++) {
            cardObjects[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    public void drawCardOneCard() {
        cardObjects.Add(Instantiate(GameManager.gm.enemyCardPrefab));
        cardObjects[cardObjects.Count - 1].name = "EnemyCard" + playerPos.ToString() + (cardObjects.Count - 1);
        cardObjects[cardObjects.Count - 1].transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        cardObjects[cardObjects.Count - 1].transform.position = GameManager.OneCard.cardPos[(cardObjects.Count - 1) / GameManager.OneCard.cardsPerLayer, playerPos]
            + GameManager.OneCard.cardVec[playerPos < (3 - playerPos) ? playerPos : (3 - playerPos)] * ((cardObjects.Count - 1) % GameManager.OneCard.cardsPerLayer) * 3 / 4
            + new Vector3(0, 0, -0.1f) * ((cardObjects.Count - 1) - (cardObjects.Count - 1) % GameManager.OneCard.cardsPerLayer) * 3 / 4;
        //cardObjects[cardObjects.Count - 1].transform.rotation = Quaternion.Euler(GameManager.OneCard.rotations[playerPos % 2]);
        StartCoroutine(GameManager.OneCard.cardAction(GameObject.Find("Deck").transform.position, cardObjects[cardObjects.Count - 1].transform.position, 0.1f, playerPos, null));
    }
    public void playCardOneCard() {
        Destroy(cardObjects[cardObjects.Count - 1]);
        cardObjects.RemoveAt(cardObjects.Count - 1);
        StartCoroutine(GameManager.OneCard.cardAction(GameManager.OneCard.positions[playerPos], GameManager.OneCard.openedCard.transform.position, 0.1f, playerPos, null));
    }
    public void endTurnOneCard() {
        for (int i = 0; i < cardObjects.Count; i++) {
            cardObjects[i].GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }
}
