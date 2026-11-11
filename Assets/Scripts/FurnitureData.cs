using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFurniture",
    menuName = "Inventory/Furniture Data"
)]

public class FurnitureData : ScriptableObject
{
    public string furnitureName;
    public Sprite icon;
    public GameObject furniturePrefab;
}
