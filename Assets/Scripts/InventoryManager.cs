using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform itemContainer;

    [Header("Furniture")]
    [SerializeField] private List<FurnitureData> items = new List<FurnitureData>();

    [Header("Inventory Position")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float panelDistance = 0.8f;
    [SerializeField] private float heightOffset = -0.1f;

    [SerializeField] private FurnitureSpawner furnitureSpawner;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

#if UNITY_EDITOR
private void Update()
{
    if (Input.GetKeyDown(KeyCode.I))
    {
        ToggleInventory();
    }
}
#endif

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("InventoryPanelが未設定です");
            return;
        }

        bool willOpen = !inventoryPanel.activeSelf;

        if (willOpen)
        {
            // 開く直前に現在の目の前へ移動
            MoveInventoryToFront();
        }

        inventoryPanel.SetActive(willOpen);

        if (willOpen)
        {
            RefreshInventory();
        }
    }

    public void AddItem(FurnitureData furniture)
    {
        if (furnitureSpawner == null) return;

        items.Add(furniture);

        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            RefreshInventory();
        }
    }

    public void RefreshInventory()
    {
        if (itemSlotPrefab == null || itemContainer == null || furnitureSpawner == null)
        {
            Debug.LogWarning("Inventoryの参照が未設定です");
            return;
        }

        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = itemContainer.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        foreach (FurnitureData furniture in items)
        {
            if (furniture == null) continue;

            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);

            InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();

            if (inventorySlot != null)
            {
                inventorySlot.Initialize(furniture, furnitureSpawner, this);
            }
            else
            {
                Debug.LogWarning("ItemSlotにInventorySlotがありません");
            }
        }

        GridObjectCollection grid = itemContainer.GetComponent<GridObjectCollection>();

        if (grid != null)
        {
            grid.UpdateCollection();
        }
    }

    private void MoveInventoryToFront()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player Cameraが未設定です");
            return;
        }

        // カメラの正面方向を取得
        Vector3 forward = playerCamera.forward;

        // 上下方向の傾きを除去
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
        {
            return;
        }

        forward.Normalize();

        // カメラの正面にインベントリを配置
        Vector3 targetPosition = playerCamera.position + forward * panelDistance + Vector3.up * heightOffset;

        inventoryPanel.transform.position = targetPosition;

        // インベントリの向きをカメラの水平方向に合わせる
        inventoryPanel.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}
