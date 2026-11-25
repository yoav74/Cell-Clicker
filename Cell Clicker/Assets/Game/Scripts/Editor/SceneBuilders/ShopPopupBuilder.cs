using CellClicker.Core;
using CellClicker.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CellClicker.EditorTools.UiFactory;

namespace CellClicker.EditorTools
{
    /// <summary>
    /// The pieces every shop shares: the round shop button in a bottom corner, the bottom-sheet popup with a
    /// scrolling list, and the row look. Used by <see cref="OrganismShopBuilder"/> and <see cref="UpgradeShopBuilder"/>.
    /// </summary>
    public static class ShopPopupBuilder
    {
        // Button: ~170x200 tap target, outside the play area so it can't steal hive taps.
        private static readonly Vector2 ButtonSize = new Vector2(170f, 200f);
        private const float ButtonMarginX = 28f;
        private const float ButtonMarginY = 16f;
        private const float ButtonCircle = 148f;

        private const float SheetHeight = 0.74f; // share of the safe area
        private const float RowHeight = 200f;

        internal static readonly Color Gold = new Color32(255, 196, 64, 255);
        private static readonly Color Ink = new Color32(30, 22, 10, 255);
        private static readonly Color SheetColor = new Color32(15, 31, 45, 255);
        private static readonly Color RowColor = new Color32(255, 255, 255, 16);
        private static readonly Color Subtle = new Color32(150, 180, 190, 220);
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.7f);

        /// <summary>The look of one shop button: its colors, icon and label.</summary>
        public readonly struct ButtonStyle
        {
            public readonly string Label;
            public readonly ProceduralSpriteKind Icon;
            public readonly Color Accent;
            public readonly Color Fill;
            public readonly Color IconColor;

            public ButtonStyle(string label, ProceduralSpriteKind icon, Color accent, Color fill, Color iconColor)
            {
                Label = label;
                Icon = icon;
                Accent = accent;
                Fill = fill;
                IconColor = iconColor;
            }
        }

        /// <summary>A round shop button in the bottom-left (<paramref name="right"/> false) or bottom-right corner.</summary>
        public static T BuildButton<T>(Transform safeArea, string name, ButtonStyle style, bool right, DnaBank bank, PopupPanel popup)
            where T : ShopButtonUI
        {
            var corner = new Vector2(right ? 1f : 0f, 0f);
            var margin = new Vector2(right ? -ButtonMarginX : ButtonMarginX, ButtonMarginY);
            RectTransform root = Pin(Create(name, safeArea), corner, ButtonSize, margin);
            AddImage(root.gameObject, Color.clear, raycast: true); // the whole area (circle + label) is tappable

            // Own canvas: it pulses every frame while something is affordable.
            root.gameObject.AddComponent<Canvas>();
            root.gameObject.AddComponent<GraphicRaycaster>();

            RectTransform pulse = Create("Pulse", root);
            pulse.anchorMin = pulse.anchorMax = new Vector2(0.5f, 1f);
            pulse.sizeDelta = new Vector2(ButtonCircle, ButtonCircle);
            pulse.anchoredPosition = new Vector2(0f, -ButtonCircle * 0.5f);

            RectTransform glow = Create("Glow", pulse);
            glow.anchorMin = new Vector2(-0.25f, -0.25f);
            glow.anchorMax = new Vector2(1.25f, 1.25f);
            glow.offsetMin = glow.offsetMax = Vector2.zero;
            AddPlaceholder(glow.gameObject, ProceduralSpriteKind.SoftDot, new Color(style.Accent.r, style.Accent.g, style.Accent.b, 0.3f));

            RectTransform circle = Create("Circle", pulse);
            Stretch(circle);
            Image circleImage = AddPlaceholder(circle.gameObject, ProceduralSpriteKind.Pill, style.Fill);

            RectTransform ring = Create("Ring", pulse);
            Stretch(ring);
            AddPlaceholder(ring.gameObject, ProceduralSpriteKind.Ring, style.Accent);

            RectTransform icon = Pin(Create("Icon", pulse), new Vector2(0.5f, 0.5f), new Vector2(104f, 104f), Vector2.zero);
            AddPlaceholder(icon.gameObject, style.Icon, style.IconColor).preserveAspect = true;

            // "Something to buy" badge on the top-right of the circle
            RectTransform badge = Pin(Create("Badge", pulse), Vector2.one, new Vector2(52f, 52f), new Vector2(6f, 6f));
            AddPlaceholder(badge.gameObject, ProceduralSpriteKind.Pill, Gold);
            RectTransform mark = Create("Mark", badge);
            Stretch(mark);
            AddText(mark.gameObject, "!", 38, Ink, null);

            RectTransform label = Create("Label", root);
            label.anchorMin = Vector2.zero;
            label.anchorMax = new Vector2(1f, 0f);
            label.pivot = new Vector2(0.5f, 0f);
            label.sizeDelta = new Vector2(40f, 44f); // a little wider than the button so the word never clips
            AddText(label.gameObject, style.Label, 32, Color.white, TmpSetup.SoftShadowPreset);

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = circleImage;

            var shopButton = root.gameObject.AddComponent<T>();
            Wire(shopButton, "bank", bank);
            Wire(shopButton, "popup", popup);
            Wire(shopButton, "badge", badge.gameObject);
            Wire(shopButton, "pulseTarget", pulse);
            badge.gameObject.SetActive(false);
            return shopButton;
        }

