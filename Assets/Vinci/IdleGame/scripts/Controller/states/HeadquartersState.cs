using System;
using System.Threading.Tasks;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class HeadquartersState : StateBase
{
    IdleGameController _controller;
    HeadquartersView mainView;

    public HeadquartersState(IdleGameController controller)
    {
        _controller = controller;
    }

    async public override void OnEnterState()
    {
        mainView = ViewManager.GetView<HeadquartersView>();
        ViewManager.Show(mainView);

        mainView.nftStakeOnSlot += OnDropNft;
        mainView.backButtonPressed += OnBackButtonPressed;

        await mainView.LoadNfts();

    }

    public override void OnExitState()
    {
        mainView.nftStakeOnSlot -= OnDropNft;
        mainView.backButtonPressed -= OnBackButtonPressed;
    }


    public override void Tick(float deltaTime)
    {

    }

    private void OnBackButtonPressed()
    {
        _controller.SwitchState(new IdleGameState(_controller));
    }

    private async void OnDropNft(int slotId, string pubkey, DraggableItem itme)
    {
        try
        {
            Debug.Log("nft pubkey: " + pubkey);
            mainView.ShowLoaderPopup("Staking nft");
            await BlockchainManager.instance.StakeNft(pubkey);
            mainView.CloseLoaderPopup();
            itme.image.raycastTarget = false;

        }
        catch (Exception e)
        {
            Debug.Log("Unable to unstake nft: " + e.Message);
            mainView.CloseLoaderPopup();
        }
    }

    async void OnAcademyBtnPressed()
    {
        OnExitState();
        await SceneLoader.instance.LoadScene("Academy");
    }

    async void OnArenaBtnPressed()
    {
        OnExitState();
        await SceneLoader.instance.LoadScene("Arena");
    }
}