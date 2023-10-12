using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Solana.Unity.Metaplex.NFT.Library;
using Solana.Unity.Metaplex.Utilities;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.SDK.Nft;
using Solana.Unity.Wallet;
using TMPro;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Rendering;
using Vinci.Core.Utils;
using VinciAccounts;
using VinciAccounts.Program;
using VinciStake.Program;

public class BlockchainManager : PersistentSingleton<BlockchainManager>
{
    [SerializeField]
    TextMeshProUGUI pubkey;
    [SerializeField]
    TextMeshProUGUI solanaBalance;

    PublicKey VinciAccountProgramId = new PublicKey("38N2x62nEqdgRf67kaemiBNFijKMdnqb3XyCa4asw2fQ");
    PublicKey VinciStakeProgramId = new PublicKey("EjhezvQjSDBEQXVyJSY1EhmqsQFGEorS7XwwHmxcRNxV");
    PublicKey SYSVAR_RENT_PUBKEY = new PublicKey("SysvarRent111111111111111111111111111111111");

    private void Start()
    {

    }

    private void OnEnable()
    {
        Web3.OnLogin += OnLogin;
        Web3.OnBalanceChange += OneBalanceChange;
    }

    private void OnDisable()
    {
        Web3.OnLogin -= OnLogin;
        Web3.OnBalanceChange -= OneBalanceChange;
    }

    private void OnLogin(Account account)
    {
        pubkey.text = account.PublicKey;
    }
/*
    private void OnWalletInstance()
    {
        Debug.Log("OnWalletInstance");
       
        Web3.OnBalanceChange += OneBalanceChange;
        Web3.OnWalletInstance += OnWalletInstance;
    }
*/
    private void OneBalanceChange(double sol)
    {
        solanaBalance.text = sol.ToString();
    }

    async public Task<List<Nft>> GetWalletNfts()
    {
       List<Nft> nfts = await Web3.LoadNFTs();

        if(nfts == null)
        {
            return null;
        }
       foreach (var nft in nfts)
       {
            Debug.Log(nft.metaplexData.data.metadata.name);
       }

        return nfts;
    }

    async public Task<string> StakeNft(string mintPubkey)
    {
        var nftKey = new PublicKey(mintPubkey);
        var masterEdition = PDALookup.FindMasterEditionPDA(nftKey);
        var metadataAccount = PDALookup.FindMetadataPDA(nftKey);
        var blockHash = await Web3.Rpc.GetLatestBlockHashAsync();

       

        PublicKey stakePool;
        PublicKey userStakeEntry;
        byte bump;

        PublicKey.TryFindProgramAddress(
            new[]
            {
                Encoding.UTF8.GetBytes("VinciStakePool"),
            },
            VinciStakeProgramId, out stakePool, out bump
        );

        PublicKey.TryFindProgramAddress(
            new[]
            {
                Encoding.UTF8.GetBytes("VinciStakeEntry"),
                Web3.Account.PublicKey.KeyBytes,
            },
            VinciStakeProgramId, out userStakeEntry, out bump

        );

        var associatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
            Web3.Account.PublicKey, nftKey
        );

        var stakeAccounts = new StakeNonCustodialAccounts();
        stakeAccounts.StakeEntry = userStakeEntry;
        stakeAccounts.StakePool = stakePool;
        stakeAccounts.OriginalMint = nftKey;
        stakeAccounts.OriginalMintMetadata = metadataAccount;
        stakeAccounts.MasterEdition = masterEdition;
        stakeAccounts.FromMintTokenAccount = associatedTokenAccount;
        stakeAccounts.ToMintTokenAccount = associatedTokenAccount;
        stakeAccounts.User = Web3.Account.PublicKey;
        stakeAccounts.TokenProgram = TokenProgram.ProgramIdKey;
        stakeAccounts.TokenMetadataProgram = MetadataProgram.ProgramIdKey;
        stakeAccounts.SystemProgram = SystemProgram.ProgramIdKey;

