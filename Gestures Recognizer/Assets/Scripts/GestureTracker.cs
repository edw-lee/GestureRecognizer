using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GestureRecognizer))]
public class GestureTracker : MonoBehaviour {

    private Vector2 mousePos;
    private Camera cam;
	private GestureRecognizer gr;

    private List<Vector2> nodes = new List<Vector2>();

    [SerializeField]
    private float nodesMinMagnitude = 0.5f;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("UI Camera").GetComponent<Camera>();
        gr = GetComponent<GestureRecognizer>();
    }

	void Update()
    {
        MouseTrackFunction();
    }

    Vector2 prevMousePos;

    private void MouseTrackFunction()
    {
        if (Input.GetMouseButtonDown(0))
             prevMousePos = mousePos;

        if (Input.GetMouseButton(0))
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if((mousePos - prevMousePos).magnitude >= nodesMinMagnitude)
            {
                nodes.Add(mousePos);
                prevMousePos = mousePos;
            } 
        }
  
        if(Input.GetMouseButtonUp(0))
        {
            gr.StartRecognizer(nodes);
            nodes.Clear();
        }
    }
}
