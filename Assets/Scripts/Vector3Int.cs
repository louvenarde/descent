#if UNITY_EDITOR
#endif

public struct Vector3Int
{
    public int x, y, z;

    public Vector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString()
    {
        return string.Format("{0}; {1}; {2}", x, y, z);
    }
};