        TransactionInstruction instr = VinciStakeProgram.StakeNonCustodial(stakeAccounts, VinciStakeProgramId);

        var transaction = new TransactionBuilder()
            .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
            .SetFeePayer(Web3.Account)
            .AddInstruction(instr);

        var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account }));
        var res = await Web3.Wallet.SignAndSendTransaction(tx, true);

        return res.Result;
    }

    async public void UnStakeNft(string mintPubkey)
    {
        var nftKey = new PublicKey(mintPubkey);
        var masterEdition = PDALookup.FindMasterEditionPDA(nftKey);
        var metadataAccount = PDALookup.FindMetadataPDA(nftKey);
        var blockHash = await Web3.Rpc.GetLatestBlockHashAsync();



        PublicKey stakePool;
        PublicKey userStakeEntry;
        byte bump;

        PublicKey.TryFindProgramAddress(
            new[]
            {
                Encoding.UTF8.GetBytes("VinciStakePool"),
            },
            VinciStakeProgramId, out stakePool, out bump
        );

        PublicKey.TryFindProgramAddress(
            new[]
            {
                Encoding.UTF8.GetBytes("VinciStakeEntry"),
                Web3.Account.PublicKey.KeyBytes,
            },
            VinciStakeProgramId, out userStakeEntry, out bump

        );

        var associatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
            Web3.Account.PublicKey, nftKey
        );

        var stakeAccounts = new ClaimNonCustodialAccounts();
        stakeAccounts.StakeEntry = userStakeEntry;
        stakeAccounts.StakePool = stakePool;
        stakeAccounts.OriginalMint = nftKey;
        stakeAccounts.MasterEdition = masterEdition;
        stakeAccounts.FromMintTokenAccount = associatedTokenAccount;
        stakeAccounts.ToMintTokenAccount = associatedTokenAccount;
        stakeAccounts.User = Web3.Account.PublicKey;
        stakeAccounts.TokenProgram = TokenProgram.ProgramIdKey;
        stakeAccounts.TokenMetadataProgram = MetadataProgram.ProgramIdKey;

        TransactionInstruction instr = VinciStakeProgram.ClaimNonCustodial(
            stakeAccounts, VinciStakeProgramId);

        var transaction = new TransactionBuilder()
            .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
            .SetFeePayer(Web3.Account)
            .AddInstruction(instr);

        var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account }));
        var res = await Web3.Wallet.SignAndSendTransaction(tx, true);

    }

    public void CallMintModel()
    {
        //MainThreadDispatcher.Instance().EnqueueAsync(MintNNmodel);
    }

    async public Task<string> MintNNmodel()
    {
        string uri = "https://arweave.net/Pe4erqz3MZoywHqntUGZoKIoH0k9QUykVDFVMjpJ08s";
        var mintKey = new Account();

        var masterEdition = PDALookup.FindMasterEditionPDA(mintKey.PublicKey);
        var metadataAccount = PDALookup.FindMetadataPDA(mintKey.PublicKey);

        var associatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
            Web3.Account.PublicKey, mintKey.PublicKey
        );

        var rent = SYSVAR_RENT_PUBKEY;

        var asd = MetadataProgram.ProgramIdKey;
        var asas = SystemProgram.ProgramIdKey;

        PublicKey mintAuth;
        byte bump;

        PublicKey.TryFindProgramAddress(
            new[]
            {
                Encoding.UTF8.GetBytes("authority"),
            },
            VinciAccountProgramId, out mintAuth, out bump
        );


        var blockHash = await Web3.Rpc.GetLatestBlockHashAsync();
        var minimumRent = await Web3.Rpc.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);

        var mintNftAccounts = new MintNftAccounts();
        mintNftAccounts.Mint = mintKey;
        mintNftAccounts.TokenAccount = associatedTokenAccount;
        mintNftAccounts.MintAuthority = mintAuth;
        mintNftAccounts.Metadata = metadataAccount;
        mintNftAccounts.MasterEdition = masterEdition;
        mintNftAccounts.Payer = Web3.Account.PublicKey;
        mintNftAccounts.Rent = SYSVAR_RENT_PUBKEY;
        mintNftAccounts.TokenProgram = TokenProgram.ProgramIdKey;
        mintNftAccounts.TokenMetadataProgram = MetadataProgram.ProgramIdKey;
        mintNftAccounts.SystemProgram = SystemProgram.ProgramIdKey;

        TransactionInstruction instr = VinciAccountsProgram.MintNft(
            mintNftAccounts,
            uri,
            "Vinci NN Model",
            VinciAccountProgramId
        );

        var transaction = new TransactionBuilder()
            .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
            .SetFeePayer(Web3.Account)
            .AddInstruction(
                SystemProgram.CreateAccount(
                    Web3.Account.PublicKey,
                    mintKey.PublicKey,
                    minimumRent.Result,
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey))
            .AddInstruction(
                TokenProgram.InitializeMint(
                    mintKey.PublicKey,
                    0,
                    mintAuth,
                    mintAuth))
            .AddInstruction(
                AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    Web3.Account,
                    Web3.Account,
                    mintKey.PublicKey))
            .AddInstruction(instr);

        var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account, mintKey }));
        var res = await Web3.Wallet.SignAndSendTransaction(tx, true);
        Debug.Log(res.Result);

        VinciAccountsClient vinciAccountsClient = new VinciAccountsClient(
            Web3.Rpc,
            Web3.WsRpc,
            VinciAccountProgramId
        );

        return res.Result;


        /*
        var requestResult = await vinciAccountsClient.SendMintNftAsync(
                            mintNftAccounts,
                            new PublicKey("7qZkw6j9o16kqGugWTj4u8Lq9YHcPAX8dgwjjd9EYrhQ"),
                            uri,
                            "Vinci World EA",
                            Web3.Account.PublicKey,
                            Callback,
                            VinciAccountProgramId
                        );

        Debug.Log("ErrorData.Error: " + requestResult.ErrorData.Error + " ServerErrorCode:" + requestResult.ServerErrorCode + " WasSuccessful: " +
        requestResult.WasSuccessful + " Reason: " + requestResult.Reason + " Result: " + requestResult.Result + " ErrorData.Logs: " + requestResult.ErrorData.Logs
        );
        */
    }

    public byte[] Callback(byte[] bytes, PublicKey publicKey)
    {
        Debug.Log("FRom callback: " + publicKey + bytes.Length);
        return bytes;
    }

    public bool RegisterPlayerOnCompetition()
    {
        return true;
    }

    public void GetPlayeresScores()
    {

    }

    async public void RegisterPlayerScore(int score)
    {
        PublicKey userAccount;
        byte bump;
        var blockHash = await Web3.Rpc.GetLatestBlockHashAsync();

        PublicKey.TryFindProgramAddress(
            new[]
            {
                Encoding.UTF8.GetBytes("VinciWorldAccount1"),
                Web3.Account.PublicKey.KeyBytes,
            },
            VinciAccountProgramId, out userAccount, out bump
        );

        var setScoreAccounts = new SetScoreAccounts();
        setScoreAccounts.BaseAccount = userAccount;
        setScoreAccounts.Owner = Web3.Account.PublicKey;

        TransactionInstruction instr = VinciAccountsProgram.SetScore(
            setScoreAccounts, (ulong)score, VinciAccountProgramId
        );

        var transaction = new TransactionBuilder()
            .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
            .SetFeePayer(Web3.Account)
            .AddInstruction(instr);

        var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account }));
        var res = await Web3.Wallet.SignAndSendTransaction(tx, true);

    }

    public void GetOpenCompetitions()
    {

    }
}
