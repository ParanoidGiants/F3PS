public class FrameRecord<T>
{
    public int Frame;
    public T Value;
    public FrameRecord(int frame, T value)
    {
        Frame = frame;
        Value = value;
    }
}