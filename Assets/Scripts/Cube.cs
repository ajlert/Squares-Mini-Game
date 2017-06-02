using UnityEngine;
using UnityEngine.UI;

public class Cube : MonoBehaviour
{
    #region INITIAL_DATA

    [SerializeField]
    Image backgroundImg;
    [SerializeField]
    Vector2 sizeOfFirstCube;

    #endregion

    public void SetAsFirst()
    {
        // first cube will be a bit bigger than the others
        // just a marker for player which cube is the first one in the sequence
        this.GetComponent<RectTransform>().sizeDelta = sizeOfFirstCube;
    }

    public void SetColor(Color newColor)
    {
        backgroundImg.color = newColor;
    }

    public Color GetColor()
    {
        return backgroundImg.color;
    }
}
