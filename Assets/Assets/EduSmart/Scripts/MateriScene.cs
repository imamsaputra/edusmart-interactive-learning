using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Scene_03_InteractiveMateri : materi interaktif (teks, audio/video, ilustrasi)
public class MateriScene : MonoBehaviour {
    Transform root; RectTransform body; Subject sub;
    int page, tab; bool playing; float audioT;
    Image[] tabImg = new Image[3]; Text[] tabTx = new Text[3];

    void Start() { root = Ui.MakeCanvas("Canvas").transform; Build(); }

    void Build() {
        Ui.Clear(root); page = 0; tab = 0; playing = false; audioT = 0;
        sub = Data.All[GameState.Subject];
        Ui.Solid(root, Pal.Bg, "Bg");
        Ui.Header(root, "Materi", sub.Name, sub.Color, Nav.S2);

        // pilih mata pelajaran
        for (int i = 0; i < Data.All.Length; i++) {
            int idx = i; var s = Data.All[i]; bool on = i == GameState.Subject;
            var chip = Ui.Box(root, on ? s.Color : Color.white, "Chip" + i); Ui.At(chip.rectTransform, 0, 1, 60 + i * 245, -330, 225, 80);
            var l = Ui.Label(chip.transform, s.Name, 28, on ? Color.white : Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(l.rectTransform);
            if (!on) Ui.MakeClickable(chip, () => { GameState.Subject = idx; Build(); });
        }
        // tab
        string[] tn = { "Teks", "Audio / Video", "Ilustrasi" };
        for (int i = 0; i < 3; i++) {
            int idx = i;
            tabImg[i] = Ui.Box(root, Color.white, "Tab" + i); Ui.At(tabImg[i].rectTransform, 0, 1, 60 + i * 330, -430, 300, 88);
            tabTx[i] = Ui.Label(tabImg[i].transform, tn[i], 32, Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(tabTx[i].rectTransform);
            Ui.MakeClickable(tabImg[i], () => { tab = idx; playing = false; Render(); });
        }
        Ui.BottomNav(root, 1);
        Render();
    }

    void Render() {
        for (int i = 0; i < 3; i++) {
            Ui.ColorTo(this, tabImg[i], i == tab ? sub.Color : Color.white);
            tabTx[i].color = i == tab ? Color.white : Pal.Ink;
        }
        if (body) Destroy(body.gameObject);
        body = Ui.Rect("Body", root); Ui.Fill(body);
        var card = Ui.Card(body, 0, 0, "Content"); var cr = card.rectTransform;
        cr.anchorMin = Vector2.zero; cr.anchorMax = Vector2.one; cr.offsetMin = new Vector2(60, 345); cr.offsetMax = new Vector2(-60, -545);
        Ui.Pop(this, card.transform, 0, .35f);

        if (tab == 0) TextTab(card.transform);
        else if (tab == 1) AudioTab(card);
        else CardsTab(card.transform);

        // kontrol bawah
        int n = sub.Pages.Length; bool pn = tab != 2;
        if (pn) {
            bool canPrev = page > 0, canNext = page < n - 1;
            var pv = Ui.Btn(body, "< Sebelumnya", canPrev ? Pal.Ink : Pal.Line, canPrev ? Color.white : Pal.Muted, 270, 110, () => { if (canPrev) { page--; playing = false; audioT = 0; Render(); } }, 30);
            Ui.At(pv.GetComponent<RectTransform>(), 0, 0, 60, 215, 270, 110);
            var nx = Ui.Btn(body, "Lanjut >", canNext ? Pal.Ink : Pal.Line, canNext ? Color.white : Pal.Muted, 270, 110, () => { if (canNext) { page++; playing = false; audioT = 0; Render(); } }, 30);
            Ui.At(nx.GetComponent<RectTransform>(), 1, 0, -60, 215, 270, 110);
            var qz = Ui.Btn(body, "Mulai Kuis", Pal.Accent, Color.white, 340, 110, () => Nav.Go(Nav.S4), 34);
            Ui.At(qz.GetComponent<RectTransform>(), .5f, 0, 0, 215, 340, 110);
        } else {
            var qz = Ui.Btn(body, "Mulai Kuis", Pal.Accent, Color.white, 960, 110, () => Nav.Go(Nav.S4), 36);
            Ui.At(qz.GetComponent<RectTransform>(), .5f, 0, 0, 215, 960, 110);
        }
    }

    void TextTab(Transform c) {
        int n = sub.Pages.Length;
        Image track; Ui.Bar(c, sub.Color, Pal.Line, (page + 1f) / n, out track); Ui.At(track.rectTransform, .5f, 1, 0, -45, 840, 18);
        var pc = Ui.Label(c, "Halaman " + (page + 1) + " dari " + n, 30, Pal.Muted, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(pc.rectTransform, 0, 1, 55, -105, 700, 50);
        var tt = Ui.Label(c, sub.Title(page), 56, sub.Color, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(tt.rectTransform, 0, 1, 55, -175, 810, 90);
        var bd = Ui.Label(c, sub.Body(page), 42, Pal.Ink, TextAnchor.UpperLeft); bd.lineSpacing = 1.35f; Ui.At(bd.rectTransform, 0, 1, 55, -300, 810, 600);
    }

    void AudioTab(Image card) {
        var c = card.transform;
        var screen = Ui.Box(c, Pal.Ink, "Screen"); Ui.At(screen.rectTransform, .5f, 1, 0, -50, 840, 440);
        var topic = Ui.Label(screen.transform, sub.Title(page), 38, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.At(topic.rectTransform, .5f, 1, 0, -30, 760, 70);
        var tag = Ui.Label(screen.transform, "Penjelasan audio / video", 26, new Color(1, 1, 1, .6f)); Ui.At(tag.rectTransform, .5f, 1, 0, -95, 760, 40);
        const int N = 17; var bars = new Image[N];
        for (int i = 0; i < N; i++) {
            bars[i] = Ui.Box(screen.transform, i % 2 == 0 ? sub.Color : Color.white, "Bar" + i);
            Ui.At(bars[i].rectTransform, .5f, .5f, (i - N / 2) * 44, -50, 24, 30);
        }
        Image track; var fill = Ui.Bar(c, sub.Color, Pal.Line, 0, out track); Ui.At(track.rectTransform, .5f, 1, 0, -540, 840, 22);
        var tm = Ui.Label(c, "00:00 / 01:30", 32, Pal.Muted, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.At(tm.rectTransform, .5f, 1, 0, -580, 840, 50);
        var pb = Ui.Box(c, sub.Color, "Play", true); Ui.At(pb.rectTransform, .5f, 1, 0, -670, 200, 200);
        var pl = Ui.Label(pb.transform, playing ? "Jeda" : "Putar", 42, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(pl.rectTransform);
        Ui.MakeClickable(pb, () => { playing = !playing; pl.text = playing ? "Jeda" : "Putar"; });
        var note = Ui.Label(c, "Ketuk tombol untuk memutar penjelasan materi", 28, Pal.Muted); Ui.At(note.rectTransform, .5f, 0, 0, 45, 840, 50);
        const float DUR = 90f;
        Ui.Loop(this, c.gameObject, t => {
            if (playing) { audioT += Time.unscaledDeltaTime; if (audioT >= DUR) { audioT = 0; playing = false; pl.text = "Putar"; } }
            for (int i = 0; i < N; i++) {
                float h = playing ? 40 + 150 * Mathf.Abs(Mathf.Sin(t * 4f + i * .75f)) : 30 + 10 * Mathf.Sin(t * 1.5f + i);
                bars[i].rectTransform.sizeDelta = new Vector2(24, h);
            }
            Ui.SetBar(fill, audioT / DUR);
            tm.text = string.Format("{0:00}:{1:00} / 01:30", (int)audioT / 60, (int)audioT % 60);
        });
    }

    void CardsTab(Transform c) {
        var h = Ui.Label(c, "Kartu Konsep", 52, sub.Color, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(h.rectTransform, 0, 1, 55, -55, 800, 80);
        var s = Ui.Label(c, "Ketuk kartu untuk membalik dan melihat ringkasan", 30, Pal.Muted, TextAnchor.MiddleLeft); Ui.At(s.rectTransform, 0, 1, 55, -135, 820, 50);
        int n = sub.Pages.Length; var open = new bool[n];
        var cnt = Ui.Label(c, "Kartu dibuka: 0/" + n, 30, Pal.Muted, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.At(cnt.rectTransform, .5f, 0, 0, 35, 800, 50);
        var light = sub.Color; light.a = .14f;
        for (int i = 0; i < n; i++) {
            int idx = i;
            var cd = Ui.Box(c, light, "Flip" + i); Ui.At(cd.rectTransform, .5f, 1, 0, -215 - i * 265, 840, 235);
            var tx = Ui.Label(cd.transform, sub.Title(i), 46, sub.Color, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(tx.rectTransform, 30, 50, 30, 20);
            var hint = Ui.Label(cd.transform, "Ketuk untuk membuka", 26, Pal.Muted); Ui.At(hint.rectTransform, .5f, 0, 0, 22, 700, 40);
            Ui.MakeClickable(cd, () => {
                open[idx] = !open[idx];
                int cnum = 0; foreach (var o in open) if (o) cnum++;
                cnt.text = "Kartu dibuka: " + cnum + "/" + n;
                StartCoroutine(Flip(cd, tx, hint, open[idx], idx, light));
            });
            Ui.Pop(this, cd.transform, i * .1f);
        }
    }

    IEnumerator Flip(Image bg, Text tx, Text hint, bool toBack, int i, Color light) {
        var rt = bg.rectTransform;
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / .15f) { if (!rt) yield break; rt.localScale = new Vector3(1 - k, 1, 1); yield return null; }
        tx.text = toBack ? sub.Summary(i) : sub.Title(i); tx.fontSize = toBack ? 36 : 46;
        tx.color = toBack ? Color.white : sub.Color; bg.color = toBack ? sub.Color : light;
        hint.text = toBack ? "Ketuk untuk menutup" : "Ketuk untuk membuka"; hint.color = toBack ? new Color(1, 1, 1, .75f) : Pal.Muted;
        for (float k = 0; k < 1; k += Time.unscaledDeltaTime / .15f) { if (!rt) yield break; rt.localScale = new Vector3(k, 1, 1); yield return null; }
        if (rt) rt.localScale = Vector3.one;
    }
}
