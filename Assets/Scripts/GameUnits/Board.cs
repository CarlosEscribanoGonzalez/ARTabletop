public class Board : AGameUnit
{
    private BoardGameManager manager;

    private void Start()
    {
        manager = FindFirstObjectByType<BoardGameManager>();
        RequestInfo(manager);  //Al ser escaneado por primera vez le pide la información al manager
    }
}
