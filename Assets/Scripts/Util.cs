using UnityEngine;

public class Util {

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        //源代码写的不对，他只设置了两层，实际上要将所有子对象的层级都设置一遍
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
