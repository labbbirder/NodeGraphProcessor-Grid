using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BBBirder;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

public class Test : MonoBehaviour
{

    [Serializable]
    public struct Foo
    {
        public string asd;
        public int aa;
    }

    [Serializable]
    public class Bar
    {
        public string asd;
        public int aa;
    }

    [Serializable]
    public class Bar2 : Bar
    {
        public string dasd;
        public int aa22;
    }

    [ReadOnly]
    [OnChange("ArrayCha")]
    [SerializeField]
    private int[] options;

    [ReadOnly]
    [OnChange("OnIntChanged")]
    public int asdasd;

    // [ReadOnly]
    [OnChange("Good")]
    public Foo foo2;

    [OnChange("Good")]
    public Foo foo;

    [OnChange("Good")]
    [ReadOnly]
    public Bar bar;

    [OnChange("Good")]
    [ReadOnly]
    [SerializeReference, Polymorphic]
    public Bar bar2;
    [OnChange("Good")]
    [ReadOnly]
    [SerializeReference, Polymorphic]
    public Bar bar22;
    [OnChange("Good")]
    public Bar eee;

    public void OnIntChanged(int v, int pv)
    {
        Debug.Log($"from {pv} to {v}");
    }
    public void Good()
    {
        Debug.Log($"Good");
    }
    void ArrayCha()
    {
        Debug.Log($"ArrayCha");
    }
    // Start is called before the first frame update
    void Start()
    {
    }
}
