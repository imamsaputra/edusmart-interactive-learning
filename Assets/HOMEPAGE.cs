using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// EduSmart Learning - Homepage (uGUI, dibangun lewat kode).
///
/// CARA PAKAI
/// 1. Taruh file ini di Assets/.
/// 2. Salin folder "Resources" (dari EduSmart_Assets.zip) ke Assets/  ->  Assets/Resources/EduSmart/*.png
/// 3. Buat Empty GameObject -> Add Component "EduSmartHomepage".
///    Desain langsung tampil di Game view TANPA Play (Edit Mode). Klik kanan komponen -> "Rebuild UI" untuk refresh manual.
///
/// Slot sprite di Inspector OPSIONAL. Jika kosong, gambar diambil otomatis dari Resources/EduSmart.
/// Layout otomatis menyesuaikan tinggi layar dan safe area (notch).
/// </summary>
[ExecuteAlways]
public class EduSmartHomepage : MonoBehaviour
{
    [Header("Sprite (kosong = otomatis dari Resources/EduSmart)")]
    public Sprite logo;
    public Sprite heroIllustration;
    public Sprite promoIllustration;
    public Sprite iconVideo, iconQuiz, iconProgress, iconCertificate;
    public Sprite[] subjectIcons = new Sprite[5];   // IPA, Fisika, Matematika, Biologi, B. Inggris
    public Sprite[] navIcons = new Sprite[4];       // Beranda, Materi, Progress, Profil (siluet putih)

    [Header("Pengaturan")]
    public string resourcesFolder = "EduSmart";
    [Tooltip("Aktif jika sprite ikon sudah termasuk lingkaran/kotak latarnya.")]
    public bool iconsIncludeBackground = true;

    /// <summary>Dipanggil saat tombol ditekan. Contoh id: "mulai_belajar", "video", "subject_IPA", "nav_Materi".</summary>
    public event Action<string> OnItemClicked;

    // ===== Warna =====
    static readonly Color Navy     = Hex("1B2F7A");
    static readonly Color Blue     = Hex("1F55B8");
    static readonly Color BlueSoft = Hex("DCEBFF");
    static readonly Color Orange   = Hex("FF6B1A");
    static readonly Color Peach    = Hex("FFE8D6");
    static readonly Color HeroBg   = Hex("F1F7FE");
    static readonly Color PromoBg  = Hex("1C509F");
    static readonly Color PageBg   = Hex("F7FAFF");
    static readonly Color Gray     = Hex("5B6B8C");
    static readonly Color GrayLite = Hex("8A94A8");
    static readonly Color NavOff   = Hex("B0BBD0");

    // ===== Ukuran desain (lebar referensi 1024) =====
    const float DesignW = 1024f;
    const float HeaderH = 130f;
    const float HeroH   = 472f;
    const float MenuH   = 212f;
    const float PromoH  = 226f;
    const float SubjH   = 205f;
    const float NavBase = 120f;

    Font font;
    RectTransform root, page, content;
    readonly Dictionary<int, Sprite> roundCache = new Dictionary<int, Sprite>();
    readonly Dictionary<string, Sprite> resCache = new Dictionary<string, Sprite>();
    readonly List<Image> navIconImgs = new List<Image>();
    readonly List<Text> navLabels = new List<Text>();
    readonly List<Image> navIndicators = new List<Image>();

    const string CanvasName = "EduSmartCanvas";

    int lastW, lastH;
    Rect lastSafe;
    bool dirty;

    // =====================================================================
    //  SIKLUS HIDUP (berjalan juga di Edit Mode, tanpa Play)
    // =====================================================================
    void OnEnable()
    {
        font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Helvetica", "Roboto", "Noto Sans" }, 32);
        if (Application.isPlaying) EnsureEventSystem();
        RebuildNow();
    }

    void OnDisable() { Cleanup(); }

    void OnValidate() { dirty = true; }

    [ContextMenu("Rebuild UI")]
    void RebuildNow()
    {
        Cleanup();
        CreateCanvas();
        Rebuild();
    }

    void Update()
    {
        if (root == null) return;
        if (dirty || Screen.width != lastW || Screen.height != lastH || Screen.safeArea != lastSafe)
        {
            dirty = false;
            Rebuild();
        }
    }

