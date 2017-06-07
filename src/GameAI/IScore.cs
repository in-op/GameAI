namespace GameAI
{
    public interface IScore<TPlayer>
    {
        int Score(TPlayer player);
    }
}
