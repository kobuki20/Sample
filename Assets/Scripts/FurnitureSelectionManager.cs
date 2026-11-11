using UnityEngine;

public class FurnitureSelectionManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject deleteButton;

    // 現在選択されている家具
    private FurnitureSelectable selectedFurniture;

    // Start is called before the first frame update
    void Start()
    {
        UpdateDeleteButton();
    }

    // 家具を選択
    public void SelectFurniture(FurnitureSelectable furniture)
    {
        if (furniture == null) return;

        selectedFurniture = furniture;

        Debug.Log("選択した家具: " + furniture.name);

        UpdateDeleteButton();
    }

    // 選択中の家具を削除
    public void DeleteSelectedFurniture()
    {
        if (selectedFurniture == null)
        {
            Debug.LogWarning("削除する家具が選択されていません");
            return;
        }

        GameObject target = selectedFurniture.gameObject;

        Debug.Log("削除する家具: " + target.name);

        selectedFurniture = null;

        UpdateDeleteButton();

        Destroy(target);
    }

    void UpdateDeleteButton()
    {
        if (deleteButton != null)
        {
            deleteButton.SetActive(selectedFurniture != null);
        }
    }
}