    void Cleanup()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c != null && c.name == CanvasName)
            {
                c.gameObject.SetActive(false);
                SafeDestroy(c.gameObject);
            }
        }
        root = null; page = null; content = null;
    }

    static void SafeDestroy(UnityEngine.Object o)
    {
        if (o == null) return;
        if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
    }

    // UI hasil generate tidak ikut tersimpan di scene (dibuat ulang otomatis)
    void ApplyFlags()
    {
        if (root == null) return;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.hideFlags = HideFlags.DontSave;
    }

    void CreateCanvas()
    {
        var canvasGO = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(DesignW, 1536);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;     // cocokkan lebar

        root = canvasGO.GetComponent<RectTransform>();
        var bg = NewImage(root, "Background", PageBg, 0);
        Stretch(bg.rectTransform);
        ApplyFlags();
    }

    // =====================================================================
    //  BUILD
    // =====================================================================
    void Rebuild()
    {
        if (root == null) return;
        lastW = Screen.width; lastH = Screen.height; lastSafe = Screen.safeArea;
        if (page != null) { page.gameObject.SetActive(false); SafeDestroy(page.gameObject); }
        navIconImgs.Clear(); navLabels.Clear(); navIndicators.Clear();

        var pg = new GameObject("Page", typeof(RectTransform));
        pg.transform.SetParent(root, false);
        page = pg.GetComponent<RectTransform>();
        Stretch(page);

        // --- ukuran layar dalam satuan canvas + safe area ---
        float sw = Mathf.Max(1, Screen.width), sh = Mathf.Max(1, Screen.height);
        float H = DesignW * sh / sw;            // tinggi canvas (match width)
        float k = H / sh;
        Rect sa = Screen.safeArea;
        float topInset = Mathf.Max(0f, (Screen.height - sa.yMax) * k);
        float botInset = Mathf.Max(0f, sa.yMin * k);

        float navH = NavBase + botInset;
        float fixedH = HeaderH + HeroH + MenuH + PromoH + SubjH;
        float avail = H - topInset - navH - 10f;
        float gap = Mathf.Clamp((avail - fixedH) / 5f, 18f, 64f);

        // --- scroll view ---
        var sv = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        sv.transform.SetParent(page, false);
        Stretch(sv.GetComponent<RectTransform>());

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(sv.transform, false);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewport.transform, false);
        content = contentGO.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;

        var scroll = sv.GetComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 40;

        // --- susun section dari atas ke bawah ---
        float y = topInset + 10f;
        BuildHeader(y);                 y += HeaderH + gap;
        BuildHero(y);                   y += HeroH + gap;
        BuildQuickMenu(y);              y += MenuH + gap;
        BuildPromo(y);                  y += PromoH + gap;
        BuildSubjects(y);               y += SubjH;

        content.sizeDelta = new Vector2(0, Mathf.Max(H, y + navH + 20f));

        BuildBottomNav(page, navH);
        ApplyFlags();
    }

    // ---------------------------------------------------------------- HEADER
    void BuildHeader(float y)
    {
        Sprite logoSp = Pick(logo, "logo");
        if (logoSp != null)
        {
            float lw = 380f;
            float lh = lw * logoSp.rect.height / logoSp.rect.width;
            var img = NewImage(content, "Logo", Color.white, 0);
            img.sprite = logoSp;
            img.raycastTarget = false;
            SetRect(img.rectTransform, 100, y + (HeaderH - lh) / 2f, lw, lh);
        }
        else
        {
            Label(content, "edu", 120, y + 15, 140, 90, 76, Navy, FontStyle.Bold, TextAnchor.MiddleLeft);
            Label(content, "Smart", 252, y + 15, 260, 90, 76, Orange, FontStyle.BoldAndItalic, TextAnchor.MiddleLeft);
            Label(content, "Helping All Kids Be Great At Science", 215, y + 95, 330, 30, 17, Navy, FontStyle.Italic, TextAnchor.MiddleLeft);
        }

        // Lonceng notifikasi
        float cy = y + (HeaderH - 44f) / 2f;
        var bell = Panel(content, "Bell", 785, cy, 44, 44, Color.clear, 0);
        Panel(bell.transform, "BellBody", 6, 4, 32, 30, Blue, 15);
        Panel(bell.transform, "BellBase", 2, 32, 40, 5, Blue, 3);
        Panel(bell.transform, "BellClapper", 17, 38, 10, 6, Blue, 3);
        Panel(bell.transform, "Dot", 30, -4, 18, 18, Orange, -1);
        AddClick(bell.gameObject, "notifikasi");

        // Hamburger
        var burger = Panel(content, "Menu", 884, cy + 2, 48, 40, Color.clear, 0);
        for (int i = 0; i < 3; i++) Panel(burger.transform, "bar" + i, 0, 4 + i * 14, 46, 5, Blue, -1);
        AddClick(burger.gameObject, "menu");
    }

    // ------------------------------------------------------------------ HERO
    void BuildHero(float y)
    {
        var card = Panel(content, "HeroCard", 58, y, 910, HeroH, HeroBg, 28);
        AddShadow(card, 0.10f);

        Sprite heroSp = Pick(heroIllustration, "hero");
        if (heroSp != null)
        {
            var ill = NewImage(card.transform, "HeroIllustration", Color.white, 0);
            ill.sprite = heroSp;
            ill.raycastTarget = false;
            SetRect(ill.rectTransform, 422, 0, 488, HeroH);   // sudah termasuk sudut membulat
        }
        else
        {
            Panel(card.transform, "Placeholder", 470, 120, 400, 300, Hex("C9DBFF"), 40);
            Label(card.transform, "Ilustrasi Hero", 470, 120, 400, 300, 28, Gray, FontStyle.Italic, TextAnchor.MiddleCenter);
        }

        Label(card.transform, "SELAMAT DATANG DI", 66, 42, 420, 40, 24, Orange, FontStyle.Bold, TextAnchor.MiddleLeft);
        Label(card.transform, "EduSmart", 66, 80, 440, 84, 70, Navy, FontStyle.Bold, TextAnchor.MiddleLeft);
        Label(card.transform, "Learning", 66, 141, 440, 84, 70, Orange, FontStyle.Bold, TextAnchor.MiddleLeft);
        Label(card.transform,
            "Platform belajar interaktif yang membantu kamu memahami sains dengan cara yang lebih mudah, menyenangkan, dan efektif.",
            66, 232, 360, 130, 24, Gray, FontStyle.Normal, TextAnchor.UpperLeft);

        var btn = Panel(card.transform, "BtnMulai", 66, 372, 243, 58, Orange, -1);
        AddShadow(btn, 0.25f);
        Label(btn.transform, "Mulai Belajar  →", 0, 0, 243, 58, 26, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        AddClick(btn.gameObject, "mulai_belajar");
    }

    // ------------------------------------------------------------ QUICK MENU
    void BuildQuickMenu(float y)
    {
        var card = Panel(content, "QuickMenu", 60, y, 906, MenuH, Color.white, 28);
        AddShadow(card, 0.08f);

        string[] names = { "Video\nPembelajaran", "Latihan Soal\n& Kuis", "Progress\nBelajar", "Sertifikat\nPenyelesaian" };
        string[] ids = { "video", "latihan_soal", "progress", "sertifikat" };
        string[] res = { "icon_video", "icon_quiz", "icon_progress", "icon_certificate" };
        string[] fb = { ">", "≡", "Il", "*" };
        Sprite[] ic = { iconVideo, iconQuiz, iconProgress, iconCertificate };
        Color[] bgc = { BlueSoft, Peach, BlueSoft, Peach };
        Color[] fgc = { Blue, Orange, Blue, Orange };

        float colW = 906f / 4f;
        for (int i = 0; i < 4; i++)
        {
            float x = i * colW;
            var cell = Panel(card.transform, "Item_" + ids[i], x, 0, colW, MenuH, Color.clear, 0);
            MakeIcon(cell.transform, (colW - 94) / 2f, 30, 94, 94, bgc[i], Pick(ic[i], res[i]), fb[i], fgc[i], -1);
            Label(cell.transform, names[i], 0, 134, colW, 64, 22, Navy, FontStyle.Bold, TextAnchor.UpperCenter);
            AddClick(cell.gameObject, ids[i]);
            if (i > 0) Panel(card.transform, "Divider" + i, x, 40, 2, 130, Hex("E4ECF8"), 0);
        }
    }

    // ----------------------------------------------------------------- PROMO
    void BuildPromo(float y)
    {
        var card = Panel(content, "PromoBanner", 62, y, 900, PromoH, PromoBg, 28);
        AddShadow(card, 0.18f);

        Sprite promoSp = Pick(promoIllustration, "promo");
        if (promoSp != null)
        {
            var ill = NewImage(card.transform, "PromoIllustration", Color.white, 0);
            ill.sprite = promoSp;
            ill.raycastTarget = false;
            SetRect(ill.rectTransform, 498, 0, 402, PromoH);
        }
        else
        {
            Panel(card.transform, "Blob1", 470, 20, 330, 260, Hex("1A4AA5"), -1);
            Panel(card.transform, "Blob2", 780, -30, 180, 120, Orange, -1);
        }

        var tag = Panel(card.transform, "Tag", 57, 28, 198, 34, Orange, -1);
        Label(tag.transform, "Belajar Lebih Mudah", 0, 0, 198, 34, 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        Label(card.transform, "Akses Semua Materi\nKapan Saja, Di Mana Saja", 57, 72, 480, 84, 31, Color.white, FontStyle.Bold, TextAnchor.UpperLeft);
        Label(card.transform, "Raih masa depan yang lebih cerah\ndengan ilmu pengetahuan.", 57, 160, 400, 56, 20, Hex("DCE8FF"), FontStyle.Normal, TextAnchor.UpperLeft);

        var go = Panel(card.transform, "BtnGo", 790, 126, 74, 74, Orange, -1);
        Label(go.transform, "→", 0, 0, 74, 74, 38, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        AddClick(go.gameObject, "akses_materi");
    }

    // -------------------------------------------------------------- SUBJECTS
    void BuildSubjects(float y)
    {
        Label(content, "Pilih Mata Pelajaran", 98, y, 520, 50, 31, Navy, FontStyle.Bold, TextAnchor.MiddleLeft);
        Label(content, "Lihat Semua  →", 640, y + 4, 290, 44, 21, Blue, FontStyle.Bold, TextAnchor.MiddleRight);
        var allBtn = Panel(content, "BtnLihatSemua", 740, y, 200, 50, Color.clear, 0);
        AddClick(allBtn.gameObject, "lihat_semua");

        string[] names = { "IPA", "Fisika", "Matematika", "Biologi", "Bahasa Inggris" };
        string[] res = { "subject_ipa", "subject_fisika", "subject_matematika", "subject_biologi", "subject_inggris" };
        string[] fb = { "A", "F", "Σ", "B", "E" };
        Color[] bgc = { BlueSoft, Peach, BlueSoft, Peach, BlueSoft };
        Color[] fgc = { Blue, Orange, Blue, Orange, Blue };

        float itemW = 177.5f;
        for (int i = 0; i < 5; i++)
        {
            float x = 65f + i * itemW;
            var cell = Panel(content, "Subject_" + names[i], x, y + 62, itemW, 140, Color.clear, 0);
            Sprite s = (subjectIcons != null && i < subjectIcons.Length) ? subjectIcons[i] : null;
            MakeIcon(cell.transform, (itemW - 102) / 2f, 0, 102, 92, bgc[i], Pick(s, res[i]), fb[i], fgc[i], 28);
            Label(cell.transform, names[i], -10, 98, itemW + 20, 32, 21, Gray, FontStyle.Normal, TextAnchor.MiddleCenter);
            AddClick(cell.gameObject, "subject_" + names[i]);
        }
    }

    // ------------------------------------------------------------ BOTTOM NAV
    void BuildBottomNav(RectTransform parent, float navH)
    {
        var bar = NewImage(parent, "BottomNav", Color.white, 40);
        bar.rectTransform.anchorMin = new Vector2(0, 0);
        bar.rectTransform.anchorMax = new Vector2(1, 0);
        bar.rectTransform.pivot = new Vector2(0.5f, 0);
        bar.rectTransform.sizeDelta = new Vector2(0, navH + 40f);     // +40 agar sudut bawah tersembunyi
        bar.rectTransform.anchoredPosition = new Vector2(0, -40f);
        AddShadow(bar, 0.10f, 8f);

        // area konten bar: berada di atas ruang tambahan 40px
        var inner = new GameObject("Inner", typeof(RectTransform));
        inner.transform.SetParent(bar.transform, false);
        var irt = inner.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0, 1); irt.anchorMax = new Vector2(1, 1);
        irt.pivot = new Vector2(0.5f, 1);
        irt.sizeDelta = new Vector2(0, navH);
        irt.anchoredPosition = Vector2.zero;

        string[] names = { "Beranda", "Materi", "Progress", "Profil" };
        string[] res = { "nav_beranda", "nav_materi", "nav_progress", "nav_profil" };
        string[] fb = { "H", "M", "P", "U" };
        float w = DesignW / 4f;

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            var item = Panel(irt, "Nav_" + names[i], i * w, 0, w, NavBase, Color.clear, 0);

            var icon = Panel(item.transform, "Icon", (w - 56) / 2f, 8, 56, 56, Color.clear, 10);
            Sprite s = (navIcons != null && i < navIcons.Length) ? navIcons[i] : null;
            s = Pick(s, res[i]);
            if (s != null) { icon.sprite = s; icon.type = Image.Type.Simple; icon.preserveAspect = true; }
            else Label(icon.transform, fb[i], 0, 0, 56, 56, 28, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            navIconImgs.Add(icon);

            navLabels.Add(Label(item.transform, names[i], 0, 64, w, 30, 21, GrayLite, FontStyle.Bold, TextAnchor.MiddleCenter));
            navIndicators.Add(Panel(item.transform, "Indicator", (w - 76) / 2f, 98, 76, 5, Orange, -1));

            AddClick(item.gameObject, "nav_" + names[i], () => SetActiveNav(idx));
        }
        SetActiveNav(0);
    }

    void SetActiveNav(int active)
    {
        for (int i = 0; i < navLabels.Count; i++)
        {
            bool on = i == active;
            navLabels[i].color = on ? Blue : GrayLite;
            navIconImgs[i].color = on ? Blue : NavOff;
            navIndicators[i].enabled = on;
        }
    }

    // =====================================================================
    //  HELPER
    // =====================================================================
    Sprite Pick(Sprite assigned, string resName)
    {
        if (assigned != null) return assigned;
        if (resCache.TryGetValue(resName, out var cached)) return cached;

        string path = resourcesFolder + "/" + resName;
        Sprite sp = Resources.Load<Sprite>(path);
        if (sp == null)
        {
            Texture2D tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                sp.hideFlags = HideFlags.DontSave;
            }
        }
        resCache[resName] = sp;     // null juga disimpan agar tidak dicari berulang
        return sp;
    }

    void MakeIcon(Transform parent, float x, float y, float w, float h, Color bg, Sprite s, string fallback, Color fg, int radius)
    {
        if (s != null && iconsIncludeBackground)
        {
            var full = NewImage(parent, "Icon", Color.white, 0);
            full.sprite = s;
            full.raycastTarget = false;
            SetRect(full.rectTransform, x, y, w, h);
            return;
        }

        var shape = Panel(parent, "IconBG", x, y, w, h, bg, radius);
        if (s != null)
        {
            var img = NewImage(shape.transform, "Icon", Color.white, 0);
            img.sprite = s; img.preserveAspect = true; img.raycastTarget = false;
            SetRect(img.rectTransform, w * 0.22f, h * 0.22f, w * 0.56f, h * 0.56f);
        }
        else
        {
            Label(shape.transform, fallback, 0, 0, w, h, (int)(Mathf.Min(w, h) * 0.42f), fg, FontStyle.Bold, TextAnchor.MiddleCenter);
        }
    }

    Image Panel(Transform parent, string name, float x, float y, float w, float h, Color color, int radius)
    {
        var img = NewImage(parent, name, color, radius < 0 ? Mathf.RoundToInt(Mathf.Min(w, h) / 2f) : radius);
        SetRect(img.rectTransform, x, y, w, h);
        return img;
    }

    Image NewImage(Transform parent, string name, Color color, int radius)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        if (radius > 0)
        {
            img.sprite = GetRounded(radius);
            img.type = Image.Type.Sliced;
        }
        return img;
    }

    Text Label(Transform parent, string text, float x, float y, float w, float h, int size, Color color,
               FontStyle style, TextAnchor anchor)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), x, y, w, h);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.lineSpacing = 1.05f;
        return t;
    }

    static void AddShadow(Image img, float alpha, float dist = 8f)
    {
        var s = img.gameObject.AddComponent<Shadow>();
        s.effectColor = new Color(0.10f, 0.22f, 0.55f, alpha);
        s.effectDistance = new Vector2(0f, -dist);
        s.useGraphicAlpha = false;
    }

    static void SetRect(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void AddClick(GameObject go, string id, Action extra = null)
    {
        var img = go.GetComponent<Image>();
        if (img == null) { img = go.AddComponent<Image>(); img.color = Color.clear; }
        img.raycastTarget = true;
        var b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.None;
        b.onClick.AddListener(() =>
        {
            Debug.Log("[EduSmart] Klik: " + id);
            extra?.Invoke();
            OnItemClicked?.Invoke(id);
        });
    }

    Sprite GetRounded(int r)
    {
        if (roundCache.TryGetValue(r, out var cached)) return cached;
        int size = r * 2 + 4;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[size * size];
        float half = size / 2f;
        for (int yy = 0; yy < size; yy++)
        {
            for (int xx = 0; xx < size; xx++)
            {
                float dx = Mathf.Abs(xx + 0.5f - half) - (half - r);
                float dy = Mathf.Abs(yy + 0.5f - half) - (half - r);
                float d = Mathf.Sqrt(Mathf.Max(dx, 0) * Mathf.Max(dx, 0) + Mathf.Max(dy, 0) * Mathf.Max(dy, 0))
                          + Mathf.Min(Mathf.Max(dx, dy), 0);
                float a = Mathf.Clamp01(r - d + 0.5f);
                px[yy * size + xx] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                               SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        sp.hideFlags = HideFlags.HideAndDontSave;
        roundCache[r] = sp;
        return sp;
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        return c;
    }
}
