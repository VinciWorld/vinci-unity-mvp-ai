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
using VinciAccounts;
using VinciAccounts.Program;
using VinciAccounts.Errors;
using VinciAccounts.Accounts;
using VinciAccounts.Types;

namespace VinciAccounts
{
    namespace Accounts
    {
        public partial class BaseAccount
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 9648973883725994512UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{16, 90, 130, 242, 159, 10, 232, 133};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "3jehgKVjiZr";
            public ulong TotalAmount { get; set; }

            public PublicKey Owner { get; set; }

            public byte Bump { get; set; }

            public byte Level { get; set; }

            public UserDetails[] SpareStruct { get; set; }

            public static BaseAccount Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                BaseAccount result = new BaseAccount();
                result.TotalAmount = _data.GetU64(offset);
                offset += 8;
                result.Owner = _data.GetPubKey(offset);
                offset += 32;
                result.Bump = _data.GetU8(offset);
                offset += 1;
                result.Level = _data.GetU8(offset);
                offset += 1;
                int resultSpareStructLength = (int)_data.GetU32(offset);
                offset += 4;
                result.SpareStruct = new UserDetails[resultSpareStructLength];
                for (uint resultSpareStructIdx = 0; resultSpareStructIdx < resultSpareStructLength; resultSpareStructIdx++)
                {
                    offset += UserDetails.Deserialize(_data, offset, out var resultSpareStructresultSpareStructIdx);
                    result.SpareStruct[resultSpareStructIdx] = resultSpareStructresultSpareStructIdx;
                }