        /// <summary>A bottom-sheet popup (closed at start) with a title and a scrolling list; rows go into <paramref name="content"/>.</summary>
        public static PopupPanel BuildPopup(Transform canvas, string name, string titleText, string subtitleText, out RectTransform content)
        {
            RectTransform root = Create(name, canvas);
            Stretch(root);
            root.gameObject.AddComponent<Canvas>();
            root.gameObject.AddComponent<GraphicRaycaster>();

            // Full-screen dim; tapping it closes the popup. Also blocks taps on the hive while open.
            RectTransform backdrop = Create("Backdrop", root);
            Stretch(backdrop);
            AddImage(backdrop.gameObject, Dim, raycast: true);
            var backdropGroup = backdrop.gameObject.AddComponent<CanvasGroup>();
            var backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;

            RectTransform safe = Create("Safe Area", root);
            Stretch(safe);
            safe.gameObject.AddComponent<CellClicker.UI.SafeArea>();

            // Bottom sheet: easy to reach with a thumb, and the hive stays visible above it.
            RectTransform sheet = Create("Sheet", safe);
            sheet.anchorMin = Vector2.zero;
            sheet.anchorMax = new Vector2(1f, SheetHeight);
            sheet.pivot = new Vector2(0.5f, 0f);
            sheet.offsetMin = sheet.offsetMax = Vector2.zero;
            AddRounded(sheet.gameObject, SheetColor, 56f, raycast: true); // raycast: taps on the sheet don't reach the backdrop

            // Squares off the bottom corners and covers the gesture bar below the safe area.
            RectTransform fill = Create("Bottom Fill", sheet);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(1f, 0f);
            fill.pivot = new Vector2(0.5f, 1f);
            fill.sizeDelta = new Vector2(0f, 400f);
            fill.anchoredPosition = new Vector2(0f, 60f);
            AddImage(fill.gameObject, SheetColor, raycast: true);

            RectTransform handle = Pin(Create("Handle", sheet), new Vector2(0.5f, 1f), new Vector2(140f, 12f), new Vector2(0f, -22f));
            AddRounded(handle.gameObject, new Color(1f, 1f, 1f, 0.15f), 6f);

            RectTransform title = PinTop(Create("Title", sheet), 46f, 96f, 56f, 200f);
            AddText(title.gameObject, titleText, 76, Color.white, TmpSetup.PopupPreset, TextAlignmentOptions.Left);

            RectTransform subtitle = PinTop(Create("Subtitle", sheet), 144f, 50f, 58f, 200f);
            AddText(subtitle.gameObject, subtitleText, 36, Subtle, TmpSetup.SoftShadowPreset, TextAlignmentOptions.Left);

            RectTransform close = Pin(Create("Close Button", sheet), Vector2.one, new Vector2(MinTapSize, MinTapSize), new Vector2(-36f, -30f));
            Image closeImage = AddRounded(close.gameObject, new Color(1f, 1f, 1f, 0.1f), MinTapSize * 0.5f, raycast: true);
            var closeButton = close.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = closeImage;
            RectTransform closeMark = Create("Mark", close);
            Stretch(closeMark);
            AddText(closeMark.gameObject, "X", 56, Color.white, TmpSetup.PopupPreset);

            content = BuildList(sheet);

            var popup = root.gameObject.AddComponent<PopupPanel>();
            Wire(popup, "sheet", sheet);
            Wire(popup, "backdrop", backdropGroup);
            WireArray(popup, "closeButtons", backdropButton, closeButton);

            root.gameObject.SetActive(false);
            return popup;
        }

