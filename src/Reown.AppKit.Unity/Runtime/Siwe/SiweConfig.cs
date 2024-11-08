using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reown.Sign.Models.Cacao;

namespace Reown.AppKit.Unity
{
    public class SiweConfig
    {
        /// <summary>
        ///     The getNonce method functions as a safeguard against spoofing, akin to a CSRF token.
        ///     The nonce can be generated locally with <see cref="SiweUtils.GenerateNonce" />,
        ///     or you can utilize an existing CSRF token from your backend if available.
        /// </summary>
        public Func<ValueTask<string>> GetNonce { get; set; }

        /// <summary>
        ///     Returns parameters that are used to create the SIWE message internally.
        /// </summary>
        public Func<SiweMessageParams> GetMessageParams { get; set; }

        /// <summary>
        ///     Generates an EIP-4361-compatible message.
        ///     You can use our provided <see cref="SiweUtils.FormatMessage" /> method or implement your own.
        /// </summary>
        public Func<SiweCreateMessageArgs, string> CreateMessage { get; set; }

        /// <summary>
        ///     Ensures the message is valid, has not been tampered with, and has been appropriately signed by the wallet address.
        /// </summary>
        public Func<SiweVerifyMessageArgs, ValueTask<bool>> VerifyMessage { get; set; }

        /// <summary>
        ///     Called after <see cref="VerifyMessage" /> succeeds.
        ///     The backend session should store the associated address and chainIds and return it via the <see cref="GetSession" /> method.
        /// </summary>
        public Func<GetSiweSessionArgs, ValueTask<SiweSession>> GetSession { get; set; }

        /// <summary>
        ///     Called when the wallet disconnects if <see cref="SignOutOnWalletDisconnect" /> is true,
        ///     and/or when the account changes if <see cref="SignOutOnAccountChange" /> is true,
        ///     and/or when the network changes if <see cref="SignOutOnChainChange" /> is true.
        /// </summary>
        public Func<ValueTask> SignOut { get; set; }

        public bool Enabled { get; set; } = true;

        public bool SignOutOnWalletDisconnect { get; set; } = true;

        public bool SignOutOnAccountChange { get; set; } = true;

        public bool SignOutOnChainChange { get; set; } = true;

        public event Action<SiweSession> SignInSuccess;

        public event Action SignOutSuccess;

        internal void OnSignInSuccess(SiweSession session)
        {
            SignInSuccess?.Invoke(session);
        }

        internal void OnSignOutSuccess()
        {
            SignOutSuccess?.Invoke();
        }
    }

    public class SiweMessageParams
    {
        public string Domain { get; set; }
        public string Uri { get; set; }

        public string Statement { get; set; }
    }

    public class SiweCreateMessageArgs
    {
        public string ChainId { get; set; }
        public string Domain { get; set; }
        public string Nonce { get; set; }
        public string Uri { get; set; }
        public string Address { get; set; }
        public string Version { get; set; } = "1";
        public string Nbf { get; set; }
        public string Exp { get; set; }
        public string Type { get; set; }
        public string Statement { get; set; }
        public long RequestId { get; set; }
        public List<string> Resources { get; set; }
        public int Expiry { get; set; }
        public string Iat { get; set; }

        public SiweCreateMessageArgs(SiweMessageParams messageParams)
        {
            Domain = messageParams.Domain;
            Uri = messageParams.Uri;
            Statement = messageParams.Statement;
        }
    }

    public class SiweVerifyMessageArgs
    {
        public string Message { get; set; }
        public string Signature { get; set; }
        public CacaoObject Cacao { get; set; }
    }

    public class GetSiweSessionArgs
    {
        public string Address { get; set; }
        public string[] ChainIds { get; set; }
    }

    public class SiweMessage
    {
        public string Message { get; set; }
        public SiweCreateMessageArgs CreateMessageArgs { get; set; }
    }

    [Serializable]
    public class SiweSession
    {
        public string EthAddress { get; set; } // Ethereum (0x...) address
        public string[] EthChainIds { get; set; } // Ethereum chain IDs

        public SiweSession()
        {
        }

        public SiweSession(GetSiweSessionArgs args)
        {
            EthAddress = args.Address;
            EthChainIds = args.ChainIds;
        }
    }
}