using System.Collections.Generic;
using System;

public class PropertyRecorder<T>
{
    // List storing only the frames where the property changed.
    private List<FrameRecord<T>> records = new List<FrameRecord<T>>();
    private T lastValue;
    private bool hasValue = false;

    // This delegate lets you decide what “changed” means for type T.
    // For instance, for Vector3 you might use the == operator (or consider an epsilon tolerance).
    public void RecordIfChanged(int frame, T newValue, Func<T, T, bool> equals)
    {
        // Record if this is the first value or if the new value is different.
        if (!hasValue || !equals(lastValue, newValue))
        {
            records.Add(new FrameRecord<T>(frame, newValue));
            lastValue = newValue;
            hasValue = true;
        }
    }

    // Retrieve the value at a given frame.
    // This uses binary search to quickly find the most recent change at or before the specified frame.
    public T GetValueAtFrame(int frame)
    {
        int low = 0;
        int high = records.Count - 1;
        int resultIndex = -1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (records[mid].Frame == frame)
            {
                return records[mid].Value;
            }
            else if (records[mid].Frame < frame)
            {
                resultIndex = mid; // possible candidate
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // If no record exists before this frame, you might choose a default value.
        // Here, we return default(T) if no record is found.
        return resultIndex >= 0 ? records[resultIndex].Value : default(T);
    }

    public void ClearAllExceptFirstFrame()
    {
        if (records.Count > 0)
        {
            records.RemoveRange(1, records.Count - 1);
        }
        lastValue = records[0].Value;
    }

    public void ClearAll()
    {
        records.Clear();
        hasValue = false;
    }
}