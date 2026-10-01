using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class WelcomePage : MonoBehaviour
{
    [Tooltip("Nama scene tujuan (harus ada di Build Settings). Kosongkan untuk hanya menampilkan log.")]
    public string nextSceneName = "";
    public float fadeDuration = 1f;

    void Start()
    {
        var button = GetComponentInChildren<Button>();
        if (button != null) button.onClick.AddListener(OnStartClicked);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        var group = gameObject.GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            group.alpha = t / fadeDuration;
            yield return null;
        }
        group.alpha = 1f;
    }

    void OnStartClicked()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.Log("Tombol MULAI ditekan. Isi 'Next Scene Name' pada Canvas untuk pindah scene.");
    }
}
