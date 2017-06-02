using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class GameSaver : MonoBehaviour
{
    #region INITIAL_DATA

    [SerializeField]
    string fileName;

    System.Text.StringBuilder strBuilder = new System.Text.StringBuilder(2);

    #endregion

    public PlayerData LoadGame()
    {
        // save path to the file on load
        strBuilder.Append(Application.persistentDataPath);
        strBuilder.Append(fileName);

        // if save exists, then read data from it
        if (File.Exists(strBuilder.ToString()))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(strBuilder.ToString(), FileMode.Open);

            PlayerData playerData = new PlayerData();
            playerData = (PlayerData)bf.Deserialize(file);
            file.Close();
            return playerData;
        }
        return null;
    }

    public void SaveGame(PlayerData playerData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = null;

        // open or create file and serialize data to it
        file = File.Open(strBuilder.ToString(), FileMode.OpenOrCreate);
        bf.Serialize(file, playerData);
        file.Close();
    }
}

[System.Serializable]
public class PlayerData
{
    public string bestScore = "0";
    public string currentScore = "0";

    public int currentCubeSequenceLength;
    public int nextCubeSequenceLength;

    public List<string> currentCubeSequenceColors = new List<string>(4);
    public List<string> nextCubeSequenceColors = new List<string>(4);

    public List<int> indexesOfFixedCells = new List<int>(25);
    public List<string> gridCellsColors = new List<string>(25);
}
