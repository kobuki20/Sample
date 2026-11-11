using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float spawnDistance = 1.0f;

    [Header("Selection Manager")]
    [SerializeField] private FurnitureSelectionManager selectionManager;

    public void SpawnFurniture(GameObject furniturePrefab)
    {
        if (playerCamera == null || furniturePrefab == null)
        {
            Debug.LogWarning("CameraまたはPrefabが未設定です");
            return;
        }

        // カメラの水平方向を取得
        Vector3 forward = playerCamera.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("生成方向を計算できません");
            return;
        }

        forward.Normalize();

        // 家具の生成位置
        Vector3 spawnPosition = playerCamera.position + forward * spawnDistance;

        // 家具を生成
        GameObject spawnFurniture = Instantiate(furniturePrefab, spawnPosition, Quaternion.identity);

        // 家具の選択機能を取得
        FurnitureSelectable selectable = spawnFurniture.GetComponent<FurnitureSelectable>();

        if (selectable == null)
        {
            Debug.LogWarning("生成した家具にFurnitureSelectableがありません");
            return;
        }

        if (selectionManager == null)
        {
            Debug.LogWarning("FurnitureSelectionManagerが未設定です");
            return;
        }

        // 選択管理システムと連携
        selectable.Initialize(selectionManager);

        // 生成直後の家具を選択状態にする
        selectionManager.SelectFurniture(selectable);
    }
}
