using UnityEngine;
using UnityEngine.UI;

// Scene_01_Welcome : halaman pembuka aplikasi
public class WelcomeScene : MonoBehaviour {
    void Start() {
        var root = Ui.MakeCanvas("Canvas").transform;
        Ui.Solid(root, Pal.Primary, "Bg");
        var d1 = Ui.Box(root, new Color(1, 1, 1, .08f), "Deco1", true); Ui.At(d1.rectTransform, 1, 1, 200, -150, 800, 800);
        var d2 = Ui.Box(root, new Color(1, 1, 1, .06f), "Deco2", true); Ui.At(d2.rectTransform, 0, 0, -250, 350, 900, 900);
        var dots = new Image[3];
        Color[] dc = { Pal.Warm, Pal.Accent, new Color(1, 1, 1, .55f) };
        Vector2[] da = { new Vector2(.14f, .70f), new Vector2(.86f, .58f), new Vector2(.78f, .80f) };
        for (int i = 0; i < 3; i++) {
            dots[i] = Ui.Box(root, dc[i], "Dot" + i, true);
            Ui.At(dots[i].rectTransform, da[i].x, da[i].y, 0, 0, i == 2 ? 40 : 64, i == 2 ? 40 : 64);
        }

        var halo = Ui.Box(root, new Color(1, 1, 1, .16f), "Halo", true); Ui.At(halo.rectTransform, .5f, .5f, 0, 420, 400, 400);
        var logo = Ui.Box(root, Color.white, "Logo"); Ui.At(logo.rectTransform, .5f, .5f, 0, 420, 300, 300);
        var e = Ui.Label(logo.transform, "E", 190, Pal.Primary, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(e.rectTransform);
        var wd = Ui.Box(logo.transform, Pal.Warm, "Dot", true); Ui.At(wd.rectTransform, 1, 1, -34, -34, 58, 58);
        Ui.Pop(this, logo.transform, .1f, .7f);

        var t1 = Ui.Label(root, "EduSmart", 112, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.At(t1.rectTransform, .5f, .5f, 0, 170, 900, 130);
        var t2 = Ui.Label(root, "Interactive Learning", 48, new Color(1, 1, 1, .92f)); Ui.At(t2.rectTransform, .5f, .5f, 0, 85, 900, 70);
        var t3 = Ui.Label(root, "Belajar jadi lebih seru, di mana saja", 34, new Color(1, 1, 1, .75f)); Ui.At(t3.rectTransform, .5f, .5f, 0, 25, 900, 60);
        Ui.Pop(this, t1.transform, .25f); Ui.Pop(this, t2.transform, .35f); Ui.Pop(this, t3.transform, .45f);

        // panel bawah
        var panel = Ui.Box(root, Color.white, "Panel"); var pr = panel.rectTransform;
        pr.anchorMin = new Vector2(0, 0); pr.anchorMax = new Vector2(1, 0); pr.pivot = new Vector2(.5f, 0);
        pr.anchoredPosition = new Vector2(0, -100); pr.sizeDelta = new Vector2(0, 880);
        var h = Ui.Label(panel.transform, "Halo! Siapa namamu?", 56, Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.At(h.rectTransform, .5f, 1, 0, -70, 900, 80);
        var s = Ui.Label(panel.transform, "Masukkan namamu untuk memulai petualangan belajar", 32, Pal.Muted); Ui.At(s.rectTransform, .5f, 1, 0, -155, 900, 50);
        var inp = Ui.Input(panel.transform, "Ketik namamu...", 900, 130); Ui.At(inp.GetComponent<RectTransform>(), .5f, 1, 0, -250, 900, 130);
        if (PlayerPrefs.HasKey("name")) inp.text = GameState.PlayerName;
        var go = Ui.Btn(panel.transform, "Mulai Belajar", Pal.Primary, Color.white, 900, 140, () => {
            var n = inp.text.Trim(); GameState.PlayerName = n.Length == 0 ? "Siswa" : n; Nav.Go(Nav.S2);
        }, 44);
        Ui.At(go.GetComponent<RectTransform>(), .5f, 1, 0, -440, 900, 140);
        var v = Ui.Label(panel.transform, "EduSmart Interactive Learning  -  v1.0", 28, Pal.Muted); Ui.At(v.rectTransform, .5f, 1, 0, -640, 900, 50);
        Ui.Slide(this, pr, new Vector2(0, -1000), .3f, .7f);

        Ui.Loop(this, root.gameObject, t => {
            d1.rectTransform.anchoredPosition = new Vector2(200, -150 + Mathf.Sin(t * .7f) * 30);
            d2.rectTransform.anchoredPosition = new Vector2(-250, 350 + Mathf.Cos(t * .6f) * 30);
            for (int i = 0; i < 3; i++) dots[i].rectTransform.anchoredPosition = new Vector2(0, Mathf.Sin(t * 1.2f + i * 2) * 28);
            halo.rectTransform.localScale = Vector3.one * (1.04f + .1f * Mathf.Sin(t * 2f));
        });
    }
}
