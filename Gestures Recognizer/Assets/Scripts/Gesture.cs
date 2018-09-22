using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture {

    public string name;
    //BitArray is used because it uses less space
    public BitArray bits;

    public Gesture() { }

    public Gesture(string name, BitArray bits)
    {
        this.name = name;
        this.bits = bits;
    }
}
