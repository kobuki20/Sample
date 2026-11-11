using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class InventorySlot : MonoBehaviour
{

    [SerializeField] private FurnitureData furnitureData;
    [SerializeField] private FurnitureSpawner furnitureSpawner;

    // インベントリの管理用
    private InventoryManager inventoryManager;

    private Interactable interactable;
    private ButtonConfigHelper buttonConfigHelper;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();

        // MRTKのボタン表示設定を取得
        buttonConfigHelper = GetComponent<ButtonConfigHelper>();

        if (buttonConfigHelper == null)
        {
            buttonConfigHelper = GetComponentInChildren<ButtonConfigHelper>(true);
        }

        if (interactable != null)
        {
            interactable.OnClick.AddListener(SelectFurniture);
        }
        else
        {
            Debug.LogWarning("Interactableが見つかりません");
        }
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.OnClick.RemoveListener(SelectFurniture);
        }
    }

    public void Initialize(FurnitureData data, FurnitureSpawner spawner, InventoryManager manager)
    {
        furnitureData = data;
        furnitureSpawner = spawner;
        inventoryManager = manager;

        // 家具データを確認
        if (furnitureData == null)
        {
            Debug.LogWarning("家具データが未設定です");
            return;
        }

        // ボタンの表示を更新
        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        if (buttonConfigHelper == null)
        {
            Debug.LogWarning("ButtonConfigHelperが見つかりません");
            return;
        }

        // 家具名を設定
        buttonConfigHelper.MainLabelText = furnitureData.furnitureName;

        // 家具アイコンを設定
        if (furnitureData.icon != null)
        {
            buttonConfigHelper.SetSpriteIcon(furnitureData.icon);
        }
        else
        {
            Debug.LogWarning(furnitureData.furnitureName + "のアイコンが未設定です");
        }
    }

    public void SelectFurniture()
    {
        if (furnitureData == null || furnitureData.furniturePrefab == null || furnitureSpawner == null || inventoryManager == null)
        {
            Debug.LogWarning("家具データまたはSpawnerが未設定です");
            return;
        }

        // 家具を生成
        furnitureSpawner.SpawnFurniture(furnitureData.furniturePrefab);

        // インベントリを閉じる
        inventoryManager.CloseInventory();
    }
}
