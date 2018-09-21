using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {

    [SerializeField]
    private GameObject nodeObj;

    private Camera cam;

    public GameObject trailPrefab;

    private List<GameObject> prevTrails = new List<GameObject>();

    void Start()
    {
        Cursor.visible = false;
        cam = GameObject.FindGameObjectWithTag("UI Camera").GetComponent<Camera>();
    }

	void Update () 
    {
        MouseFollow();
        DestroyTrails();
	}

    private void MouseFollow()
    {
        GameObject newTrail;

        transform.position = cam.ScreenToWorldPoint(Input.mousePosition);
        
        if(Input.GetMouseButtonDown(0))
        {
            newTrail = Instantiate(trailPrefab);
            newTrail.transform.position = transform.position;
            newTrail.transform.parent = transform;
        }
        
        if(Input.GetMouseButtonUp(0))
        {
            GameObject childTrail = transform.GetChild(0).gameObject;
            prevTrails.Add(childTrail);

            childTrail.GetComponent<Animator>().enabled = true;

            transform.DetachChildren();
        }
    }

    private void DestroyTrails()
    {
        if (prevTrails.Count == 0)
            return;

        GameObject temp = null;

        for (int i = 0; i < prevTrails.Count; i++)
        {
            if (prevTrails[i].GetComponent<SpriteRenderer>().color.a <= 0)
            {
                temp = prevTrails[i];
                break;
            }
        }

        prevTrails.Remove(temp);
        Destroy(temp);
    }
}
