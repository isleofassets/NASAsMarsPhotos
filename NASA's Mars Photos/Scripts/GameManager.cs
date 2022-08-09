using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NASAsMarsPhotos
{
    [HelpURL("https://assetstore.unity.com/packages/slug/227800")]
    public class GameManager : MonoBehaviour
    {
        private class JsonData
        {
            public string img_src, earth_date;
            public int id, sol;
        }

        [SerializeField]
        private Screen[] screens;

        [SerializeField]
        private GameObject hintText;

        [SerializeField]
        private InputField dateInputField;

        [SerializeField]
        private Text pageText;

        [SerializeField]
        private Texture2D defaultTexture;

        private string API_KEY = "DEMO_KEY";            // Use this site "https://api.nasa.gov" to get your own API, which will allow you to make up to 1000 requests per hour!
        private const string LATEST_PHOTOS_URL = "https://api.nasa.gov/mars-photos/api/v1/rovers/perseverance/latest_photos?api_key=";
        private const string DATE_PHOTOS_URL = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?api_key=";
        private const int LATEST_PHOTOS_SUBSTRING_START_INDEX = 18;
        private const int DATE_PHOTOS_SUBSTRING_START_INDEX = 11;

        private JsonData[] jsonDatas;
        private Coroutine loadImagesWait;
        private int pageIndex;

        /// <summary>
        /// Date selection
        /// </summary>
        public void OnEndEditDate()
        {
            if (dateInputField.text == string.Empty)
                return;
            string[] numbersStr = dateInputField.text.Split('-');
            if (numbersStr.Length != 3)
                return;
            string date = "&earth_date=";
            int[] numbers = new int[3];
            for (int i = 0; i < numbers.Length; i++)
            {
                if (!int.TryParse(numbersStr[i], out int number))
                    return;
                date += number.ToString() + (i < numbers.Length - 1 ? "-" : string.Empty);
            }
            if (loadImagesWait != null)
                StopCoroutine(loadImagesWait);
            pageIndex = 0;
            pageText.text = "Page: 1";
            StartCoroutine(Request(DATE_PHOTOS_URL, date, DATE_PHOTOS_SUBSTRING_START_INDEX));
        }

        /// <summary>
        /// Loading and installing textures
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadImages()
        {
            WaitForSeconds delay = new WaitForSeconds(0.1f);
            for (int i = 0; i < screens.Length; i++)
            {
                int index = pageIndex * screens.Length + i;
                if (index < jsonDatas.Length)
                {
                    if (jsonDatas == null)
                        yield break;
                    WWW www = new WWW(jsonDatas[index].img_src);
                    yield return www;
                    screens[i].SetTextData(jsonDatas[index].earth_date, jsonDatas[index].id, jsonDatas[index].sol);
                    screens[i].SetTexture(www.texture);
                }
                else
                {
                    yield return delay;
                    screens[i].SetTextData("---", 0, 0);
                    screens[i].SetTexture(defaultTexture);
                }
                if (!screens[i].gameObject.activeSelf)
                    screens[i].gameObject.SetActive(true);
            }
            if (hintText != null)
                hintText.SetActive(true);
            loadImagesWait = null;
        }

        /// <summary>
        /// Getting all the necessary information from NASA servers
        /// </summary>
        /// <returns></returns>
        private IEnumerator Request(string url, string date, int substringStartIndex)
        {
            WWW www = new WWW(url + API_KEY + date);
            yield return www;
            string[] blocks = www.text.Substring(substringStartIndex, www.text.Length - (substringStartIndex + 2)).Split(new string[] { "}}," }, System.StringSplitOptions.None);
            for (int i = 0; i < blocks.Length - 1; i++)
                blocks[i] = blocks[i] + "}}";
            jsonDatas = new JsonData[blocks.Length];
            try
            {
                for (int i = 0; i < blocks.Length; i++)
                    jsonDatas[i] = JsonUtility.FromJson<JsonData>(blocks[i]);
            }
            catch
            {
                Debug.LogError("Couldn't read the JSON file, maybe you haven't specified the API key yet");
                yield break;
            }
            loadImagesWait = StartCoroutine(LoadImages());
        }

        /// <summary>
        /// Uploading new photos
        /// </summary>
        private void NextPage()
        {
            if (loadImagesWait != null)
                StopCoroutine(loadImagesWait);
            if (hintText != null)
                Destroy(hintText);
            pageIndex++;
            if (pageIndex == Mathf.Ceil((float)jsonDatas.Length / screens.Length))
                pageIndex = 0;
            pageText.text = "Page: " + (pageIndex + 1).ToString();
            loadImagesWait = StartCoroutine(LoadImages());
        }

        /// <summary>
        /// Instance Initialization
        /// </summary>
        private void Start()
        {
            if (API_KEY == "DEMO_KEY")
                Debug.LogWarning("Please specify the API key, you can get it from the link https://api.nasa.gov");
            hintText.SetActive(false);
            StartCoroutine(Request(LATEST_PHOTOS_URL, "", LATEST_PHOTOS_SUBSTRING_START_INDEX));
        }

        /// <summary>
        /// Tracking space clicks
        /// </summary>
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
                return;
            NextPage();
        }
    }
}