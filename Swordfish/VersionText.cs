using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour
{
    private void Start()
	{
		this.GetComponent<Text>().text = Application.productName + " " + Application.version;
    }
}
