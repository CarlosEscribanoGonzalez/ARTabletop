public class Piece : AGameUnit
{
    private void Start()
    {
        RequestInfo(FindFirstObjectByType<PieceGameManager>());
    }
}
