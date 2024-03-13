using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class OpenSererverWindowScript : MonoBehaviour
{
    public TMP_InputField portField;
    public Button openServerBtn, closeServerBtn;
    private static string portString;
    // Start is called before the first frame update
    void Start()
    {
        portField = transform.Find("PortInputField").GetComponent<TMP_InputField>();
        openServerBtn = transform.Find("OpenServerBtn").GetComponent<Button>();
        closeServerBtn = transform.Find("CloseServerBtn").GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openOpenServerWindow() {
        gameObject.SetActive(true);
        if (NetworkManager.nm.opened) {
            portField.interactable = false;
            openServerBtn.interactable = false;
            closeServerBtn.interactable = true;

            portField.text = portString;
        } else {
            portField.interactable = true;
            openServerBtn.interactable = true;
            closeServerBtn.interactable = false;

            portField.text = "";
        }
    }
    public void closeOpenServerWindow() {
        gameObject.SetActive(false);
    }
    public void openServer() {
        Thread serverThread = new Thread(new ParameterizedThreadStart(NetworkManager.nm.openServer));
        serverThread.Start(int.Parse(portField.text));
        while (!NetworkManager.nm.opened) {
        }

        portField.interactable = false;
        openServerBtn.interactable = false;
        closeServerBtn.interactable = true;

        portString = portField.text;
    }
    public void closeServer() {
        NetworkManager.nm.closeServer();
        portField.interactable = true;
        openServerBtn.interactable = true;
        closeServerBtn.interactable = false;
    }
}
