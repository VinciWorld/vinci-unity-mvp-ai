using System;
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

    public override void OnEnterState()
    {
        mainView = ViewManager.GetView<HeadquartersView>();
        ViewManager.Show(mainView);

        mainView.nftStakeOnSlot += OnDropNft;

        MainThreadDispatcher.Instance().EnqueueAsync(mainView.LoadNfts);
    
    }

    private void OnDropNft(int obj)
    {
        mainView.ShowLoaderPopup("Staking nft");
        //BlockchainManager.instance.StakeNft();
        mainView.CloseLoaderPopup();
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    async void OnAcademyBtnPressed()
    {
        await SceneLoader.instance.LoadScene("Academy");
    }

    async void OnArenaBtnPressed()
    {
        await SceneLoader.instance.LoadScene("Arena");
    }
}