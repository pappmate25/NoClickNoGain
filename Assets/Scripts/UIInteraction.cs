using UnityEngine.UIElements;

public static class UIInteraction
{
    public static bool IsPointerOverUI { get; private set; }

    public static void Initialize(VisualElement root)
    {
        root.RegisterCallback<PointerEnterEvent>(_ => IsPointerOverUI = true, TrickleDown.TrickleDown);
        root.RegisterCallback<PointerLeaveEvent>(_ => IsPointerOverUI = false, TrickleDown.TrickleDown);
        root.RegisterCallback<PointerDownEvent>(_ => IsPointerOverUI = true, TrickleDown.TrickleDown);
        root.RegisterCallback<PointerUpEvent>(_ => IsPointerOverUI = false, TrickleDown.TrickleDown);
        root.RegisterCallback<PointerCancelEvent>(_ => IsPointerOverUI = false, TrickleDown.TrickleDown);
    }
}