

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIAnimationController
{
    private readonly Dictionary<string, UIAnimation> animations = new();

    public void Register(string key, string folderPath, string filename, int frameCount, int intervalMs, VisualElement element)
    {
        if (animations.ContainsKey(key)) return;

        UIAnimation animation = new UIAnimation(element, folderPath, filename, frameCount, intervalMs);
        animations[key] = animation;
    }

    public void StartAnimation(string key)
    {
        if(animations.TryGetValue(key, out var anim))
        {
            anim.Start();
        }
    }

    public void PauseAnimation(string key)
    {
        if(animations.TryGetValue(key, out var anim))
        {
            anim.Pause();
        }
    }

    public void ResumeAnimation(string key)
    {
        if(animations.TryGetValue(key, out var anim))
        {
            anim.Resume();
        }
    }

    public void StopAnimation(string key)
    {
        if(animations.TryGetValue(key, out var anim))
        {
            anim.Stop();
        }
    }

    public void SetVisibility(string key, bool visible)
    {
        if(animations.TryGetValue(@key, out var anim))
        {
            anim.Element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

public class UIAnimation
{
    public VisualElement Element { get; }
    private readonly string folderPath;
    private readonly string filename;
    private readonly int frameCount;
    private readonly int intervalMs;

    private Texture2D[] frames;
    private int currentFrame;
    private IVisualElementScheduledItem scheduledItem;

    public UIAnimation(VisualElement element, string folderPath, string filename, int frameCount, int intervalMs)
    {
        Element = element;
        this.folderPath = folderPath;
        this.filename = filename;
        this.frameCount = frameCount;
        this.intervalMs = intervalMs;

    }

    public void Start()
    {
        if (scheduledItem != null) return;

        frames = new Texture2D[frameCount];
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = Resources.Load<Texture2D>($"{folderPath}/{filename} {i}");
        }

        currentFrame = 0;
        scheduledItem = Element.schedule.Execute(() =>
        {
            Element.style.backgroundImage = new StyleBackground(frames[currentFrame]);
            currentFrame = (currentFrame + 1) % frames.Length;

        }).Every(intervalMs);
    }

    public void Pause()
    {
        scheduledItem?.Pause();
    }

    public void Resume()
    {
        scheduledItem?.Resume();
    }

    public void Stop()
    {
        scheduledItem?.Pause();
        scheduledItem = null;
    }
}
