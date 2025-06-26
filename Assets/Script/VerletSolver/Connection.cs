public struct Connection
{
    public int FirstDotIndex { get; }
    public int SecondDotIndex { get; }
    public float Length { get; }

    public Connection(int firstDotIndex, int secondDotIndex, float length)
    {
        FirstDotIndex = firstDotIndex;
        SecondDotIndex = secondDotIndex;
        Length = length;
    }
}