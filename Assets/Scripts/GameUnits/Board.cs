public class Board : AGameUnit
{
    private void Start()
    {
        RequestInfo(FindFirstObjectByType<BoardGameManager>());  //Al ser escaneado por primera vez le pide la informaci�n al manager
    }
}
