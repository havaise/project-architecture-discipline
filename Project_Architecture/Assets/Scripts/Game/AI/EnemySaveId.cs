using UnityEngine;

[DisallowMultipleComponent]
public class EnemySaveId : MonoBehaviour
{
    [SerializeField] private string id;

    public string Id => id;

    private void Awake()
    {
        EnsureId();
    }

    private void Reset()
    {
        EnsureId();
    }

    private void OnValidate()
    {
        EnsureId();
    }

    private void EnsureId()
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        id = System.Guid.NewGuid().ToString("N");
    }
}
