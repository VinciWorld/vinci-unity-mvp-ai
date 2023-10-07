using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Solana.Unity.Metaplex.NFT.Library;
using Solana.Unity.Metaplex.Utilities;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Unity.Barracuda;
using UnityEngine;
using Vinci.Core.Utils;
using VinciAccounts;
using VinciAccounts.Program;

public class BlockchainManager : PersistentSingleton<BlockchainManager>
{

    PublicKey VinciAccountProgramId = new PublicKey("38N2x62nEqdgRf67kaemiBNFijKMdnqb3XyCa4asw2fQ");
    PublicKey VinciStakeProgramId = new PublicKey("EjhezvQjSDBEQXVyJSY1EhmqsQFGEorS7XwwHmxcRNxV");
    PublicKey SYSVAR_RENT_PUBKEY = new PublicKey("SysvarRent111111111111111111111111111111111");

    public void GetWalletNfts()
    {

    }

    public void StakeNft()
    {





                /*
                var userStakeEntry = PublicKey.TryFindProgramAddress(
                      new[]
                            {
                                Encoding.UTF8.GetBytes("VinciStakeEntry"),
                                Web3.Account.PublicKey.KeyBytes,
                            },
                );

                */






        var metadata = new Metadata()
        {
            name = "NNModel",
            symbol = "VinciNN",
            
            sellerFeeBasisPoints = 0,
            creators = new List<Creator> { new(Web3.Account.PublicKey, 100, true) }
        };
    }

    public void CallMintModel()
    {
        MainThreadDispatcher.Instance().EnqueueAsync(MintNNmodel);
    }

    async public void MintNNmodel()
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


        var blockHash = await Web3.Rpc.GetLatestBlockHashAsync();
        var minimumRent = await Web3.Rpc.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);

        var mintNftAccounts = new MintNftAccounts();
        mintNftAccounts.Mint = mintKey;
        mintNftAccounts.TokenAccount = associatedTokenAccount;
        mintNftAccounts.MintAuthority = Web3.Account.PublicKey;
        mintNftAccounts.Metadata = metadataAccount;
        mintNftAccounts.MasterEdition = masterEdition;
        mintNftAccounts.Payer = Web3.Account.PublicKey;
        mintNftAccounts.Rent = SYSVAR_RENT_PUBKEY;
        mintNftAccounts.TokenProgram = TokenProgram.ProgramIdKey;
        mintNftAccounts.TokenMetadataProgram = MetadataProgram.ProgramIdKey;
        mintNftAccounts.SystemProgram = SystemProgram.ProgramIdKey;

        TransactionInstruction instr = VinciAccountsProgram.MintNft(
            mintNftAccounts, new PublicKey("7qZkw6j9o16kqGugWTj4u8Lq9YHcPAX8dgwjjd9EYrhQ"), uri, "Vinci World EA", VinciAccountProgramId
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
                    Web3.Account.PublicKey,
                    Web3.Account.PublicKey))
            .AddInstruction(
                AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    Web3.Account,
                    Web3.Account,
                    mintKey.PublicKey))
            .AddInstruction(instr);

        var tx = Transaction.Deserialize(transaction.Build(new List<Account> { Web3.Account, mintKey }));
        var res = await Web3.Wallet.SignAndSendTransaction(tx);

        VinciAccountsClient vinciAccountsClient = new VinciAccountsClient(
            Web3.Rpc,
            Web3.WsRpc,
            VinciAccountProgramId
        );

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

    public void GetOpenCompetitions()
    {

    }

    public void RegisterOnCompetition()
    {

    }


}
