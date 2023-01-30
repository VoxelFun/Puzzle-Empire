using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour {

    [Header("Requirement")]
    public CanvasGroup canvasGroup;
    public Text infoText;
    public GameObject skipButton;

    private System.Action skipAction;
    private bool visible;

	private void OnEnable() {
        Library.guiController = this;
	}

    #region IGUIController

    public void EditLayout(bool top, TextAnchor anchor, System.Action skipAction = null) {
        RectTransform transform = (RectTransform)infoText.transform.parent;

        int sign = top.ToSign();
        transform.pivot = new Vector2(0.5f, top.ToInt());
        transform.localPosition = new Vector2(0, -16 * sign);
        transform.parent.localPosition = new Vector3(0, 960 * sign);

        this.skipAction = skipAction;
        skipButton.SetActive(skipAction != null);

        infoText.alignment = anchor;
    }

    public void RestoreLayout() {
        EditLayout(true, TextAnchor.MiddleCenter);
    }

    //public void SetGUIController(IGUIController controller) {
    //    this.controller = controller;
    //}

    #endregion

    #region Info

    public void HideInfo() {
        Hide();
    }

    public void ShowInfo(string text) {
        ShowInfo(text, HideInfo);
    }

    public void ShowInfo(string text, System.Action onGetButton) {
        Show(onGetButton);
        infoText.text = text;
    }

    public void Skip() {
        skipAction();
    }

    #endregion

    #region Other

    public bool IsVisible() {
        return visible;
    }

    #endregion

    void Hide() {
        HideCommon();
        AnimationController.StartWithController(new AnimationType.CanvasGroup(null, canvasGroup), 0.4f);
    }

    void HideCommon() {
        canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
        visible = false;
    }

    public void HideImmediately() {
        StopAllCoroutines();
        HideCommon();
        canvasGroup.alpha = 0;
    }

    void Show(System.Action onGetButton) {
        canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;

        StartCoroutine(WaitForAction(onGetButton));
        visible = true;
    }

    IEnumerator WaitForAction(System.Action action){
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        if(visible)
            action();
    }

}
