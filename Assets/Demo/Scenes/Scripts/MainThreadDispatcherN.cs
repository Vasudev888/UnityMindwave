using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcherN : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();

    private void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public static void Enqueue(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        lock (actions)
        {
            actions.Enqueue(action);
        }
    }
}
