using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Efek tekan (mengecil sedikit) untuk semua tombol.
public class ButtonFx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    Coroutine c;
    public void OnPointerDown(PointerEventData e) { To(.93f); }
    public void OnPointerUp(PointerEventData e) { To(1f); }
    public void OnPointerExit(PointerEventData e) { To(1f); }
    void To(float s) {
        if (c != null) StopCoroutine(c);
        if (gameObject.activeInHierarchy) c = StartCoroutine(Co(s));
    }
    IEnumerator Co(float s) {
        float from = transform.localScale.x, t = 0;
        while (t < 1) { t += Time.unscaledDeltaTime / .12f; transform.localScale = Vector3.one * Mathf.Lerp(from, s, t); yield return null; }
    }
}
