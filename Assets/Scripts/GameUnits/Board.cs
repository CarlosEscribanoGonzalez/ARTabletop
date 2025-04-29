public class Board : AGameUnit
{
    private void Start()
    {
        RequestInfo(FindFirstObjectByType<BoardGameManager>()); 
    }
}
