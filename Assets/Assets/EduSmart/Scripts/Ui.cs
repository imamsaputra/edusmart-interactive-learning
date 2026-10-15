using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Kumpulan helper untuk membangun UI lewat kode (tanpa perlu prefab).
public static class Ui {
    static Sprite _round, _circle; static Font _font;

    public static Font F {
        get {
            if (_font == null) {
                try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                if (_font == null) { try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
                if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            }
            return _font;
        }
    }

    // ---------- Sprite ----------
    public static Sprite Round { get { if (_round == null) _round = MakeSprite(64, 26, true); return _round; } }
    public static Sprite Circle { get { if (_circle == null) _circle = MakeSprite(64, 32, false); return _circle; } }

    static Sprite MakeSprite(int s, int r, bool sliced) {
        var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        var px = new Color32[s * s];
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++) {
                float cx = Mathf.Clamp(x + .5f, r, s - r), cy = Mathf.Clamp(y + .5f, r, s - r);
                float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(cx, cy));
                px[y * s + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(r - d + .5f) * 255));
            }
        t.SetPixels32(px); t.Apply();
        return Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect,
            sliced ? new Vector4(r, r, r, r) : Vector4.zero);
    }

    // ---------- Canvas & EventSystem ----------
    public static Canvas MakeCanvas(string n, bool eventSystem = true) {
        var g = new GameObject(n, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = g.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
        var s = g.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1080, 1920); s.matchWidthOrHeight = 0f; // lebar selalu 1080
        if (eventSystem) EnsureEventSystem();
        return c;
    }

    static void EnsureEventSystem() {
#if UNITY_2023_1_OR_NEWER
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
#else
        if (Object.FindObjectOfType<EventSystem>() != null) return;
#endif
        var g = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (t != null) {
            var m = g.AddComponent(t);
            var mi = t.GetMethod("AssignDefaultActions");
            if (mi != null) mi.Invoke(m, null);
        } else g.AddComponent<StandaloneInputModule>();
#else
        g.AddComponent<StandaloneInputModule>();
#endif
    }

    // ---------- Layout ----------
    public static RectTransform Rect(string n, Transform p) {
        var r = new GameObject(n, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(p, false); return r;
    }
    public static void Fill(RectTransform r, float l = 0, float b = 0, float rt = 0, float t = 0) {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rt, -t);
    }
    // anchor = pivot = (ax, ay); posisi relatif terhadap anchor
    public static RectTransform At(RectTransform r, float ax, float ay, float x, float y, float w, float h) {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(ax, ay); r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h); return r;
    }
    public static LayoutElement LE(Component c, float h) {
        var l = c.gameObject.AddComponent<LayoutElement>(); l.preferredHeight = h; return l;
    }
    public static void Clear(Transform t) { for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject); }

    // ---------- Elemen dasar ----------
    public static Image Solid(Transform p, Color c, string n = "Solid") {
        var r = Rect(n, p); Fill(r); var i = r.gameObject.AddComponent<Image>(); i.color = c; return i;
    }
    public static Image Box(Transform p, Color c, string n = "Box", bool circle = false) {
        var r = Rect(n, p); var i = r.gameObject.AddComponent<Image>();
        i.sprite = circle ? Circle : Round; i.type = circle ? Image.Type.Simple : Image.Type.Sliced; i.color = c; return i;
    }
    public static Image Card(Transform p, float w, float h, string n = "Card") {
        var i = Box(p, Color.white, n); i.rectTransform.sizeDelta = new Vector2(w, h);
        var s = i.gameObject.AddComponent<Shadow>(); s.effectColor = new Color(.12f, .2f, .45f, .10f); s.effectDistance = new Vector2(0, -8);
        return i;
    }
    public static Text Label(Transform p, string s, int size, Color c, TextAnchor a = TextAnchor.MiddleCenter, FontStyle st = FontStyle.Normal) {
        var t = Rect("Text", p).gameObject.AddComponent<Text>();
        t.font = F; t.text = s; t.fontSize = size; t.color = c; t.alignment = a; t.fontStyle = st;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.raycastTarget = false; t.lineSpacing = 1.2f;
        return t;
    }
    public static Button MakeClickable(Graphic g, System.Action onClick) {
        var b = g.gameObject.AddComponent<Button>(); b.targetGraphic = g; b.transition = Selectable.Transition.None;
        b.onClick.AddListener(() => onClick()); g.gameObject.AddComponent<ButtonFx>(); return b;
    }
    public static Button Btn(Transform p, string label, Color bg, Color fg, float w, float h, System.Action onClick, int size = 40) {
        var img = Box(p, bg, "Btn_" + label); img.rectTransform.sizeDelta = new Vector2(w, h);
        var t = Label(img.transform, label, size, fg, TextAnchor.MiddleCenter, FontStyle.Bold); Fill(t.rectTransform);
        return MakeClickable(img, onClick);
    }
    public static InputField Input(Transform p, string placeholder, float w, float h) {
        var bg = Box(p, Pal.Hex("EEF2FB"), "Input"); bg.rectTransform.sizeDelta = new Vector2(w, h);
        var inp = bg.gameObject.AddComponent<InputField>();
        var tx = Label(bg.transform, "", 42, Pal.Ink, TextAnchor.MiddleLeft); Fill(tx.rectTransform, 40, 0, 40, 0);
        tx.horizontalOverflow = HorizontalWrapMode.Overflow; tx.supportRichText = false;
        var ph = Label(bg.transform, placeholder, 42, Pal.Muted, TextAnchor.MiddleLeft, FontStyle.Italic); Fill(ph.rectTransform, 40, 0, 40, 0);
        ph.horizontalOverflow = HorizontalWrapMode.Overflow;
        inp.textComponent = tx; inp.placeholder = ph; inp.targetGraphic = bg; inp.characterLimit = 16;
        return inp;
    }

    // ---------- Progress bar ----------
    public static Image Bar(Transform p, Color fg, Color track, float v, out Image trackImg) {
        trackImg = Box(p, track, "BarTrack");
        var f = Box(trackImg.transform, fg, "BarFill"); Fill(f.rectTransform); SetBar(f, v); return f;
    }
    public static void SetBar(Image f, float v) {
        f.rectTransform.anchorMin = Vector2.zero; f.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(v), 1);
        f.rectTransform.offsetMin = f.rectTransform.offsetMax = Vector2.zero;
    }

    // ---------- Scroll ----------
    public static RectTransform Scroll(Transform p, int padX = 60, float spacing = 24) {
        var sv = Rect("Scroll", p); sv.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, .004f);
        var sr = sv.gameObject.AddComponent<ScrollRect>();
        var vp = Rect("Viewport", sv); Fill(vp); vp.gameObject.AddComponent<RectMask2D>();
        var c = Rect("Content", vp); c.anchorMin = new Vector2(0, 1); c.anchorMax = new Vector2(1, 1); c.pivot = new Vector2(.5f, 1); c.sizeDelta = Vector2.zero;
        var v = c.gameObject.AddComponent<VerticalLayoutGroup>(); v.spacing = spacing; v.padding = new RectOffset(padX, padX, 10, 40);
        v.childControlWidth = true; v.childControlHeight = true; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        c.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.viewport = vp; sr.content = c; sr.horizontal = false; sr.vertical = true; sr.scrollSensitivity = 30;
        return sv;
    }
    public static RectTransform Content(RectTransform sv) { return sv.GetComponent<ScrollRect>().content; }

    // ---------- Komponen bersama ----------
    public static void Header(Transform p, string title, string sub, Color col, string backScene, float h = 300) {
        var b = Box(p, col, "Header"); var r = b.rectTransform;
        r.anchorMin = new Vector2(0, 1); r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f, 1);
        r.anchoredPosition = new Vector2(0, 120); r.sizeDelta = new Vector2(0, h + 120);
        b.gameObject.AddComponent<RectMask2D>();
        var d = Box(b.transform, new Color(1, 1, 1, .09f), "Deco", true); At(d.rectTransform, 1, 0, 90, -90, 420, 420);
        float x = 60;
        if (backScene != null) {
            var bb = Box(p, new Color(1, 1, 1, .22f), "Back", true); At(bb.rectTransform, 0, 1, 50, -80, 100, 100);
            var l = Label(bb.transform, "<", 56, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold); Fill(l.rectTransform);
            MakeClickable(bb, () => Nav.Go(backScene)); x = 190;
        }
        var t = Label(p, title, 56, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold); At(t.rectTransform, 0, 1, x, -70, 800, 80);
        var s = Label(p, sub, 34, new Color(1, 1, 1, .85f), TextAnchor.MiddleLeft); At(s.rectTransform, 0, 1, x, -150, 800, 60);
    }

    public static void BottomNav(Transform p, int active) {
        var bar = Card(p, 960, 150, "BottomNav"); At(bar.rectTransform, .5f, 0, 0, 40, 960, 150);
        string[] n = { "Beranda", "Materi", "Kuis", "Profil" }; string[] sc = { Nav.S2, Nav.S3, Nav.S4, Nav.S5 };
        for (int i = 0; i < 4; i++) {
            bool on = i == active; string target = sc[i];
            var c = Pal.Primary; c.a = .14f;
            var pill = Box(bar.transform, on ? c : new Color(1, 1, 1, .004f), "Tab" + n[i]); At(pill.rectTransform, 0, .5f, 24 + i * 232, 0, 212, 110);
            var l = Label(pill.transform, n[i], 34, on ? Pal.Primary : Pal.Muted, TextAnchor.MiddleCenter, FontStyle.Bold); Fill(l.rectTransform);
            if (!on) MakeClickable(pill, () => Nav.Go(target));
        }
    }

    // ---------- Animasi ----------
    public static float EaseOut(float t) { return 1 - Mathf.Pow(1 - t, 3); }
    public static float EaseBack(float t) { float c = 1.70158f; return 1 + (c + 1) * Mathf.Pow(t - 1, 3) + c * Mathf.Pow(t - 1, 2); }

    public static IEnumerator TweenCo(float d, System.Action<float> f, float delay, System.Func<float, float> ease) {
        if (delay > 0) yield return new WaitForSecondsRealtime(delay);
        float t = 0;
        while (t < 1) { t += Time.unscaledDeltaTime / d; f((ease ?? EaseOut)(Mathf.Clamp01(t))); yield return null; }
        f((ease ?? EaseOut)(1));
    }
    public static void Tween(MonoBehaviour m, float d, System.Action<float> f, float delay = 0, System.Func<float, float> ease = null) {
        m.StartCoroutine(TweenCo(d, f, delay, ease));
    }
    public static void Loop(MonoBehaviour m, GameObject owner, System.Action<float> f) { m.StartCoroutine(LoopCo(owner, f)); }
    static IEnumerator LoopCo(GameObject o, System.Action<float> f) { while (o != null) { f(Time.unscaledTime); yield return null; } }

    public static void Pop(MonoBehaviour m, Transform t, float delay = 0, float dur = .45f) {
        var cg = t.GetComponent<CanvasGroup>(); if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0; t.localScale = Vector3.one * .85f;
        Tween(m, dur, k => { if (!t) return; cg.alpha = Mathf.Clamp01(k * 1.6f); t.localScale = Vector3.one * Mathf.LerpUnclamped(.85f, 1f, k); }, delay, EaseBack);
    }
    public static void Slide(MonoBehaviour m, RectTransform r, Vector2 from, float delay = 0, float dur = .6f) {
        var to = r.anchoredPosition; r.anchoredPosition = from;
        Tween(m, dur, k => { if (r) r.anchoredPosition = Vector2.LerpUnclamped(from, to, k); }, delay);
    }
    public static void Shake(MonoBehaviour m, RectTransform r) {
        var o = r.anchoredPosition;
        Tween(m, .4f, k => { if (r) r.anchoredPosition = o + new Vector2(Mathf.Sin(k * Mathf.PI * 6) * (1 - k) * 28, 0); }, 0, x => x);
    }
    public static void Bump(MonoBehaviour m, Transform t) {
        Tween(m, .35f, k => { if (t) t.localScale = Vector3.one * (1 + .06f * Mathf.Sin(k * Mathf.PI)); }, 0, x => x);
    }
    public static void ColorTo(MonoBehaviour m, Graphic g, Color to, float dur = .25f) {
        var from = g.color; Tween(m, dur, k => { if (g) g.color = Color.Lerp(from, to, k); }, 0, x => x);
    }
}
