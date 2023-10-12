using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;
using Solana.Unity.SDK.Nft;
using System;

public class HeadquartersView : View
{
    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private LoaderPopup _loaderPopup;

    [SerializeField]
    GameObject nftSlotPrefab;
    [SerializeField]
    GameObject nftSlotRoot;

    [SerializeField]
    List<Slot> slots;

    public event Action<int> nftStakeOnSlot;

    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => ViewManager.ShowLast());

        _loaderPopup.gameObject.SetActive(false);
    }


    public async void LoadNfts()
    {
        int id = 0;
        ShowLoaderPopup("Loading Nfts");
        Debug.Log("Load Nfts");
        List<Nft> nfts = await BlockchainManager.instance.GetWalletNfts();

        if (nfts == null)
        {
            CloseLoaderPopup();
            return;
        }

        foreach (var nft in nfts)
        {
            iNftFile<Texture2D> texture = nft.metaplexData.nftImage;
            if (texture != null)
            {
                Sprite sprite = TextureToSprite(texture.file);

                GameObject slotObj = Instantiate(nftSlotPrefab, nftSlotRoot.transform);

                Slot slot = slotObj.GetComponent<Slot>();
                slot.id = id--;
                slot.OnDropNft += OnDropNft;
                DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
                item.image.sprite = sprite;

            }
        }

        CloseLoaderPopup();
    }

    public void ShowLoaderPopup(string messange)
    {
        _loaderPopup.gameObject.SetActive(true);
        _loaderPopup.SetProcessingMEssage(messange);
        _loaderPopup.Open();
    }

    public void CloseLoaderPopup()
    {
        _loaderPopup.Close();
    }

    public void OnDropNft(int id)
    {
        Debug.Log("DROP NFT: id: " + id);
        nftStakeOnSlot?.Invoke(id);
    }

    Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        return null;
    }

    Sprite TextureToSprite(Texture2D texture)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }
}
