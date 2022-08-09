using UnityEngine;
using UnityEngine.UI;

namespace NASAsMarsPhotos
{
    [HelpURL("https://assetstore.unity.com/packages/slug/227800")]
    public class Screen : MonoBehaviour
    {
        [SerializeField]
        private Renderer sreenRenderer;

        [SerializeField]
        private Text text;

        /// <summary>
        /// Setting the texture
        /// </summary>
        /// <param name="texture2D"></param>
        public void SetTexture(Texture2D texture2D)
        {
            sreenRenderer.material.mainTexture = texture2D;
        }

        /// <summary>
        /// Setting up photo information
        /// </summary>
        /// <param name="date"></param>
        /// <param name="id"></param>
        /// <param name="sol"></param>
        public void SetTextData(string date, int id, int sol)
        {
            text.text = "Id: " + id + "\nDate: " + date + "\nSol: " + sol;
        }

        /// <summary>
        /// Instance Initialization
        /// </summary>
        private void Start()
        {
            text.text = string.Empty;
            gameObject.SetActive(false);
        }
    }
}