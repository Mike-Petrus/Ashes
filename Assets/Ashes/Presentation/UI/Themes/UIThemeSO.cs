using UnityEngine;

[CreateAssetMenu(fileName = "NewUITheme", menuName = "Ashes/UI/Theme")]
public class UIThemeSO : ScriptableObject
{
    [Header("Panel Backgrounds")]
    public Color PanelNormal = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color PanelHover = new Color(0.55f, 0.55f, 0.55f, 0.9f);
    public Color PanelActive = new Color(0.1f, 0.3f, 0.6f, 0.9f);

    [Header("Text & Cursors")]
    public Color TextNormal = Color.white;
    public Color TextHighlight = Color.yellow;
    public Color TextDisabled = Color.gray;

    [Header("Bars")]
    public Color ATBFilling = Color.red;
    public Color ATBReady = Color.green;

    [Header("Floating Text")]
    public Color DamageText = Color.white;
    public Color HealText = Color.green;
    public Color SystemText = Color.cyan;
}