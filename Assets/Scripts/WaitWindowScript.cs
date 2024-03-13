using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class WaitWindowScript : MonoBehaviour
{
    private GameObject loading;
    private TMP_InputField inputField;
    private TextMeshProUGUI message;
    private Button startBtn, exitBtn;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.OneCard.waitWindow = gameObject;
        loading = transform.Find("Loading").gameObject;
        inputField = transform.Find("InputField").GetComponent<TMP_InputField>();
        message = transform.Find("Message").GetComponent<TextMeshProUGUI>();
        startBtn = transform.Find("StartBtn").GetComponent<Button>();
        exitBtn = transform.Find("ExitBtn").GetComponent<Button>();

        loading.SetActive(false);
        inputField.interactable = true;
        inputField.text = "";
        message.text = "";
        startBtn.interactable = true;
        exitBtn.interactable = true;

        gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        while(NetworkManager.nm.receivedWaitWindowQueue.Count > 0) {
            string rec = NetworkManager.nm.receivedWaitWindowQueue.Dequeue();
            if (rec.Equals("Existing Name")) {
                message.text = "Name Exists!";
            } else if (rec.StartsWith("Start")) {
                GameManager.OneCard.name = inputField.text;
                rec = rec.Substring(5);
                GameManager.OneCard.nameList.Clear();
                int count = 0;
                string startCard = "";
                foreach(string name in rec.Split('.')) {
                    if (count == 0) {
                        startCard = name;
                    } else {
                        if (!name.Equals("")) {
                            GameManager.OneCard.nameList.Add(name);
                        }
                    }
                    count++;
                }
                GameManager.OneCard.start(true, int.Parse(startCard));
            } else {
                message.text = int.Parse(rec) + "/4";
                inputField.interactable = false;
                startBtn.interactable = false;
                if (!loading.activeSelf) {
                    loading.SetActive(true);
                    StartCoroutine(startLoading());
                }
                if (int.Parse(rec) == 4) {
                    exitBtn.interactable = false;
                }
            }
        }
    }

    public void open() {
        loading.SetActive(false);
        inputField.interactable = true;
        inputField.text = "";
        message.text = "";
        startBtn.interactable = true;
        exitBtn.interactable = true;

        gameObject.SetActive(true);
    }

    public IEnumerator startLoading() {
        float angle = 360;
        while (loading.activeSelf && gameObject.activeSelf) {
            loading.transform.rotation = Quaternion.Euler(0, 0, angle);
            angle -= 3;
            if(angle < 0) {
                angle = 360;
            }
            //Debug.Log(angle);
            yield return null;
        }
    }

    public void start() {
        NetworkManager.nm.sendQueue.Enqueue("W"+inputField.text);
    }
    public void exit() {
        NetworkManager.nm.sendQueue.Enqueue("WExit" + inputField.text);
        gameObject.SetActive(false);
    }
}
