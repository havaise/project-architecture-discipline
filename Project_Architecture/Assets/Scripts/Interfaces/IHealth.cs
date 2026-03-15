using System;
using UnityEngine;

public interface IHealth
{
    int _current { get; }
    int _max { get; }
    void Reduce(int count);
    bool isDead();
}
