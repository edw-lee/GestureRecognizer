using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureRecognizer : MonoBehaviour {

    [SerializeField]
    private GameObject nodeObj, parentNode;

    [SerializeField]
    private int vectorScale = 1, numberOfNodes = 64;//in bits

    [SerializeField]
    private Image gestureDisplay;

    [SerializeField]
    private Vector2 pivot = new Vector2(0.5f, 0.5f);

    public void StartRecognizer(List<Vector2> nodes)
    {
        foreach (Transform child in parentNode.transform)
            Destroy(child.gameObject);

        OptimizeNodes(ref nodes);
        NodesScaling(nodes);

        DrawGestureTexture(nodes, Color.black);

        FindGesture(nodes);

        DrawNodes(nodes, Color.green);
    }

    void DrawNodes(List<Vector2> nodes, Color color)
    {
        foreach (Vector2 node in nodes)
        {
            GameObject newNode = Instantiate(nodeObj, node, nodeObj.transform.rotation);
            newNode.GetComponent<SpriteRenderer>().color = color;
            newNode.transform.parent = parentNode.transform;
        }
    }

    void NodesScaling(List<Vector2> nodes)
    {
        Vector2 maxVector, minVector;
        GetMinMaxNodes(nodes, out minVector, out maxVector);

        if (maxVector.x == minVector.x)
        {
            if (maxVector.x < 0)
                maxVector.x = 0;
            else
                minVector.x = 0;
        }

        if (maxVector.y == minVector.y)
        {
            if (maxVector.y < 0)
                maxVector.y = 0;
            else
                minVector.y = 0;
        }

        float x, y;
        if ((maxVector.y - minVector.y) < (maxVector.x - minVector.x))//Width longer than height - horizontal rect
        {
            x = vectorScale;
            y = (maxVector.y - minVector.y) * vectorScale / (maxVector.x - minVector.x);
        }
        else//Vertical rect
        {
            y = vectorScale;
            x = (maxVector.x - minVector.x) * vectorScale / (maxVector.y - minVector.y);
        }

        Vector2 ratio = new Vector2(x, y);

        //Scale vector
        for (int i = 0; i < nodes.Count; i++)
            nodes[i] = (nodes[i]-minVector) * ratio / (maxVector - minVector);
    }

    void GetMinMaxNodes(List<Vector2> nodes, out Vector2 min, out Vector2 max)
    {
        //Sort vector x value in ascending order
        nodes.Sort(delegate (Vector2 a, Vector2 b) { return (a.x.CompareTo(b.x)); });
        max.x = nodes[nodes.Count - 1].x;
        min.x = nodes[0].x;

        //Sort vector y value in ascending order
        nodes.Sort(delegate (Vector2 a, Vector2 b) { return (a.y.CompareTo(b.y)); });
        max.y = nodes[nodes.Count - 1].y;
        min.y = nodes[0].y;
    }
    //Optimize nodes to a set number of nodes
    void OptimizeNodes(ref List<Vector2> nodes)
    {
        List<Vector2> optimizedNodesList = new List<Vector2>();

        optimizedNodesList.Add(nodes[0]); //use first point as starting point

        float interval = GetTotalNodesLength(nodes) / (numberOfNodes - 1); //get interval. maxNodes - 1 because interval is 1 lesser than points     

        float totalNodesDistance = 0.0f;
        for(int i = 1; i < nodes.Count; i++)
        {
            float currNodesDistance = Vector2.Distance(nodes[i - 1], nodes[i]);
            totalNodesDistance += currNodesDistance;
            
            if(totalNodesDistance >= interval)
            {
                float distanceRatio = (totalNodesDistance - interval)/currNodesDistance;
                float x = nodes[i].x - distanceRatio * (nodes[i].x - nodes[i - 1].x); //x = x0 - ratio * distanceOfX
                float y = nodes[i].y - distanceRatio * (nodes[i].y - nodes[i - 1].y); //y = y0 - ratio * distanceOfY

                Vector2 newNode = new Vector2(x, y);

                optimizedNodesList.Add(newNode);

                nodes.Insert(i, newNode);

                totalNodesDistance = 0.0f;
            }
        }

        //If rounding error occur where number of nodes is lesser by 1, add the last node
        if (optimizedNodesList.Count == numberOfNodes - 1)
            optimizedNodesList.Add(nodes[nodes.Count - 1]);

        nodes = optimizedNodesList;
    }

    float GetTotalNodesLength(List<Vector2> vectors)
    {
        float length = 0.0f;
        for(int i = 0; i < vectors.Count - 1; i++)
        {
            length += Vector2.Distance(vectors[i], vectors[i + 1]);
        }

        return length;
    }

    Vector2 SetNodesPivot(List<Vector2> nodes)
    {
        Vector2 minVector, maxVector;
        GetMinMaxNodes(nodes, out minVector, out maxVector);
        
        return minVector + (maxVector - minVector) * pivot;
    }

    void TranslateNodesPosition(ref List<Vector2> nodes)
    {
        Vector2 center = SetNodesPivot(nodes);

        for (int i = 0; i < nodes.Count; i++)
            nodes[i] -= center + Vector2.zero;//Origin = (0,0)
    }

    void DrawGestureTexture(List<Vector2> nodes, Color color)
    {
        int textureSize = vectorScale * numberOfNodes;
        Texture2D gestureTexture = new Texture2D(textureSize, textureSize);
        gestureTexture.filterMode = FilterMode.Point;
        gestureTexture.wrapMode = TextureWrapMode.Clamp;

        SetTextureColor(ref gestureTexture, Color.white);

        for(int i = 0; i < nodes.Count; i++)
        {
            int x = Mathf.RoundToInt(nodes[i].x * numberOfNodes),
                y = Mathf.RoundToInt(nodes[i].y * numberOfNodes);

            gestureTexture.SetPixel(x, y, color);
        }

        gestureTexture.Apply();

        Rect rect = new Rect(0, 0, gestureTexture.width, gestureTexture.height);
        Vector2 pivot = Vector2.zero;
        Sprite sprite = Sprite.Create(gestureTexture, rect, pivot);

        gestureDisplay.sprite = sprite;
    }

    void SetTextureColor(ref Texture2D texture, Color color)
    {
        for(int x = 0; x < texture.width; x++)
        {
            for(int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    void FindGesture(List<Vector2> nodes)
    {
        
    }
}
