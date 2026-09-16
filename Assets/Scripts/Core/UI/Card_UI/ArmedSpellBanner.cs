using UnityEngine;
using TMPro;

public class ArmedSpellBanner : MonoBehaviour
{
    public TextMeshProUGUI spellNameText;
    private CardData mySpellData;

    public void Setup(CardData spellData)
    {
        mySpellData = spellData;
        if (spellNameText != null)
        {
            spellNameText.text = spellData.nomCarte;
        }
    }

    // À RELIER AU BOUTON 'X' DANS UNITY
    public void OnCancelClicked()
    {
        if (GameManager.Instance.CurrentState == GameState.PreparationPhase)
        {
            DeckManager.Instance.UnarmSpell(mySpellData);
            Destroy(gameObject); 
        }
    }
}