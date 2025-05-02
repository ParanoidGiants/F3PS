using System.Collections.Generic;
using System;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PropertyRecorder<T>
{
    // List storing only the frames where the property changed.
    private List<FrameRecord<T>> records = new List<FrameRecord<T>>();
    private T lastValue;
    private int _framesRecorded = 0;
    public int FramesRecorded => _framesRecorded;
    private bool hasValue = false;

    public int Count => records.Count;

    // This delegate lets you decide what “changed” means for type T.
    // For instance, for Vector3 you might use the == operator (or consider an epsilon tolerance).
    public void RecordIfChanged(int frame, T newValue, Func<T, T, bool> equals)
    {
        _framesRecorded = frame;
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
        int resultIndex = GetIndexAtFrame(frame);
        return resultIndex >= 0 ? records[resultIndex].Value : default(T);
    }

    public int GetIndexAtFrame(int frame)
    {
        int low = 0;
        int high = records.Count - 1;
        int resultIndex = -1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (records[mid].Frame == frame)
            {
                return mid;
            }
            else if (records[mid].Frame < frame)
            {
                resultIndex = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return resultIndex >= 0 ? resultIndex : -1;
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

    internal void ClearAllAfterCurrentFrame(int currentFrame)
    {
        // Log all values here
        var index = GetIndexAtFrame(currentFrame);
        Debug.Log($"Current frame: {currentFrame}, Index: {index}");
        Debug.Log($"Records count: {records.Count}");
        if (records.Count > index)
        {
            records.RemoveRange(index, records.Count - index - 1);
        }
        lastValue = records[index].Value;
        _framesRecorded = currentFrame;
    }
}