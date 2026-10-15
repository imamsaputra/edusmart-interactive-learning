using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ===== Palet warna (simple & elegan, tema edukasi) =====
public static class Pal {
    public static readonly Color Bg = Hex("F4F7FC"), Primary = Hex("3B5BDB"), Accent = Hex("12B886"),
        Warm = Hex("FAB005"), Danger = Hex("F03E3E"), Ink = Hex("1F2A44"), Muted = Hex("7C879E"), Line = Hex("E3E8F2");
    public static Color Hex(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }
}

// ===== Data pemain (tersimpan di PlayerPrefs) =====
public static class GameState {
    public static string PlayerName { get { return PlayerPrefs.GetString("name", "Siswa"); } set { PlayerPrefs.SetString("name", value); } }
    public static int XP { get { return PlayerPrefs.GetInt("xp", 0); } set { PlayerPrefs.SetInt("xp", value); } }
    public static int Quizzes { get { return PlayerPrefs.GetInt("quizzes", 0); } set { PlayerPrefs.SetInt("quizzes", value); } }
    public static int Level { get { return XP / 100 + 1; } }
    public static int Subject; // index mata pelajaran aktif
}

// ===== Konten pelajaran =====
public class Q {
    public string Text, Why; public string[] Opt; public int Ans;
    public Q(string t, string[] o, int a, string w) { Text = t; Opt = o; Ans = a; Why = w; }
}

public class Subject {
    public string Name, Letter; public Color Color; public string[] Pages; public Q[] Quiz;
    public string Title(int i) { var p = Pages[i]; int k = p.IndexOf("\n\n"); return k < 0 ? p : p.Substring(0, k); }
    public string Body(int i) { var p = Pages[i]; int k = p.IndexOf("\n\n"); return k < 0 ? "" : p.Substring(k + 2); }
    public string Summary(int i) { var b = Body(i); int k = b.IndexOf('.'); return k < 0 ? b : b.Substring(0, k + 1); }
}

