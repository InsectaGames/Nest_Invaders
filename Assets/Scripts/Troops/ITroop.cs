public interface ITroop
{
    public TroopType Type { get; }
    public void InitializeBehaviour();
    public void ResetBehaviour();
}