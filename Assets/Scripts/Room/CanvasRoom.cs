using UnityEngine;
using UnityEngine.UI;

public class CanvasRoom : MonoBehaviour
{
    [Header("--- CanvasRoom data ---")]
    [SerializeField] private RectTransform _teamPanel;
    [SerializeField] private RectTransform _redTeam;
    [SerializeField] private RectTransform _blueTeam;
    [SerializeField] private GridLayoutGroup __redTeamGridLayoutGroup;
    [SerializeField] private GridLayoutGroup __blueTeamGridLayoutGroup;

    private void Start()
    {
        SetUIResolution();
    }

    private void SetUIResolution()
    {
        float width = _teamPanel.rect.width / 2f;
        float height = (_teamPanel.rect.height - 100f) / 4f;

        _redTeam.sizeDelta = new Vector2(width, _redTeam.sizeDelta.y);
        _blueTeam.sizeDelta = new Vector2(width, _blueTeam.sizeDelta.y);

        __redTeamGridLayoutGroup.cellSize = new Vector2(width, height);
        __blueTeamGridLayoutGroup.cellSize = new Vector2(width, height);
    }
}
