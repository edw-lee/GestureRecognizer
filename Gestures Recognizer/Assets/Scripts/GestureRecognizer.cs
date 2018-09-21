using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureRecognizer : MonoBehaviour {

    [SerializeField]
    private GameObject nodeObj, parentNode;

    [SerializeField]
    private int textureBits = 64; /*bits of texture*/
    
    [SerializeField, Range(1, 8)]
    private int pixelsPerNodeFactor = 4; /*factor of pixels per node (preferably multiples of 2)*/

    private int pixelsPerNode; /*Pixel thickness of each node*/

    [SerializeField]
    private Image gestureDisplay;

    [SerializeField]
    private Color textureColor = Color.white,
        gestureColor = Color.black;

    [SerializeField]
    private Vector2 pivot = new Vector2(0.5f, 0.5f);

    public void StartRecognizer(List<Vector2> nodes)
    {
        //Set pixels per node based on factor
        pixelsPerNode = textureBits / pixelsPerNodeFactor;

        //Preprocessing
        OptimizeNodes(ref nodes);
        DrawGestureTexture(nodes, Color.black);

        //--------For visualization--------------//
        //TranslateNodesToCenter(ref nodes);
        //DrawNodes(nodes, Color.green, true);
    }

    void DrawNodes(List<Vector2> nodes, Color color, bool drawCenter)
    {
        //Destroy all existing gameobject nodes before instantiating new ones
        foreach (Transform child in parentNode.transform)
            Destroy(child.gameObject);

        Vector2 center = GetNodesOrigin(nodes);

        foreach (Vector2 node in nodes)
        {
            GameObject newNode = Instantiate(nodeObj, node, nodeObj.transform.rotation, parentNode.transform);
            newNode.GetComponent<SpriteRenderer>().color = color;
            newNode.GetComponent<SpriteRenderer>().sortingOrder--;
        }

        if (drawCenter)
            Instantiate(nodeObj, center, nodeObj.transform.rotation, parentNode.transform);

    }

    //Optimize nodes distance to more constant interval based on pixelspernode(pixel thickness)
    void OptimizeNodes(ref List<Vector2> nodes)
    {
        List<Vector2> optimizedNodesList = new List<Vector2>();

        optimizedNodesList.Add(nodes[0]); //use first point as starting point

        float interval = GetTotalNodesLength(nodes) / ((nodes.Count - 1) * pixelsPerNode); //get interval. maxNodes - 1 because interval is 1 lesser than points     

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
        if (optimizedNodesList.Count == textureBits - 1)
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

    void NodesScaling(List<Vector2> nodes, int scale)
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
            x = scale;
            y = (maxVector.y - minVector.y) * scale / (maxVector.x - minVector.x);
        }
        else//Vertical rect
        {
            y = scale;
            x = (maxVector.x - minVector.x) * scale / (maxVector.y - minVector.y);
        }

        Vector2 ratio = new Vector2(x, y);

        //Scale vector
        for (int i = 0; i < nodes.Count; i++)
            nodes[i] = (nodes[i] - minVector) * ratio / (maxVector - minVector);
    }

    void DrawGestureTexture(List<Vector2> nodes, Color color)
    {
        Texture2D gestureTexture;

        //Initialize texture
        if (gestureDisplay.sprite == null)
        {
            gestureTexture = new Texture2D(textureBits, textureBits);
            gestureTexture.filterMode = FilterMode.Point;
            gestureTexture.wrapMode = TextureWrapMode.Clamp;
            SetTextureColor(ref gestureTexture, textureColor);
        }
        else
            gestureTexture = gestureDisplay.sprite.texture;
        

        //Convert nodes position to [0..1] scale
        List<Vector2> nodesClone = new List<Vector2>(nodes);//To prevent the original nodes list from being changed
        NodesScaling(nodesClone, 1);

        for(int i = 0; i < nodesClone.Count; i++)
        {
            //Set pixels offset based on pixels covered by each node(thickness)
            int pixelOffset = pixelsPerNode / 2;

            //Set texture size. (Subtract pixel offset to prevent right and top edge pixels from being cropped off)
            int textureSize = textureBits - pixelOffset; 

            for (int y = 0; y < pixelOffset; y ++)
            {
                for(int x = 0; x < pixelOffset; x++)
                {
                    //Convert nodes position to pixel position
                    int pixelX = Mathf.RoundToInt(nodesClone[i].x * textureSize),
                    pixelY = Mathf.RoundToInt(nodesClone[i].y * textureSize);

                    gestureTexture.SetPixel(pixelX + x, pixelY + y, color);
                }
            }
        }

        gestureTexture.Apply();

        //Create new sprite
        Rect rect = new Rect(0, 0, gestureTexture.width, gestureTexture.height);
        Vector2 pivot = Vector2.zero;
        Sprite sprite = Sprite.Create(gestureTexture, rect, pivot);

        gestureDisplay.sprite = sprite;
    }

    public void ClearGestureTexture()
    {
        Texture2D clearTexture = new Texture2D(textureBits, textureBits);

        //Initialize texture
        clearTexture.filterMode = FilterMode.Point;
        clearTexture.wrapMode = TextureWrapMode.Clamp;

        //Clear texture
        SetTextureColor(ref clearTexture, textureColor);

        //Create new sprite
        Rect rect = new Rect(0, 0, clearTexture.width, clearTexture.height);
        Vector2 pivot = Vector2.zero;
        Sprite sprite = Sprite.Create(clearTexture, rect, pivot);

        gestureDisplay.sprite = sprite;
    }

    //Sets whole texture color
    void SetTextureColor(ref Texture2D texture, Color color)
    {
        for(int x = 0; x < texture.width; x++)
        {
            for(int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
    }

    void FindGesture(List<Vector2> nodes)
    {
        
    }


    //----------Exta functions for debugging and visualization purposes----------//

    Vector2 GetNodesOrigin(List<Vector2> nodes)
    {
        Vector2 minVector, maxVector;
        GetMinMaxNodes(nodes, out minVector, out maxVector);

        return minVector + (maxVector - minVector) * pivot;
    }

    //Translate node to origin based on pivot
    void TranslateNodesToCenter(ref List<Vector2> nodes)
    {
        Vector2 center = GetNodesOrigin(nodes);

        for (int i = 0; i < nodes.Count; i++)
            nodes[i] -= center + Vector2.zero;//Actual Origin = (0,0)
    }
}
