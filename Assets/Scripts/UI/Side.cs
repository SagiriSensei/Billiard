using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Side : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler 
{
    Vector2 m_dotMoveDirectionInCircle;
    CameraControl m_cameraControl;
    Image m_dotImage;
    GameObject m_whiteBallGameobject;
    RectTransform m_parentTransform;

    private void Awake()
    {
        m_cameraControl = Camera.main.GetComponent<CameraControl>();
        m_dotImage = GetComponent<Image>();
        m_parentTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        m_whiteBallGameobject = GameManager.instance.GetWhiteBallGameObject();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_dotImage.raycastTarget = false;
        transform.position = LimitPosition(eventData.position);
        m_cameraControl.SetCuePositionWithSide(m_dotMoveDirectionInCircle);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = LimitPosition(eventData.position);
        m_cameraControl.SetCuePositionWithSide(m_dotMoveDirectionInCircle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_dotImage.raycastTarget = true;
    }

    public Vector2 LimitPosition(Vector2 position)
    {
        float widthScaler = Screen.width / 1280.0f;
        float heightScaler = Screen.height / 720.0f;
        float height = m_parentTransform.rect.height / 2 * heightScaler;
        float width = m_parentTransform.rect.width / 2 * widthScaler;
        position.x = Mathf.Clamp(position.x, m_parentTransform.position.x - width, m_parentTransform.position.x + width);
        position.y = Mathf.Clamp(position.y, m_parentTransform.position.y - height, m_parentTransform.position.y + height);
        m_dotMoveDirectionInCircle.x = (position.x - m_parentTransform.position.x) / width;
        m_dotMoveDirectionInCircle.y = (position.y - m_parentTransform.position.y) / height;
        m_dotMoveDirectionInCircle = MyMath.SquareToCircle(m_dotMoveDirectionInCircle);
        position.x = m_dotMoveDirectionInCircle.x * width + m_parentTransform.position.x;
        position.y = m_dotMoveDirectionInCircle.y * height + m_parentTransform.position.y;
        return position;
    }

    public Vector2 GetSideOffset()
    {
        return m_dotMoveDirectionInCircle;
    }

    public void ResetSide()
    {
        m_dotMoveDirectionInCircle = Vector2.zero;
        m_dotImage.transform.position = m_parentTransform.position;
    }
}
