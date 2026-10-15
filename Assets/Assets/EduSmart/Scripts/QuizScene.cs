using UnityEngine;
using UnityEngine.UI;

// Scene_04_QuizEvaluasi : ujian/latihan soal interaktif
public class QuizScene : MonoBehaviour {
    Transform root; RectTransform body; Subject sub; int qi, score; bool answered;
    Image[] optBg = new Image[4], optLt = new Image[4]; Text[] optTx = new Text[4], optLtx = new Text[4]; Text feedback;

    void Start() {
        sub = Data.All[GameState.Subject];
        root = Ui.MakeCanvas("Canvas").transform;
        Ui.Solid(root, Pal.Bg, "Bg");
        Ui.Header(root, "Kuis", sub.Name, sub.Color, Nav.S2);
        ShowQuestion();
    }

    RectTransform NewBody() { if (body) Destroy(body.gameObject); body = Ui.Rect("Body", root); Ui.Fill(body); return body; }

    void ShowQuestion() {
        answered = false; var b = NewBody(); var q = sub.Quiz[qi]; int n = sub.Quiz.Length;
        var l1 = Ui.Label(b, "Soal " + (qi + 1) + " dari " + n, 34, Pal.Muted, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(l1.rectTransform, 0, 1, 60, -325, 500, 50);
        var l2 = Ui.Label(b, "Skor: " + score, 34, sub.Color, TextAnchor.MiddleRight, FontStyle.Bold); Ui.At(l2.rectTransform, 1, 1, -60, -325, 400, 50);
        Image track; var fill = Ui.Bar(b, sub.Color, Pal.Line, (float)qi / n, out track); Ui.At(track.rectTransform, .5f, 1, 0, -385, 960, 22);
        float a = (float)qi / n, z = (float)(qi + 1) / n;
        Ui.Tween(this, .5f, k => Ui.SetBar(fill, Mathf.Lerp(a, z, k)));

        var qc = Ui.Card(b, 960, 340, "Question"); Ui.At(qc.rectTransform, .5f, 1, 0, -440, 960, 340);
        var qt = Ui.Label(qc.transform, q.Text, 48, Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(qt.rectTransform, 40, 30, 40, 30);
        Ui.Pop(this, qc.transform);

        string[] L = { "A", "B", "C", "D" };
        for (int i = 0; i < 4; i++) {
            int idx = i;
            var o = Ui.Card(b, 960, 140, "Option" + i); Ui.At(o.rectTransform, .5f, 1, 0, -810 - i * 160, 960, 140);
            optBg[i] = o;
            optLt[i] = Ui.Box(o.transform, Pal.Line, "Letter", true); Ui.At(optLt[i].rectTransform, 0, .5f, 30, 0, 80, 80);
            optLtx[i] = Ui.Label(optLt[i].transform, L[i], 40, Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(optLtx[i].rectTransform);
            optTx[i] = Ui.Label(o.transform, q.Opt[i], 38, Pal.Ink, TextAnchor.MiddleLeft); Ui.Fill(optTx[i].rectTransform, 140, 10, 30, 10);
            Ui.MakeClickable(o, () => Answer(idx));
            Ui.Pop(this, o.transform, .15f + i * .08f);
        }
        feedback = Ui.Label(b, "", 34, Pal.Ink, TextAnchor.UpperLeft); Ui.At(feedback.rectTransform, .5f, 1, 0, -1455, 940, 170);
    }

    void Answer(int i) {
        if (answered) return; answered = true;
        var q = sub.Quiz[qi]; bool ok = i == q.Ans; int n = sub.Quiz.Length;
        if (ok) score++;
        Ui.ColorTo(this, optBg[i], ok ? Pal.Accent : Pal.Danger);
        optTx[i].color = Color.white; optLtx[i].color = Color.white; optLt[i].color = new Color(1, 1, 1, .3f);
        if (ok) Ui.Bump(this, optBg[i].transform);
        else {
            Ui.ColorTo(this, optBg[q.Ans], Pal.Hex("C3F0E0"));
            Ui.Shake(this, optBg[i].rectTransform);
        }
        feedback.text = (ok ? "Benar! " : "Belum tepat. ") + q.Why;
        feedback.color = ok ? Pal.Hex("0B7A5A") : Pal.Hex("C92A2A");
        var nb = Ui.Btn(body, qi + 1 >= n ? "Lihat Hasil" : "Soal Berikutnya", sub.Color, Color.white, 960, 130, () => {
            qi++; if (qi >= n) ShowResult(); else ShowQuestion();
        }, 42);
        Ui.At(nb.GetComponent<RectTransform>(), .5f, 0, 0, 70, 960, 130);
        Ui.Pop(this, nb.transform);
    }

    void ShowResult() {
        var b = NewBody(); int n = sub.Quiz.Length; int pct = score * 100 / n;
        int gain = score * 10 + (score == n ? 20 : 0);
        GameState.XP += gain; GameState.Quizzes++;
        var circ = Ui.Box(b, sub.Color, "Score", true); Ui.At(circ.rectTransform, .5f, 1, 0, -400, 380, 380);
        var pt = Ui.Label(circ.transform, "0%", 110, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(pt.rectTransform);
        Ui.Pop(this, circ.transform, 0, .7f);
        Ui.Tween(this, 1f, k => pt.text = Mathf.RoundToInt(k * pct) + "%", .3f);
        string msg = pct >= 100 ? "Sempurna!" : pct >= 60 ? "Bagus sekali!" : "Ayo coba lagi!";
        var m = Ui.Label(b, msg, 72, Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.At(m.rectTransform, .5f, 1, 0, -850, 960, 100);
        var d = Ui.Label(b, "Kamu menjawab " + score + " dari " + n + " soal dengan benar", 36, Pal.Muted); Ui.At(d.rectTransform, .5f, 1, 0, -955, 960, 60);
        var xp = Ui.Box(b, Pal.Warm, "XPChip"); Ui.At(xp.rectTransform, .5f, 1, 0, -1035, 320, 90);
        var xl = Ui.Label(xp.transform, "+" + gain + " XP", 44, Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(xl.rectTransform);
        Ui.Pop(this, m.transform, .4f); Ui.Pop(this, xp.transform, .6f);

        var r1 = Ui.Btn(b, "Ulangi Kuis", sub.Color, Color.white, 960, 130, () => { qi = 0; score = 0; ShowQuestion(); }, 42); Ui.At(r1.GetComponent<RectTransform>(), .5f, 1, 0, -1200, 960, 130);
        var r2 = Ui.Btn(b, "Lihat Leaderboard", Pal.Accent, Color.white, 960, 130, () => Nav.Go(Nav.S5), 42); Ui.At(r2.GetComponent<RectTransform>(), .5f, 1, 0, -1350, 960, 130);
        var r3 = Ui.Btn(b, "Kembali ke Menu", Pal.Line, Pal.Ink, 960, 130, () => Nav.Go(Nav.S2), 42); Ui.At(r3.GetComponent<RectTransform>(), .5f, 1, 0, -1500, 960, 130);
        Ui.Pop(this, r1.transform, .7f); Ui.Pop(this, r2.transform, .8f); Ui.Pop(this, r3.transform, .9f);
    }
}
