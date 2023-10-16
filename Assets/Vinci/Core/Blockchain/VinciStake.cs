using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Solana.Unity;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using VinciStake;
using VinciStake.Program;
using VinciStake.Errors;
using VinciStake.Accounts;
using VinciStake.Types;

namespace VinciStake
{
    namespace Accounts
    {
        public partial class StakeEntry
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 2906586042612678587UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{187, 127, 9, 35, 155, 68, 86, 40};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "YMx1BScecEs";
            public PublicKey Pool { get; set; }

            public ulong Amount { get; set; }

            public long LastStakedAt { get; set; }

            public BigInteger TotalStakeSeconds { get; set; }

            public StakeTime[] OriginalMintSecondsStruct { get; set; }

            public long? CooldownStartSeconds { get; set; }

            public long? LastUpdatedAt { get; set; }

            public byte Bump { get; set; }

            public static StakeEntry Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                StakeEntry result = new StakeEntry();
                result.Pool = _data.GetPubKey(offset);
                offset += 32;
                result.Amount = _data.GetU64(offset);
                offset += 8;
                result.LastStakedAt = _data.GetS64(offset);
                offset += 8;
                result.TotalStakeSeconds = _data.GetBigInt(offset, 16, false);
                offset += 16;
                int resultOriginalMintSecondsStructLength = (int)_data.GetU32(offset);
                offset += 4;
                result.OriginalMintSecondsStruct = new StakeTime[resultOriginalMintSecondsStructLength];
                for (uint resultOriginalMintSecondsStructIdx = 0; resultOriginalMintSecondsStructIdx < resultOriginalMintSecondsStructLength; resultOriginalMintSecondsStructIdx++)
                {
                    offset += StakeTime.Deserialize(_data, offset, out var resultOriginalMintSecondsStructresultOriginalMintSecondsStructIdx);
                    result.OriginalMintSecondsStruct[resultOriginalMintSecondsStructIdx] = resultOriginalMintSecondsStructresultOriginalMintSecondsStructIdx;
                }

                if (_data.GetBool(offset++))
                {
                    result.CooldownStartSeconds = _data.GetS64(offset);
                    offset += 8;
                }

                if (_data.GetBool(offset++))
                {
                    result.LastUpdatedAt = _data.GetS64(offset);
                    offset += 8;
                }

