using UnityEngine;

public class TESt : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool testtrue = false;
    public void TestTrue(){
        testtrue = !testtrue;
    }
    public void OnGUI() {
        if(testtrue) { if(GUI.Button(new Rect(0,0,80,20), ("Hello world"))){
            print("hello");

        }}
    }
}
