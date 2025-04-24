// ReSharper disable all
#nullable disable

// Taken from Unity's source code, this is here to allow NativeWebSocket to work in Il2Cpp.

using System.Collections;

public abstract class CustomYieldInstruction : IEnumerator
{
    public abstract bool keepWaiting
    {
        get;
    }

    public object Current
    {
        get
        {
            return null;
        }
    }
    public bool MoveNext() { return keepWaiting; }
    public virtual void Reset() {}
}
