using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaRectTransform;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        UnitSelesctionManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
        UnitSelesctionManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;

        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf) UpdateVisual();
    }

    private void UnitSelectionManager_OnSelectionAreaStart(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);
    }

    private void UnitSelectionManager_OnSelectionAreaEnd(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        Rect selectionAreaRect = UnitSelesctionManager.Instance.GetSelectionAreaRect();

        float canvasScale = canvas.transform.localScale.x;
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
        selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
    }
}