                result.Bump = _data.GetU8(offset);
                offset += 1;
                return result;
            }
        }

        public partial class StakePool
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 2089528729768174201UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{121, 34, 206, 21, 79, 127, 255, 28};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "MGAteRdkWBD";
            public ulong Identifier { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey[] RequiresCreators { get; set; }

            public PublicKey[] RequiresCollections { get; set; }

            public bool RequiresAuthorization { get; set; }

            public string OverlayText { get; set; }

            public string ImageUri { get; set; }

            public bool ResetOnStake { get; set; }

            public uint TotalStaked { get; set; }

            public uint? CooldownSeconds { get; set; }

            public uint? MinStakeSeconds { get; set; }

            public long? EndDate { get; set; }

            public bool? DoubleOrResetEnabled { get; set; }

            public uint? MaxStakeAmount { get; set; }

            public byte Bump { get; set; }

            public static StakePool Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                StakePool result = new StakePool();
                result.Identifier = _data.GetU64(offset);
                offset += 8;
                result.Authority = _data.GetPubKey(offset);
                offset += 32;
                int resultRequiresCreatorsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.RequiresCreators = new PublicKey[resultRequiresCreatorsLength];
                for (uint resultRequiresCreatorsIdx = 0; resultRequiresCreatorsIdx < resultRequiresCreatorsLength; resultRequiresCreatorsIdx++)
                {
                    result.RequiresCreators[resultRequiresCreatorsIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                int resultRequiresCollectionsLength = (int)_data.GetU32(offset);
                offset += 4;
                result.RequiresCollections = new PublicKey[resultRequiresCollectionsLength];
                for (uint resultRequiresCollectionsIdx = 0; resultRequiresCollectionsIdx < resultRequiresCollectionsLength; resultRequiresCollectionsIdx++)
                {
                    result.RequiresCollections[resultRequiresCollectionsIdx] = _data.GetPubKey(offset);
                    offset += 32;
                }

                result.RequiresAuthorization = _data.GetBool(offset);
                offset += 1;
                offset += _data.GetBorshString(offset, out var resultOverlayText);
                result.OverlayText = resultOverlayText;
                offset += _data.GetBorshString(offset, out var resultImageUri);
                result.ImageUri = resultImageUri;
                result.ResetOnStake = _data.GetBool(offset);
                offset += 1;
                result.TotalStaked = _data.GetU32(offset);
                offset += 4;
                if (_data.GetBool(offset++))
                {
                    result.CooldownSeconds = _data.GetU32(offset);
                    offset += 4;
                }

                if (_data.GetBool(offset++))
                {
                    result.MinStakeSeconds = _data.GetU32(offset);
                    offset += 4;
                }

                if (_data.GetBool(offset++))
                {
                    result.EndDate = _data.GetS64(offset);
                    offset += 8;
                }

                if (_data.GetBool(offset++))
                {
                    result.DoubleOrResetEnabled = _data.GetBool(offset);
                    offset += 1;
                }

                if (_data.GetBool(offset++))
                {
                    result.MaxStakeAmount = _data.GetU32(offset);
                    offset += 4;
                }

                result.Bump = _data.GetU8(offset);
                offset += 1;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum VinciStakeErrorKind : uint
        {
            MetadataAccountEmpty = 6000U,
            InvalidMintMetadata = 6001U,
            InvalidMint = 6002U,
            InvalidMintOwner = 6003U,
            MissingCreators = 6004U,
            InvalidStakePool = 6005U,
            OriginalMintNotClaimed = 6006U,
            MintAlreadyClaimed = 6007U,
            UnauthorizedSigner = 6008U
        }
    }

    namespace Types
    {
        public partial class StakeTime
        {
            public BigInteger Time { get; set; }

            public PublicKey Mint { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteBigInt(Time, offset, 16, true);
                offset += 16;
                _data.WritePubKey(Mint, offset);
                offset += 32;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out StakeTime result)
            {
                int offset = initialOffset;
                result = new StakeTime();
                result.Time = _data.GetBigInt(offset, 16, false);
                offset += 16;
                result.Mint = _data.GetPubKey(offset);
                offset += 32;
                return offset - initialOffset;
            }
        }
    }

    public partial class VinciStakeClient : TransactionalBaseClient<VinciStakeErrorKind>
    {
        public VinciStakeClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<StakeEntry>>> GetStakeEntrysAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = StakeEntry.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<StakeEntry>>(res);
            List<StakeEntry> resultingAccounts = new List<StakeEntry>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => StakeEntry.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<StakeEntry>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<StakePool>>> GetStakePoolsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = StakePool.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<StakePool>>(res);
            List<StakePool> resultingAccounts = new List<StakePool>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => StakePool.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<StakePool>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<StakeEntry>> GetStakeEntryAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<StakeEntry>(res);
            var resultingAccount = StakeEntry.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<StakeEntry>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<StakePool>> GetStakePoolAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<StakePool>(res);
            var resultingAccount = StakePool.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<StakePool>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeStakeEntryAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, StakeEntry> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                StakeEntry parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = StakeEntry.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeStakePoolAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, StakePool> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                StakePool parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = StakePool.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<RequestResult<string>> SendInitializeStakePoolAsync(InitializeStakePoolAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.InitializeStakePool(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendInitializeStakeEntryAsync(InitializeStakeEntryAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.InitializeStakeEntry(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendStakeAsync(StakeAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.Stake(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendStakeNonCustodialAsync(StakeNonCustodialAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.StakeNonCustodial(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendClaimStakeAsync(ClaimStakeAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.ClaimStake(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendClaimNonCustodialAsync(ClaimNonCustodialAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.ClaimNonCustodial(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendUpdateStakeAsync(UpdateStakeAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.UpdateStake(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendClaimRewardsAsync(ClaimRewardsAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.ClaimRewards(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCloseStakeEntryAsync(CloseStakeEntryAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.CloseStakeEntry(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCloseStakePoolAsync(CloseStakePoolAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciStakeProgram.CloseStakePool(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        protected override Dictionary<uint, ProgramError<VinciStakeErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<VinciStakeErrorKind>>{{6000U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.MetadataAccountEmpty, "Metadata Account is empty")}, {6001U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.InvalidMintMetadata, "Invalid Mint Metadata")}, {6002U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.InvalidMint, "Invalid Mint")}, {6003U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.InvalidMintOwner, "Invalid Mint Owner")}, {6004U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.MissingCreators, "Missing Creators")}, {6005U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.InvalidStakePool, "Invalid Stake Pool")}, {6006U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.OriginalMintNotClaimed, "Original Mint Not Claimed")}, {6007U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.MintAlreadyClaimed, "Staked Mint Already Claimed")}, {6008U, new ProgramError<VinciStakeErrorKind>(VinciStakeErrorKind.UnauthorizedSigner, "Unauthorized Signer")}, };
        }
    }

    namespace Program
    {
        public class InitializeStakePoolAccounts
        {
            public PublicKey StakePool { get; set; }

            public PublicKey User { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class InitializeStakeEntryAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey StakePoolAccount { get; set; }

            public PublicKey User { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class StakeAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey StakePool { get; set; }

            public PublicKey OriginalMint { get; set; }

            public PublicKey OriginalMintMetadata { get; set; }

            public PublicKey MasterEdition { get; set; }

            public PublicKey FromMintTokenAccount { get; set; }

            public PublicKey ToMintTokenAccount { get; set; }

            public PublicKey User { get; set; }

            public PublicKey TokenProgram { get; set; }

            public PublicKey SystemProgram { get; set; }

            public PublicKey TokenMetadataProgram { get; set; }
        }

        public class StakeNonCustodialAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey StakePool { get; set; }

            public PublicKey OriginalMint { get; set; }

            public PublicKey OriginalMintMetadata { get; set; }

            public PublicKey MasterEdition { get; set; }

            public PublicKey FromMintTokenAccount { get; set; }

            public PublicKey ToMintTokenAccount { get; set; }

            public PublicKey User { get; set; }

            public PublicKey TokenProgram { get; set; }

            public PublicKey SystemProgram { get; set; }

            public PublicKey TokenMetadataProgram { get; set; }
        }

        public class ClaimStakeAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey StakePool { get; set; }

            public PublicKey OriginalMint { get; set; }

            public PublicKey MasterEdition { get; set; }

            public PublicKey FromMintTokenAccount { get; set; }

            public PublicKey ToMintTokenAccount { get; set; }

            public PublicKey User { get; set; }

            public PublicKey TokenProgram { get; set; }

            public PublicKey TokenMetadataProgram { get; set; }
        }

        public class ClaimNonCustodialAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey StakePool { get; set; }

            public PublicKey OriginalMint { get; set; }

            public PublicKey MasterEdition { get; set; }

            public PublicKey FromMintTokenAccount { get; set; }

            public PublicKey ToMintTokenAccount { get; set; }

            public PublicKey User { get; set; }

            public PublicKey TokenProgram { get; set; }

            public PublicKey TokenMetadataProgram { get; set; }
        }

        public class UpdateStakeAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey StakePool { get; set; }
        }

        public class ClaimRewardsAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey VinciAccount { get; set; }

            public PublicKey Owner { get; set; }

            public PublicKey AccountsProgram { get; set; }

            public PublicKey RewardsProgram { get; set; }
        }

        public class CloseStakeEntryAccounts
        {
            public PublicKey StakeEntry { get; set; }

            public PublicKey Destination { get; set; }
        }

        public class CloseStakePoolAccounts
        {
            public PublicKey StakePool { get; set; }

            public PublicKey Destination { get; set; }
        }

        public static class VinciStakeProgram
        {
            public static Solana.Unity.Rpc.Models.TransactionInstruction InitializeStakePool(InitializeStakePoolAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePool, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5990987154433752368UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction InitializeStakeEntry(InitializeStakeEntryAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePoolAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(6229476509544972732UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction Stake(StakeAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePool, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.OriginalMint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.OriginalMintMetadata, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MasterEdition, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.FromMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.ToMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMetadataProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(7832834834166362318UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction StakeNonCustodial(StakeNonCustodialAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePool, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.OriginalMint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.OriginalMintMetadata, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MasterEdition, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.FromMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.ToMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMetadataProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(10750262761081227100UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction ClaimStake(ClaimStakeAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePool, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.OriginalMint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MasterEdition, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.FromMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.ToMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMetadataProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(10030989668264546622UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction ClaimNonCustodial(ClaimNonCustodialAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePool, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.OriginalMint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MasterEdition, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.FromMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.ToMintTokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMetadataProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(18133708538662497564UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction UpdateStake(UpdateStakeAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.StakePool, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(1398800794702409976UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction ClaimRewards(ClaimRewardsAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.VinciAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.AccountsProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardsProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5807136032701059076UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CloseStakeEntry(CloseStakeEntryAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakeEntry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Destination, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2354958373024786224UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CloseStakePool(CloseStakePoolAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.StakePool, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Destination, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(285556047662812151UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}