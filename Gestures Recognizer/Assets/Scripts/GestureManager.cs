using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GestureRecognizer))]
public class GestureManager : MonoBehaviour {

    private GestureRecognizer gr;
    private List<Gesture> gestures;
    private string displayName;

    [SerializeField]
    private string gestureName;

    [SerializeField]
    private float threshold = 0.7f;

    private void Start()
    {
        gr = GetComponent<GestureRecognizer>();
        gestures = new List<Gesture>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
            FindGesture();

        if (Input.GetKeyUp(KeyCode.S))
            StoreGesture();
    }

    void StoreGesture()
    {
        BitArray bits = gr.GetBits();
        Gesture newGesture = new Gesture(gestureName, bits);

        gestures.Add(newGesture);

        print(newGesture.name + " gesture Stored. Count = " + gestures.Count);
    }

    void FindGesture()
    {
        //Initialize variables
        BitArray bits = gr.GetBits();
        float score = 0.0f;
        displayName = "No Match Found.";

        for (int i = 0; i < gestures.Count; i++)
        {
            float tempScore = MatchGesture(gestures[i].bits, bits);
            if (tempScore >= threshold)
            {
                if(tempScore > score)
                {
                    displayName = gestures[i].name;
                    score = tempScore; //To get the gesture with the highest score
                }
            }
        }
    }

    float MatchGesture(BitArray a, BitArray b)
    {
        int score = 0;
        for(int i = 0; i < a.Count; i++)
        {
            if (a[i] == b[i])
                score++;
        }

        return ((float)score/a.Count);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "S = Store Gesture, F = Find Gesture");
        GUI.Label(new Rect(10, 30, 300, 20), "Gesture = " + displayName);
    }
}
