using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Scene_05_LeaderboardProfile : profil siswa & papan peringkat
public class LeaderboardScene : MonoBehaviour {
    class Row { public string N; public int XP; public bool Me; }

    void Start() {
        var root = Ui.MakeCanvas("Canvas").transform;
        Ui.Solid(root, Pal.Bg, "Bg");
        Ui.Header(root, "Profil & Peringkat", "Pantau progres belajarmu", Pal.Primary, Nav.S2);

        string name = GameState.PlayerName;
        var card = Ui.Card(root, 960, 290, "Profile"); Ui.At(card.rectTransform, .5f, 1, 0, -240, 960, 290);
        var av = Ui.Box(card.transform, Pal.Primary, "Avatar", true); Ui.At(av.rectTransform, 0, 1, 40, -40, 150, 150);
        var al = Ui.Label(av.transform, name.Substring(0, 1).ToUpper(), 76, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(al.rectTransform);
        var nm = Ui.Label(card.transform, name, 52, Pal.Ink, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(nm.rectTransform, 0, 1, 220, -40, 700, 70);
        var st = Ui.Label(card.transform, "Level " + GameState.Level + "  |  " + GameState.XP + " XP  |  " + GameState.Quizzes + " kuis", 32, Pal.Muted, TextAnchor.MiddleLeft);
        Ui.At(st.rectTransform, 0, 1, 220, -115, 700, 50);
        Image track; var fill = Ui.Bar(card.transform, Pal.Accent, Pal.Line, 0, out track); Ui.At(track.rectTransform, 0, 0, 40, 75, 880, 24);
        var nx = Ui.Label(card.transform, (100 - GameState.XP % 100) + " XP lagi menuju Level " + (GameState.Level + 1), 28, Pal.Muted, TextAnchor.MiddleLeft);
        Ui.At(nx.rectTransform, 0, 0, 40, 25, 880, 40);
        float target = (GameState.XP % 100) / 100f;
        Ui.Tween(this, .8f, k => Ui.SetBar(fill, target * k), .3f);
        Ui.Pop(this, card.transform, .1f);

        var sec = Ui.Label(root, "Papan Peringkat", 46, Pal.Ink, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(sec.rectTransform, 0, 1, 60, -585, 700, 70);

        var rows = new List<Row> {
            new Row { N = "Alya", XP = 480 }, new Row { N = "Raka", XP = 420 }, new Row { N = "Sinta", XP = 390 },
            new Row { N = "Dimas", XP = 340 }, new Row { N = "Nadia", XP = 280 }, new Row { N = "Fajar", XP = 210 },
            new Row { N = "Bima", XP = 150 }, new Row { N = name, XP = GameState.XP, Me = true } };
        rows.Sort((a, b) => b.XP.CompareTo(a.XP));

        var sv = Ui.Scroll(root, 60, 20); Ui.Fill(sv, 0, 220, 0, 665); var c = Ui.Content(sv);
        Color[] medal = { Pal.Hex("FAB005"), Pal.Hex("ADB5BD"), Pal.Hex("E8590C") };
        var mine = Pal.Primary; mine.a = .12f;
        for (int i = 0; i < rows.Count; i++) {
            var r = rows[i];
            var row = Ui.Card(c, 960, 130, "Row" + i); Ui.LE(row, 130);
            if (r.Me) row.color = Color.Lerp(Color.white, Pal.Primary, .12f);
            var rk = Ui.Box(row.transform, i < 3 ? medal[i] : Pal.Line, "Rank", true); Ui.At(rk.rectTransform, 0, .5f, 30, 0, 76, 76);
            var rl = Ui.Label(rk.transform, (i + 1).ToString(), 38, i < 3 ? Color.white : Pal.Ink, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(rl.rectTransform);
            var ab = Ui.Box(row.transform, r.Me ? Pal.Primary : Pal.Hex("CED6E8"), "Av", true); Ui.At(ab.rectTransform, 0, .5f, 130, 0, 80, 80);
            var ax = Ui.Label(ab.transform, r.N.Substring(0, 1).ToUpper(), 38, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(ax.rectTransform);
            var nl = Ui.Label(row.transform, r.Me ? r.N + " (Kamu)" : r.N, 38, Pal.Ink, TextAnchor.MiddleLeft, r.Me ? FontStyle.Bold : FontStyle.Normal);
            Ui.At(nl.rectTransform, 0, .5f, 240, 0, 460, 60);
            var xl = Ui.Label(row.transform, r.XP + " XP", 38, Pal.Primary, TextAnchor.MiddleRight, FontStyle.Bold); Ui.At(xl.rectTransform, 1, .5f, -35, 0, 220, 60);
            Ui.Pop(this, row.transform, .2f + i * .07f);
        }
        var reset = Ui.Btn(c, "Reset Progres", Pal.Line, Pal.Danger, 960, 100, () => { GameState.XP = 0; GameState.Quizzes = 0; Nav.Go(Nav.S5); }, 32);
        Ui.LE(reset, 100);
        Ui.BottomNav(root, 3);
    }
}
