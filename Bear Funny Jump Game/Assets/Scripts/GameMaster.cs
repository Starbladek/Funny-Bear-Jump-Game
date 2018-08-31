using UnityEngine;

/*
    TODO:
    -Fix climbing functionality
    -Make the game check for if extending back down after a jump will put it inside another, causing it to clip through and fall
    -Separate all code into multiple files
    -Automate colliders applying to ledges with the correct width and Y position
*/

public class GameMaster : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}