public static class Data {
    public static readonly Subject[] All = {
        new Subject { Name = "Matematika", Letter = "M", Color = Pal.Primary,
            Pages = new[] {
                "Bilangan Pecahan\n\nPecahan menyatakan bagian dari keseluruhan. Contoh: 1/2 berarti satu bagian dari dua bagian yang sama besar. Angka di atas disebut pembilang, angka di bawah disebut penyebut.",
                "Menjumlahkan Pecahan\n\nJika penyebut sama, cukup jumlahkan pembilangnya. Contoh: 2/5 + 1/5 = 3/5. Jika penyebut berbeda, samakan dahulu penyebutnya sebelum dijumlahkan.",
                "Persen dan Desimal\n\n1/2 = 0,5 = 50%. Pecahan dapat diubah menjadi desimal dengan membagi pembilang dengan penyebut, lalu dikali 100 untuk menjadi persen." },
            Quiz = new[] {
                new Q("Hasil dari 2/5 + 1/5 adalah...", new[]{"3/10","3/5","2/5","1/5"}, 1, "Penyebut sama, jadi pembilang dijumlahkan: 2 + 1 = 3 sehingga 3/5."),
                new Q("1/2 sama dengan berapa persen?", new[]{"25%","50%","75%","100%"}, 1, "1 dibagi 2 = 0,5 dan 0,5 x 100 = 50%."),
                new Q("Bagian atas dari sebuah pecahan disebut...", new[]{"Penyebut","Pembilang","Desimal","Persen"}, 1, "Angka di atas garis pecahan adalah pembilang.") } },
        new Subject { Name = "IPA", Letter = "S", Color = Pal.Accent,
            Pages = new[] {
                "Fotosintesis\n\nTumbuhan hijau membuat makanan sendiri dengan bantuan cahaya matahari, air, dan karbon dioksida. Hasilnya adalah glukosa dan oksigen.",
                "Rantai Makanan\n\nEnergi berpindah dari produsen ke konsumen. Contoh: rumput -> belalang -> katak -> ular. Produsen adalah makhluk hidup yang membuat makanan sendiri.",
                "Siklus Air\n\nAir laut menguap, uap air membentuk awan (kondensasi), lalu turun sebagai hujan (presipitasi) dan kembali mengalir ke laut." },
            Quiz = new[] {
                new Q("Gas apa yang dihasilkan oleh fotosintesis?", new[]{"Oksigen","Nitrogen","Hidrogen","Metana"}, 0, "Fotosintesis menghasilkan glukosa dan oksigen."),
                new Q("Produsen dalam rantai makanan adalah...", new[]{"Ular","Katak","Tumbuhan","Belalang"}, 2, "Tumbuhan hijau membuat makanan sendiri sehingga menjadi produsen."),
                new Q("Uap air berubah menjadi awan melalui proses...", new[]{"Presipitasi","Kondensasi","Infiltrasi","Erosi"}, 1, "Pendinginan uap air menjadi titik air disebut kondensasi.") } },
        new Subject { Name = "B. Indonesia", Letter = "B", Color = Pal.Hex("F76707"),
            Pages = new[] {
                "Unsur Kalimat\n\nKalimat memiliki unsur subjek dan predikat. Contoh: 'Adik membaca buku.' Subjek: adik, predikat: membaca, objek: buku.",
                "Ide Pokok Paragraf\n\nIde pokok adalah gagasan utama yang dibahas dalam paragraf. Biasanya terdapat pada kalimat utama di awal atau di akhir paragraf.",
                "Kata Baku\n\nKata baku adalah kata yang sesuai kaidah KBBI. Contoh: apotek (baku), apotik (tidak baku); analisis (baku), analisa (tidak baku)." },
            Quiz = new[] {
                new Q("Unsur wajib dalam sebuah kalimat adalah...", new[]{"Subjek dan predikat","Objek dan keterangan","Pelengkap","Konjungsi"}, 0, "Kalimat minimal terdiri atas subjek dan predikat."),
                new Q("Manakah penulisan kata baku yang benar?", new[]{"Apotik","Apotek","Apotic","Appotek"}, 1, "Menurut KBBI, bentuk bakunya adalah 'apotek'."),
                new Q("Gagasan utama dalam paragraf disebut...", new[]{"Ide pokok","Judul","Kesimpulan","Ilustrasi"}, 0, "Gagasan utama paragraf disebut ide pokok.") } },
        new Subject { Name = "B. Inggris", Letter = "E", Color = Pal.Hex("9775FA"),
            Pages = new[] {
                "Greetings\n\nGood morning (pagi), good afternoon (siang), good evening (sore/malam). Reply with: Good morning! How are you?",
                "Simple Present\n\nUsed for habits and facts. Formula: S + V1 (+s/es for he, she, it). Example: She reads a book every day.",
                "Plural Nouns\n\nAdd -s to most nouns: book -> books. Add -es after s, x, ch, sh: box -> boxes, watch -> watches." },
            Quiz = new[] {
                new Q("Good ..., Sir! (said at 8 AM)", new[]{"evening","morning","night","afternoon"}, 1, "8 AM is in the morning, so we say 'Good morning'."),
                new Q("She ... to school every day.", new[]{"go","goes","going","gone"}, 1, "With she/he/it in simple present, add -es: goes."),
                new Q("What is the plural of 'box'?", new[]{"boxs","boxes","boxen","boxies"}, 1, "Nouns ending in -x take -es: boxes.") } }
    };
}

// ===== Navigasi scene dengan transisi fade =====
public static class Nav {
    public const string S1 = "Scene_01_Welcome", S2 = "Scene_02_MainMenuDashboard", S3 = "Scene_03_InteractiveMateri",
        S4 = "Scene_04_QuizEvaluasi", S5 = "Scene_05_LeaderboardProfile";
    static NavRunner runner; static Image veil; static bool busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { busy = false; runner = null; veil = null; }

    static Color A(float a) { var c = Pal.Primary; c.a = a; return c; }

    public static void Go(string scene) {
        if (busy) return;
        if (runner == null) {
            var c = Ui.MakeCanvas("TransitionCanvas", false); c.sortingOrder = 999;
            Object.DontDestroyOnLoad(c.gameObject);
            runner = c.gameObject.AddComponent<NavRunner>();
            veil = Ui.Solid(c.transform, Pal.Primary, "Veil"); veil.color = A(0); veil.raycastTarget = false;
        }
        runner.StartCoroutine(Co(scene));
    }

    static IEnumerator Co(string scene) {
        busy = true; veil.raycastTarget = true;
        yield return Ui.TweenCo(.28f, k => veil.color = A(k), 0, x => x);
        var op = SceneManager.LoadSceneAsync(scene);
        while (!op.isDone) yield return null;
        yield return null;
        yield return Ui.TweenCo(.35f, k => veil.color = A(1 - k), 0, x => x);
        veil.raycastTarget = false; busy = false;
    }
}
