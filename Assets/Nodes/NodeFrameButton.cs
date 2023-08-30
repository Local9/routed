using UnityEngine;
using UnityEngine.EventSystems;

public class NodeFrameButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject NodeFrameParent;
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                NodeFrameParent.GetComponent<NodeFrame>().UpdateSigs();
                break;
            case PointerEventData.InputButton.Right:
                NodeFrameParent.GetComponent<NodeFrame>().RemoveSigs();
                break;
        }
    }
}
