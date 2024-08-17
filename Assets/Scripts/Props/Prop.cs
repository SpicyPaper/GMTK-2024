using UnityEngine;

public class Prop : MonoBehaviour
{
    public enum SizeCategory { UNDEFINED, XS, S, M, L, XL, XXL }

    public float GetSize()
    {
        if (gameObject.TryGetComponent<Renderer>(out var renderer))
        {
            Vector3 boundsSize = renderer.bounds.size;
            return boundsSize.x * boundsSize.y * boundsSize.z;
        }

        return 0;
    }

    public SizeCategory GetSizeCategory()
    {
        float size = GetSize();
        if (size > 0)
        {
            return GetCategoryFromSize(size);
        }

        return SizeCategory.UNDEFINED;
    }

    private SizeCategory GetCategoryFromSize(float size)
    {
        if (size <= 0.125f) return SizeCategory.XS;
        else if (size <= 1f) return SizeCategory.S;
        else if (size <= 8f) return SizeCategory.M;
        else if (size <= 27f) return SizeCategory.L;
        else if (size <= 64f) return SizeCategory.XL;
        else return SizeCategory.XXL;
    }
}
