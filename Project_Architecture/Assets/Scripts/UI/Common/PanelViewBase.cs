using UnityEngine;

public abstract class PanelViewBase : MonoBehaviour
{
    [SerializeField] private GameObject root;

    protected virtual void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    public virtual void Show()
    {
        if (root != null)
        {
            root.SetActive(true);
        }
    }

    public virtual void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }
}
