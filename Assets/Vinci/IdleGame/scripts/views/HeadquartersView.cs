using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;
using Solana.Unity.SDK.Nft;
using System;
using Solana.Unity.Wallet;
using System.Threading.Tasks;


public class HeadquartersView : View
{
    [SerializeField]
    private Button _backButton;
    [SerializeField]
    private Button _unstakeButton;

    [SerializeField]
    private LoaderPopup _loaderPopup;

    [SerializeField]
    GameObject nftSlotPrefab;
    [SerializeField]
    GameObject nftSlotRoot;

    [SerializeField]
    DraggableItem draggableItemPrefab;

    [SerializeField]
    List<Slot> slots;

    public Slot slot;

    public event Action<int, string, DraggableItem> nftStakeOnSlot;
    public event Action backButtonPressed;

    PublicKey stakedNftKey; //new PublicKey("G7zPrFKAEHkGPqKUhLSV4L95XECviMFgJbr3DT38BkWk");

    private bool isStaked = false;

    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => backButtonPressed?.Invoke());
        _unstakeButton.onClick.AddListener(OnUnstakeNft);

        _loaderPopup.gameObject.SetActive(false);
        slot.OnDropNft += OnDropNft;
    }

    async public void OnUnstakeNft()
    {
        try{

            stakedNftKey = await BlockchainManager.instance.GetStakeEntries();

            RemoveAllChildObjects(nftSlotRoot.transform);
            ShowLoaderPopup("Unstaking nft");
            string results = await BlockchainManager.instance.UnStakeNft(stakedNftKey);
            //await Task.Delay(200);
            Debug.Log("unstake nft: " + results);


            Transform nftItem = slot.transform.GetChild(0);
            DraggableItem dragItem = nftItem.gameObject.GetComponent<DraggableItem>();

            GameObject slotObj = Instantiate(nftSlotPrefab, nftSlotRoot.transform);
            Slot _slot = slotObj.GetComponent<Slot>();
            slot.id = 0;
            DraggableItem itemSlot = _slot.GetComponentInChildren<DraggableItem>();
            itemSlot.image.sprite = dragItem.image.sprite;
            itemSlot.pubkeyNft = dragItem.pubkeyNft;
            itemSlot.image.raycastTarget = true;

            Destroy(nftItem.gameObject);

            CloseLoaderPopup();
            _unstakeButton.gameObject.SetActive(false);
            isStaked = false;
        }catch(Exception e)
        {
            Debug.Log("Unable to unstake nft: " + e.Message);
            CloseLoaderPopup();
        }

    }

    async public Task LoadNfts()
    {

        stakedNftKey = await BlockchainManager.instance.GetStakeEntries();
        

        int id = 0;
        ShowLoaderPopup("Loading Nfts");
        Debug.Log("Load Nfts");
        List<Nft> nfts = await BlockchainManager.instance.GetWalletNfts();

        if (nfts == null)
        {
            CloseLoaderPopup();
            return;
        }

        PublicKey creator = BlockchainManager.instance.GetCreator();

        RemoveAllChildObjects(slot.transform);
        RemoveAllChildObjects(nftSlotRoot.transform);


        foreach (var nft in nfts)
        {

            //TODO: REMOVE HARDCODED pkey
            if (nft.metaplexData.data.metadata.creators[0].key != "9tKEKFaUndFTuKRmiR9HJfBmMK9nDuYJDA43q2pwimxj"
                && nft.metaplexData.data.metadata.name != "Vinci Guardian"
            ) continue;

            //stakedNftKey = new PublicKey(nft.metaplexData.data.mint);
            iNftFile<Texture2D> texture = nft.metaplexData.nftImage;
            if (stakedNftKey == nft.metaplexData.data.mint)
            {

                Debug.Log("nft.metaplexData.data.mint:" + nft.metaplexData.data.mint);
                isStaked = true;
                Sprite sprite = TextureToSprite(texture.file);
                DraggableItem item = Instantiate(draggableItemPrefab, slot.transform);
                item.image.sprite = sprite;
                item.pubkeyNft = nft.metaplexData.data.mint;
                item.image.raycastTarget = false;
                _unstakeButton.gameObject.SetActive(true);
                continue;
            }

           
            if (texture != null)
            {
                Sprite sprite = TextureToSprite(texture.file);

                GameObject slotObj = Instantiate(nftSlotPrefab, nftSlotRoot.transform);

                Slot slot = slotObj.GetComponent<Slot>();
                slot.id = id--;
                slot.OnDropNft += OnDropNft;
                DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
                item.image.sprite = sprite;
                item.pubkeyNft = nft.metaplexData.data.mint;
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

    public void OnDropNft(int id, string pubkey, DraggableItem item)
    {

        Debug.Log("DROP NFT: id: " + id + " pkey: " + pubkey);
        if (isStaked) return;

        nftStakeOnSlot?.Invoke(id, pubkey, item);

        item.image.raycastTarget = false;
        _unstakeButton.gameObject.SetActive(true);
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

    void ChangeParentOfFirstChild(Transform originalParent, Transform newParentTransform)
    {
        if (originalParent.childCount > 0)
        {
            Transform firstChild = originalParent.GetChild(0);
            firstChild.SetParent(newParentTransform);
        }
        else
        {
            Debug.LogWarning("The original parent has no children.");
        }
    }

    void RemoveAllChildObjects(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
    }
}