        // ScrollRect > Viewport (RectMask2D: cheaper than Mask, no stencil) > Content (VerticalLayoutGroup + ContentSizeFitter)
        private static RectTransform BuildList(RectTransform sheet)
        {
            RectTransform list = Create("List", sheet);
            Stretch(list);
            list.offsetMin = new Vector2(32f, 24f);
            list.offsetMax = new Vector2(-32f, -220f);

            RectTransform viewport = Create("Viewport", list);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            AddImage(viewport.gameObject, Color.clear, raycast: true); // lets the player drag from empty space

            RectTransform content = Create("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 8, 24);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = list.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        /// <summary>The shared row look; T says what the row sells.</summary>
        public static T BuildRow<T>(RectTransform content, string name, ProceduralSpriteKind iconKind) where T : ShopRowUI
        {
            RectTransform row = Create(name, content);
            AddRounded(row.gameObject, RowColor, 36f);
            var element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = element.preferredHeight = RowHeight;

            RectTransform iconBack = Create("Icon Back", row);
            iconBack.anchorMin = iconBack.anchorMax = new Vector2(0f, 0.5f);
            iconBack.sizeDelta = new Vector2(150f, 150f);
            iconBack.anchoredPosition = new Vector2(24f + 75f, 0f); // pivot stays centered so the buy "punch" scales evenly
            AddPlaceholder(iconBack.gameObject, ProceduralSpriteKind.Pill, new Color(0f, 0f, 0f, 0.28f));

            RectTransform icon = Pin(Create("Icon", iconBack), new Vector2(0.5f, 0.5f), new Vector2(120f, 120f), Vector2.zero);
            Image iconImage = AddPlaceholder(icon.gameObject, iconKind, Color.white);
            iconImage.preserveAspect = true; // some placeholders (the helix) are taller than wide

            TMP_Text nameText = AddText(RowText("Name", row, 4f, 64f).gameObject, "Name", 50, Color.white, TmpSetup.SoftShadowPreset, TextAlignmentOptions.Left);
            TMP_Text infoText = AddText(RowText("Info", row, -56f, -4f).gameObject, "Info", 34, Subtle, TmpSetup.SoftShadowPreset, TextAlignmentOptions.Left);

            RectTransform buy = Pin(Create("Buy Button", row), new Vector2(1f, 0.5f), new Vector2(290f, 150f), new Vector2(-24f, 0f));
            Image buyImage = AddRounded(buy.gameObject, new Color32(76, 196, 120, 255), 36f, raycast: true);
            var buyButton = buy.gameObject.AddComponent<Button>();
            buyButton.targetGraphic = buyImage;
            ColorBlock colors = buyButton.colors;
            colors.disabledColor = Color.white; // ShopRowUI colors the button itself
            buyButton.colors = colors;

            RectTransform action = PinTop(Create("Action", buy), 12f, 42f);
            TMP_Text actionText = AddText(action.gameObject, "Buy", 30, new Color(1f, 1f, 1f, 0.9f), TmpSetup.SoftShadowPreset);

            RectTransform cost = Create("Cost", buy);
            cost.anchorMin = Vector2.zero;
            cost.anchorMax = new Vector2(1f, 0f);
            cost.pivot = new Vector2(0.5f, 0f);
            cost.sizeDelta = new Vector2(-24f, 84f);
            cost.anchoredPosition = new Vector2(0f, 14f);
            var costLayout = cost.gameObject.AddComponent<HorizontalLayoutGroup>();
            costLayout.childAlignment = TextAnchor.MiddleCenter;
            costLayout.spacing = 8f;
            costLayout.childControlWidth = costLayout.childControlHeight = true;
            costLayout.childForceExpandWidth = costLayout.childForceExpandHeight = false;

            RectTransform helix = Create("DNA Icon", cost);
            AddPlaceholder(helix.gameObject, ProceduralSpriteKind.Helix, Color.white);
            var helixSize = helix.gameObject.AddComponent<LayoutElement>();
            helixSize.preferredWidth = 36f;
            helixSize.preferredHeight = 48f;

            TMP_Text costText = AddText(Create("Cost Text", cost).gameObject, "15", 50, Color.white, TmpSetup.PopupPreset);

            var rowUI = row.gameObject.AddComponent<T>();
            Wire(rowUI, "icon", iconImage);
            Wire(rowUI, "nameText", nameText);
            Wire(rowUI, "infoText", infoText);
            Wire(rowUI, "buyButton", buyButton);
            Wire(rowUI, "buyBackground", buyImage);
            Wire(rowUI, "actionText", actionText);
            Wire(rowUI, "costText", costText);
            Wire(rowUI, "punchTarget", iconBack);
            return rowUI;
        }

        // Text between the icon and the buy button, from bottom to top relative to the row's middle.
        private static RectTransform RowText(string name, RectTransform row, float bottom, float top)
        {
            RectTransform rect = Create(name, row);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(196f, bottom);
            rect.offsetMax = new Vector2(-330f, top);
            return rect;
        }
    }
}
