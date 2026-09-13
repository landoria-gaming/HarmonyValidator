using System.Collections;
using TMPro;
using UnityEngine;

namespace MyTestMod;

// Owns the screen-space name shown above the preview character.
internal static class CharacterNameLabel
{
    private static GameObject? labelRoot;
    private static TMP_Text? label;
    private static Transform? characterHead;
    private static bool revealReady;
    private static bool selectionWasVisible;
    private static int revealVersion;

    // Creates a white label that uses the same font as the Valheim menu.
    internal static void Create(FejdStartup startup, PlayerProfile? profile)
    {
        Remove();
        Player player = startup.GetPreviewPlayer();
        Animator animator = player.GetComponentInChildren<Animator>();
        characterHead = animator.GetBoneTransform(HumanBodyBones.Head);
        if (characterHead == null)
        {
            return;
        }

        labelRoot = new GameObject("MyTestModCharacterName");
        labelRoot.SetActive(false);
        Canvas canvas = labelRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        label = UnityEngine.Object.Instantiate(
            startup.m_csName,
            labelRoot.transform);
        label.name = "CharacterNameText";
        label.gameObject.SetActive(true);
        label.text = profile?.GetName() ?? "New Character";
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = false;
        label.fontSize = 64f;

        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600f, 120f);
        rect.localScale = Vector3.one;
        Update(startup);
        MyTestModPlugin.logger.LogInfo(
            $"Showing character name: {label.text}.");
    }

    private static IEnumerator RevealAfterDelay(
        FejdStartup startup,
        GameObject expectedRoot,
        int expectedVersion)
    {
        yield return new WaitForSecondsRealtime(1f);
        if (labelRoot == expectedRoot &&
            revealVersion == expectedVersion &&
            startup.m_characterSelectScreen.activeInHierarchy)
        {
            revealReady = true;
            Update(startup);
        }
    }

    // Tracks selection-screen entries and places the label above the character.
    internal static void Update(FejdStartup startup)
    {
        if (label == null || characterHead == null)
        {
            return;
        }

        bool selectionIsVisible =
            startup.m_characterSelectScreen.activeInHierarchy;
        if (!selectionIsVisible)
        {
            if (selectionWasVisible)
            {
                revealVersion++;
                revealReady = false;
            }

            selectionWasVisible = false;
            labelRoot?.SetActive(false);
            return;
        }

        if (!selectionWasVisible)
        {
            selectionWasVisible = true;
            revealReady = false;
            int expectedVersion = ++revealVersion;
            startup.StartCoroutine(
                RevealAfterDelay(startup, labelRoot!, expectedVersion));
        }

        labelRoot?.SetActive(revealReady);
        if (revealReady)
        {
            Vector3 worldPosition = characterHead.position + Vector3.up * 0.35f;
            Vector3 viewportPosition = startup.m_mainCamera
                .GetComponent<Camera>()
                .WorldToViewportPoint(worldPosition);
            label.rectTransform.position = new Vector3(
                viewportPosition.x * Screen.width,
                viewportPosition.y * Screen.height,
                0f);
        }
    }

    internal static void Remove()
    {
        if (labelRoot != null)
        {
            UnityEngine.Object.Destroy(labelRoot);
            labelRoot = null;
            label = null;
            characterHead = null;
            revealReady = false;
            selectionWasVisible = false;
            revealVersion++;
        }
    }
}
