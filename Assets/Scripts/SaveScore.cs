using UnityEngine;

//<summary>
// This script is responsible for saving the score of the player. Saving it to a file.
// </summary>
public class SaveScore
{
    //In this function, we save the score to a file.
    public void SaveScoreToFile(int score)
    {
        string path = Application.persistentDataPath + "/score.txt"; // Path to save the score file
        System.IO.File.WriteAllText(path, score.ToString()); // Write the score to the file
        Debug.Log("Score saved to: " + path); // Log the path where the score is saved
    }

    //in this script, we load the score from a file, if there's not a file, or its corrupted, returns 0.
    public int LoadScoreFromFile()
    {
        string path = Application.persistentDataPath + "/score.txt"; // Path to load the score file
        if (System.IO.File.Exists(path))
        {
            string scoreString = System.IO.File.ReadAllText(path); // Read the score from the file
            if (int.TryParse(scoreString, out int score))
            {
                return score; // Return the loaded score
            }
            else
            {
                Debug.LogError("Failed to parse score from file. Returning 0.");
                return 0; // Return 0 if parsing fails
            }
        }
        else
        {
            Debug.LogWarning("Score file not found. Returning 0.");
            return 0; // Return 0 if the file does not exist
        }
    }
}
