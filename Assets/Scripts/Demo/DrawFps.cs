using UnityEngine;
using System.Collections.Generic;

public class DrawFps : MonoBehaviour
{

    float elapsedTime = 0F;
    int elapsedFrames = 0;
    int fps = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime > 1F)
        {
            fps = elapsedFrames;
            elapsedTime = 0F;
            elapsedFrames = 0;
        }
        else
            elapsedFrames++;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 200F, 150F), fps.ToString());
    }
}
