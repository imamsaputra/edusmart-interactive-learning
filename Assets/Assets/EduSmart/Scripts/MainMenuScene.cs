using UnityEngine;
using UnityEngine.UI;

// Scene_02_MainMenuDashboard : pusat navigasi utama
public class MainMenuScene : MonoBehaviour {
    void Start() {
        var root = Ui.MakeCanvas("Canvas").transform;
        Ui.Solid(root, Pal.Bg, "Bg");
        Ui.Header(root, "Halo, " + GameState.PlayerName + "!", "Mau belajar apa hari ini?", Pal.Primary, null, 520);

        var av = Ui.Box(root, Color.white, "Avatar", true); Ui.At(av.rectTransform, 1, 1, -60, -70, 120, 120);
        var al = Ui.Label(av.transform, GameState.PlayerName.Substring(0, 1).ToUpper(), 60, Pal.Primary, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(al.rectTransform);
        Ui.MakeClickable(av, () => Nav.Go(Nav.S5));

        // kartu level & XP
        var xp = Ui.Card(root, 960, 220, "XPCard"); Ui.At(xp.rectTransform, .5f, 1, 0, -340, 960, 220);
        var lv = Ui.Label(xp.transform, "Level " + GameState.Level, 50, Pal.Primary, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(lv.rectTransform, 0, 1, 45, -30, 400, 70);
        var xl = Ui.Label(xp.transform, (GameState.XP % 100) + " / 100 XP", 34, Pal.Muted, TextAnchor.MiddleRight); Ui.At(xl.rectTransform, 1, 1, -45, -38, 400, 60);
        Image track; var fill = Ui.Bar(xp.transform, Pal.Accent, Pal.Line, 0, out track); Ui.At(track.rectTransform, .5f, 0, 0, 80, 870, 26);
        var hint = Ui.Label(xp.transform, "Selesaikan kuis untuk naik level", 28, Pal.Muted, TextAnchor.MiddleLeft); Ui.At(hint.rectTransform, 0, 0, 45, 25, 700, 40);
        float target = (GameState.XP % 100) / 100f;
        Ui.Tween(this, .8f, k => Ui.SetBar(fill, target * k), .3f);
        Ui.Pop(this, xp.transform, .1f);

        // daftar konten
        var sv = Ui.Scroll(root, 60, 28); Ui.Fill(sv, 0, 220, 0, 600);
        var c = Ui.Content(sv);

        var promo = Ui.Box(c, Pal.Warm, "Promo"); Ui.LE(promo, 230);
        var pt = Ui.Label(promo.transform, "Tantangan Harian", 46, Pal.Ink, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(pt.rectTransform, 0, 1, 40, -35, 560, 70);
        var ps = Ui.Label(promo.transform, "Kerjakan 1 kuis dan raih XP tambahan!", 30, Pal.Ink, TextAnchor.UpperLeft); Ui.At(ps.rectTransform, 0, 1, 40, -110, 560, 100);
        var pb = Ui.Btn(promo.transform, "Mulai", Pal.Ink, Color.white, 220, 100, () => Nav.Go(Nav.S4), 38);
        Ui.At(pb.GetComponent<RectTransform>(), 1, .5f, -40, 0, 220, 100);
        Ui.Pop(this, promo.transform, .2f);

        var sec = Ui.Label(c, "Mata Pelajaran", 46, Pal.Ink, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.LE(sec, 70);

        var grid = Ui.Rect("Grid", c);
        var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(465, 420); gl.spacing = new Vector2(30, 30); gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 2;
        int rows = (Data.All.Length + 1) / 2; Ui.LE(grid, rows * 420 + (rows - 1) * 30);
        for (int i = 0; i < Data.All.Length; i++) {
            int idx = i; var s = Data.All[i];
            var card = Ui.Card(grid, 465, 420, s.Name);
            var ic = Ui.Box(card.transform, s.Color, "Icon", true); Ui.At(ic.rectTransform, 0, 1, 35, -35, 130, 130);
            var il = Ui.Label(ic.transform, s.Letter, 70, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(il.rectTransform);
            var nm = Ui.Label(card.transform, s.Name, 42, Pal.Ink, TextAnchor.MiddleLeft, FontStyle.Bold); Ui.At(nm.rectTransform, 0, 1, 35, -185, 410, 60);
            var inf = Ui.Label(card.transform, s.Pages.Length + " materi  -  " + s.Quiz.Length + " soal", 28, Pal.Muted, TextAnchor.MiddleLeft); Ui.At(inf.rectTransform, 0, 1, 35, -250, 410, 45);
            var tint = s.Color; tint.a = .15f;
            var chip = Ui.Box(card.transform, tint, "Chip"); Ui.At(chip.rectTransform, 0, 0, 35, 35, 190, 66);
            var cl = Ui.Label(chip.transform, "Belajar", 30, s.Color, TextAnchor.MiddleCenter, FontStyle.Bold); Ui.Fill(cl.rectTransform);
            Ui.MakeClickable(card, () => { GameState.Subject = idx; Nav.Go(Nav.S3); });
            Ui.Pop(this, card.transform, .3f + i * .08f);
        }
        Ui.BottomNav(root, 0);
    }
}
