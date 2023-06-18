using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASController : MonoBehaviour
{
    public GameObject textCA, textSA, textNA, textN;

    private string actualAS = "";

    public string getActualSpace()
    {
        return actualAS;
    }

    public void updateAS(string space)
    {
        switch (space)
        {
            case "cApse":
                textN.SetActive(false);
                textNA.SetActive(false);
                textSA.SetActive(false);
                textCA.SetActive(true);
                actualAS = space;
                break;
            case "nave":
                textCA.SetActive(false);
                textNA.SetActive(false);
                textSA.SetActive(false);
                textN.SetActive(true);
                actualAS = space;
                break;
            case "nApse":
                textN.SetActive(false);
                textCA.SetActive(false);
                textSA.SetActive(false);
                textNA.SetActive(true);
                actualAS = space;
                break;
            case "sApse":
                textN.SetActive(false);
                textNA.SetActive(false);
                textCA.SetActive(false);
                textSA.SetActive(true);
                actualAS = space;
                break;
            default:
                // code block
                break;
        }
    }

    public void hideLabels()
    {
        textN.SetActive(false);
        textNA.SetActive(false);
        textCA.SetActive(false);
        textSA.SetActive(false);
    }

    public void showLabels()
    {
        updateAS(actualAS);
    }
}

