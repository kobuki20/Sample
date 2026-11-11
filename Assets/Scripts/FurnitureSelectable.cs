using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(ObjectManipulator))]
public class FurnitureSelectable : MonoBehaviour
{
    private FurnitureSelectionManager selectionManager;
    private ObjectManipulator objectManipulator;

    private void Awake()
    {
        objectManipulator = GetComponent<ObjectManipulator>();

        // 家具を掴み始めた際のイベントを登録
        objectManipulator.OnManipulationStarted.AddListener(HandleManipulationStarted);
    }

    public void Initialize(FurnitureSelectionManager manager)
    {
        selectionManager = manager;
    }

    private void HandleManipulationStarted(ManipulationEventData eventData)
    {
        if (selectionManager != null)
        {
            selectionManager.SelectFurniture(this);
        }
    }

    private void OnDestroy()
    {
        if (objectManipulator != null)
        {
            objectManipulator.OnManipulationStarted.RemoveListener(HandleManipulationStarted);
        }
    }
}