                return result;
            }
        }

        public partial class Tournament
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 6645556528406825903UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{175, 139, 119, 242, 115, 194, 57, 92};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "WN1FxbBd1hD";
            public PublicKey Owner { get; set; }

            public TournamentStruct[] TournamentList { get; set; }

            public uint PrizePool { get; set; }

            public static Tournament Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Tournament result = new Tournament();
                result.Owner = _data.GetPubKey(offset);
                offset += 32;
                int resultTournamentListLength = (int)_data.GetU32(offset);
                offset += 4;
                result.TournamentList = new TournamentStruct[resultTournamentListLength];
                for (uint resultTournamentListIdx = 0; resultTournamentListIdx < resultTournamentListLength; resultTournamentListIdx++)
                {
                    offset += TournamentStruct.Deserialize(_data, offset, out var resultTournamentListresultTournamentListIdx);
                    result.TournamentList[resultTournamentListIdx] = resultTournamentListresultTournamentListIdx;
                }

                result.PrizePool = _data.GetU32(offset);
                offset += 4;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum VinciAccountsErrorKind : uint
        {
            InsufficientBalanceSpl = 6000U,
            InsufficientBalanceSol = 6001U,
            WrongSigner = 6002U,
            WrongPDA = 6003U,
            WrongBump = 6004U
        }
    }

    namespace Types
    {
        public partial class UserDetails
        {
            public uint Ammount { get; set; }

            public PublicKey UserAddress { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WriteU32(Ammount, offset);
                offset += 4;
                _data.WritePubKey(UserAddress, offset);
                offset += 32;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out UserDetails result)
            {
                int offset = initialOffset;
                result = new UserDetails();
                result.Ammount = _data.GetU32(offset);
                offset += 4;
                result.UserAddress = _data.GetPubKey(offset);
                offset += 32;
                return offset - initialOffset;
            }
        }

        public partial class TournamentStruct
        {
            public PublicKey User { get; set; }

            public uint Score { get; set; }

            public int Serialize(byte[] _data, int initialOffset)
            {
                int offset = initialOffset;
                _data.WritePubKey(User, offset);
                offset += 32;
                _data.WriteU32(Score, offset);
                offset += 4;
                return offset - initialOffset;
            }

            public static int Deserialize(ReadOnlySpan<byte> _data, int initialOffset, out TournamentStruct result)
            {
                int offset = initialOffset;
                result = new TournamentStruct();
                result.User = _data.GetPubKey(offset);
                offset += 32;
                result.Score = _data.GetU32(offset);
                offset += 4;
                return offset - initialOffset;
            }
        }
    }

    public partial class VinciAccountsClient : TransactionalBaseClient<VinciAccountsErrorKind>
    {
        public VinciAccountsClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<BaseAccount>>> GetBaseAccountsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = BaseAccount.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<BaseAccount>>(res);
            List<BaseAccount> resultingAccounts = new List<BaseAccount>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => BaseAccount.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<BaseAccount>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Tournament>>> GetTournamentsAsync(string programAddress, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Tournament.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Tournament>>(res);
            List<Tournament> resultingAccounts = new List<Tournament>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Tournament.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Tournament>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<BaseAccount>> GetBaseAccountAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<BaseAccount>(res);
            var resultingAccount = BaseAccount.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<BaseAccount>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Tournament>> GetTournamentAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Tournament>(res);
            var resultingAccount = Tournament.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Tournament>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeBaseAccountAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, BaseAccount> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                BaseAccount parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = BaseAccount.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeTournamentAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Tournament> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Tournament parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Tournament.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<RequestResult<string>> SendStartStuffOffAsync(StartStuffOffAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.StartStuffOff(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendMintTokenAsync(MintTokenAccounts accounts, ulong ammount, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.MintToken(accounts, ammount, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendBurnTokenAsync(BurnTokenAccounts accounts, ulong ammount, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.BurnToken(accounts, ammount, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendClaimTokensAsync(ClaimTokensAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.ClaimTokens(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendAddAmmountAsync(AddAmmountAccounts accounts, ulong ammount, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.AddAmmount(accounts, ammount, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendRemoveAmmountAsync(RemoveAmmountAccounts accounts, ulong ammount, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.RemoveAmmount(accounts, ammount, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendStartTournamentAsync(StartTournamentAccounts accounts, uint prizePool, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.StartTournament(accounts, prizePool, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendAddTournamentParticipantAsync(AddTournamentParticipantAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.AddTournamentParticipant(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendPayTournamentAsync(PayTournamentAccounts accounts, ulong ammount, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.PayTournament(accounts, ammount, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendMintNftAsync(MintNftAccounts accounts, PublicKey creatorKey, string uri, string title, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.MintNft(accounts, creatorKey, uri, title, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendSeasonRewardsAsync(SeasonRewardsAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.SeasonRewards(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        public async Task<RequestResult<string>> SendCloseAccountAsync(CloseAccountAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solana.Unity.Rpc.Models.TransactionInstruction instr = Program.VinciAccountsProgram.CloseAccount(accounts, programId);
            return await SignAndSendTransaction(instr, feePayer, signingCallback);
        }

        protected override Dictionary<uint, ProgramError<VinciAccountsErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<VinciAccountsErrorKind>>{{6000U, new ProgramError<VinciAccountsErrorKind>(VinciAccountsErrorKind.InsufficientBalanceSpl, "Insufficient Balance - SPL")}, {6001U, new ProgramError<VinciAccountsErrorKind>(VinciAccountsErrorKind.InsufficientBalanceSol, "Insufficient Balance - SOL")}, {6002U, new ProgramError<VinciAccountsErrorKind>(VinciAccountsErrorKind.WrongSigner, "Wrong Signer")}, {6003U, new ProgramError<VinciAccountsErrorKind>(VinciAccountsErrorKind.WrongPDA, "Invalid Quiz PDA")}, {6004U, new ProgramError<VinciAccountsErrorKind>(VinciAccountsErrorKind.WrongBump, "Invalid Quiz Bump")}, };
        }
    }

    namespace Program
    {
        public class StartStuffOffAccounts
        {
            public PublicKey User { get; set; }

            public PublicKey BaseAccount { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class MintTokenAccounts
        {
            public PublicKey TokenProgram { get; set; }

            public PublicKey Mint { get; set; }

            public PublicKey TokenAccount { get; set; }

            public PublicKey Payer { get; set; }
        }

        public class BurnTokenAccounts
        {
            public PublicKey TokenProgram { get; set; }

            public PublicKey Mint { get; set; }

            public PublicKey TokenAccount { get; set; }

            public PublicKey Payer { get; set; }
        }

        public class ClaimTokensAccounts
        {
            public PublicKey TokenProgram { get; set; }

            public PublicKey Mint { get; set; }

            public PublicKey TokenAccount { get; set; }

            public PublicKey BaseAccount { get; set; }

            public PublicKey Payer { get; set; }
        }

        public class AddAmmountAccounts
        {
            public PublicKey BaseAccount { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class RemoveAmmountAccounts
        {
            public PublicKey BaseAccount { get; set; }

            public PublicKey Owner { get; set; }
        }

        public class StartTournamentAccounts
        {
            public PublicKey User { get; set; }

            public PublicKey Tournament { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class AddTournamentParticipantAccounts
        {
            public PublicKey User { get; set; }

            public PublicKey TournamentList { get; set; }

            public PublicKey NewParticipant { get; set; }
        }

        public class PayTournamentAccounts
        {
            public PublicKey User { get; set; }
        }

        public class MintNftAccounts
        {
            public PublicKey Mint { get; set; }

            public PublicKey TokenAccount { get; set; }

            public PublicKey MintAuthority { get; set; }

            public PublicKey Metadata { get; set; }

            public PublicKey MasterEdition { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey Rent { get; set; }

            public PublicKey TokenProgram { get; set; }

            public PublicKey TokenMetadataProgram { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class SeasonRewardsAccounts
        {
            public PublicKey VinciQuiz { get; set; }

            public PublicKey Owner { get; set; }

            public PublicKey QuizProgram { get; set; }
        }

        public class CloseAccountAccounts
        {
            public PublicKey VinciAccount { get; set; }

            public PublicKey Destination { get; set; }
        }

        public static class VinciAccountsProgram
        {
            public static Solana.Unity.Rpc.Models.TransactionInstruction StartStuffOff(StartStuffOffAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BaseAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2360766785266202238UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction MintToken(MintTokenAccounts accounts, ulong ammount, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Mint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.TokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(4101212246258452908UL, offset);
                offset += 8;
                _data.WriteU64(ammount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction BurnToken(BurnTokenAccounts accounts, ulong ammount, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Mint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.TokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5351999914653558201UL, offset);
                offset += 8;
                _data.WriteU64(ammount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction ClaimTokens(ClaimTokensAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Mint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.TokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BaseAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(4623741067803678828UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction AddAmmount(AddAmmountAccounts accounts, ulong ammount, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BaseAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(15985251043751243299UL, offset);
                offset += 8;
                _data.WriteU64(ammount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction RemoveAmmount(RemoveAmmountAccounts accounts, ulong ammount, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BaseAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2015887454945968588UL, offset);
                offset += 8;
                _data.WriteU64(ammount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction StartTournament(StartTournamentAccounts accounts, uint prizePool, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Tournament, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(17427815840464545956UL, offset);
                offset += 8;
                _data.WriteU32(prizePool, offset);
                offset += 4;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction AddTournamentParticipant(AddTournamentParticipantAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.User, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TournamentList, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.NewParticipant, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5251640520228228533UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction PayTournament(PayTournamentAccounts accounts, ulong ammount, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.User, true)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(9091575001133976709UL, offset);
                offset += 8;
                _data.WriteU64(ammount, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction MintNft(MintNftAccounts accounts, PublicKey creatorKey, string uri, string title, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Mint, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.TokenAccount, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.MintAuthority, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Metadata, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.MasterEdition, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMetadataProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(18096548587977980371UL, offset);
                offset += 8;
                _data.WritePubKey(creatorKey, offset);
                offset += 32;
                offset += _data.WriteBorshString(uri, offset);
                offset += _data.WriteBorshString(title, offset);
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction SeasonRewards(SeasonRewardsAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.VinciQuiz, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Owner, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.QuizProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(11472877556975683937UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction CloseAccount(CloseAccountAccounts accounts, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.VinciAccount, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Destination, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(1749686311319895933UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}