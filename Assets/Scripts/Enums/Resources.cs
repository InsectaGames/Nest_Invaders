public enum Resource
{
    STICK,
    LEAF
}

[System.Serializable]
public struct ResourceSlot {

    public Resource res;
    public int cantidad;

}