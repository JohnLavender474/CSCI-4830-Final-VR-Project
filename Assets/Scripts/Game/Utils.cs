using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public static class Utils
{
    public static void Set(Transform toMove, Transform target)
    {
        Set(toMove.position, target.position);
    }

    public static void Set(Vector3 toMove, Vector3 targetPosition)
    {
        var x = targetPosition.x;
        var y = targetPosition.y;
        var z = targetPosition.z;
        toMove.Set(x, y, z);
    }
}
