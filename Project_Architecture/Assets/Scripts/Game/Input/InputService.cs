using UnityEngine;

// Backward-compatible alias for existing scene/prefab references.
// New setup should use InputServiceComponent directly.
[DisallowMultipleComponent]
public sealed class InputService : InputServiceComponent
{
}
