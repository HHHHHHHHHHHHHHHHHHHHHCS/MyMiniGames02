using UnityEngine;
using Random = UnityEngine.Random;

namespace SmashCSS
{
    public class AutomaticSlotSelection : MonoBehaviour
    {
        private void Start()
        {
            RectTransform artworkTS = transform.Find("Artwork").GetComponent<RectTransform>();
            Vector2 artworkOriginalSize = artworkTS.sizeDelta;

            int random = Random.Range(0, SmashCSSMono.instance.characters.Count - 1);

            Character randomChar = SmashCSSMono.instance.characters[random];
            
            SmashCSSMono.instance.ShowCharacterInSlot(transform.GetSiblingIndex(),randomChar);

            artworkTS.sizeDelta = artworkOriginalSize * randomChar.zoom;
        }
    }
}
