namespace GameAI
{
    public interface IWinner<TPlayer>
    {
        bool IsWinner(TPlayer player);
    }
}
