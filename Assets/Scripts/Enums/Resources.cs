public enum Resource
{
    FUNGUS,
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
            res = Resource.FUNGUS,
            cantidad = 1
        };
    }
}