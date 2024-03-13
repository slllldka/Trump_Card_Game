using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckScript : MonoBehaviour
{
    public bool isMouseOver;
    // Start is called before the first frame update
    void Start()
    {
        isMouseOver = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnMouseUp() {
        if (SceneManager.GetActiveScene().name.Equals("OneCard")) {
            if (gameObject.GetComponent<SpriteRenderer>().color == Color.white) {
                if (isMouseOver) {
                    GameManager.gm.player.endTurnOneCard(false);
                }
            }
        }
    }
    public void OnMouseOver() {
        isMouseOver = true;
    }
    public void OnMouseExit() {
        isMouseOver = false;
    }
}
