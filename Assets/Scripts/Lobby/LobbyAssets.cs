using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite evilWizardSprite;
    [SerializeField] private Sprite huntressSprite;
    [SerializeField] private Sprite martialHeroSprite;
    [SerializeField] private Sprite wizardSprite;
    


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManager.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManager.PlayerCharacter.EvilWizard:   return evilWizardSprite;
            case LobbyManager.PlayerCharacter.Huntress:    return huntressSprite;
            case LobbyManager.PlayerCharacter.MartialArtist:   return martialHeroSprite;
            case LobbyManager.PlayerCharacter.Wizard:   return wizardSprite;
        }
    }

}