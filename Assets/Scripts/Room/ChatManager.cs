using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private TMP_InputField _inputField;

    private void Awake()
    {
        _inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        SendChat(text);

        _inputField.text = "";
    }

    private void SendChat(string message)
    {
        AddMessage($"<You> {message}");
        CanvasRoom.Instance.RpcBroadcastChat(message, NetworkRunnerManager.Instance.GetLocalPlayer());
    }

    public void AddMessage(string text)
    {
        GameObject message = AD.Managers.PoolM.PopFromPool(AD.GameConstants.PoolObjects.Message.ToString());
        message.transform.SetParent(_content, false);
        message.GetComponent<TMP_Text>().text = text;
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0;
    }
}
