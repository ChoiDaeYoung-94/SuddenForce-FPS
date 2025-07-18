using TMPro;
using UnityEngine;

public class PopupMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    
    public void SetMessage(string message)
    {
        _messageText.text = message;
    }   
}
