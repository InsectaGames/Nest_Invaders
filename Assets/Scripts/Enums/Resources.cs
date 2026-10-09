public enum Resource
{
    LEAF,
    STICK
}

[System.Serializable]
public struct ResourceSlot
{
    public Resource res;
    public int cantidad;

    public ResourceSlot(Resource r, int n)
    {
        res = r;
        cantidad = n;
    }

    public static implicit operator ResourceSlot(LogicEvent logicEvent)
    {
        return new ResourceSlot
        { 
            res = Resource.LEAF,
            cantidad = 1
        };
    }
}