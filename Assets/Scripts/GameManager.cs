using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Color variables
    public List<Color> colors;
    private List<Color> usedColors = new();

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Color> GetAvailableColors()
    {
        return colors.Except(usedColors).ToList();
    }

    public void AddColorToUsed(Color color)
    {
        usedColors.Add(color);

        Debug.Log("[AddColorToUsed] UsedColors count: " + usedColors.Count);
        Debug.Log("[AddColorToUsed] AvailableColors count: " + colors.Except(usedColors).ToList().Count);
    }

    public void RemoveColorFromUsed(Color color)
    {
        if (usedColors.Contains(color)) {
            usedColors.Remove(color);
        }

        Debug.Log("[RemoveColorFromUsed] UsedColors count: " + usedColors.Count);
        Debug.Log("[RemoveColorFromUsed] AvailableColors count: " + colors.Except(usedColors).ToList().Count);
    }
}
