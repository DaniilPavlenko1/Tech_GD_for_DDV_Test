using UnityEngine;
using TMPro;

public class CarrotCounter : MonoBehaviour
{
    public static CarrotCounter Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private UnityEngine.UI.Image carrotIcon;

    private int _count;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCarrot(int amount = 1)
    {
        _count += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (counterText) counterText.text = "x" + _count;
    }
}
