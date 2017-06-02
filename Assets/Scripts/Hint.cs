using System.Collections.Generic;
using UnityEngine;

public class Hint : MonoBehaviour
{
    #region INITIAL_DATA

    [SerializeField]
    Transform nextCubedBlockContentTr;

    #endregion

    public void ShowNextCubedBlock(List<Cube> nextCubedBlock)
    {
        for (int i = 0; i < nextCubedBlock.Count; i++)
        {
            GameObject cube = Instantiate(nextCubedBlock[i].gameObject);
            cube.transform.SetParent(nextCubedBlockContentTr, false);

            // show cube as the next sequence was completely hidden
            cube.gameObject.SetActive(true);
        }
    }
}
