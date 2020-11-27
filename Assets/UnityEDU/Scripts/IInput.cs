using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input. Could be either player input, or a replay.
/// </summary>
public interface IInput 
{
    float Thruster { get; }
    float Rudder { get; }
    bool IsBreaking { get; }
}
