using System.Collections.Generic;
using UnityEngine;

public class IconLibrary : MonoBehaviour
{
    [SerializeField] private List<Sprite> iconSprites;

    private Dictionary<string, Sprite> iconMap;

    private void Awake()
    {
        iconMap = new Dictionary<string, Sprite>();
        foreach (var sprite in iconSprites)
        {
            var key = sprite.name.Replace("_icon", "").ToLower();
            iconMap[key] = sprite;
        }
    }

    public Sprite GetIcon(string itemName)
    {
        itemName = itemName.ToLower();
        return iconMap.TryGetValue(itemName, out var sprite) ? sprite : null;
    }
}
