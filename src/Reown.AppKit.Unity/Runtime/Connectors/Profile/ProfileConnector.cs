using System;
using System.Linq;
using Newtonsoft.Json;
using Reown.Sign.Models;
using UnityEngine;

namespace Reown.AppKit.Unity.Profile
{
    public class ProfileConnector : WalletConnectConnector
    {
        public string Email { get; private set; }

        public Account[] SmartAccounts { get; private set; }

        public Account[] EoaAccounts { get; private set; }

        public AccountType PreferredAccountType { get; private set; } = AccountType.None;

        public Account PreferredAccount { get; private set; }

        private const string PreferredAccountTypeKey = "PreferredAccount";

        public ProfileConnector()
        {
            Type = ConnectorType.Profile;
        }

        protected override async void OnAccountConnected(AccountConnectedEventArgs e)
        {
            try
            {
                var addressProvider = SignClient.AddressProvider;
                var sessionProperties = addressProvider.DefaultSession.SessionProperties;

                SmartAccounts = JsonConvert
                    .DeserializeObject<string[]>(sessionProperties["smartAccounts"])
                    .Select(x => new Account(x))
                    .ToArray();
                
                var allAccounts = await GetAccountsAsyncCore();
                EoaAccounts = allAccounts
                    .Except(SmartAccounts)
                    .ToArray();

                Email = sessionProperties["email"];

                base.OnAccountConnected(e);

                var preferredAccountTypeStr = PlayerPrefs.GetString(PreferredAccountTypeKey);
                
                var preferredAccountType = Enum.TryParse<AccountType>(preferredAccountTypeStr, out var accountType)
                    ? accountType
                    : AccountType.SmartAccount;
                SetPreferredAccount(preferredAccountType);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public void SetPreferredAccount(AccountType accountType)
        {
            SetPreferredAccountCore(accountType);
            OnAccountChanged(new AccountChangedEventArgs(GetCurrentAccount()));
        }

        private void SetPreferredAccountCore(AccountType accountType)
        {
            if (AppKit.NetworkController.ActiveChain == null)
                return;

            var chainId = AppKit.NetworkController.ActiveChain.ChainId;

            // Find preferred account for the current chain
            PreferredAccount = accountType == AccountType.SmartAccount
                ? SmartAccounts.First(a => a.ChainId == chainId)
                : EoaAccounts.First(a => a.ChainId == chainId);
            
            PreferredAccountType = accountType;

            // The preferred account type is saved so it can be recovered after the session is resumed
            PlayerPrefs.SetString(PreferredAccountTypeKey, accountType.ToString());
        }

        protected override Account GetCurrentAccount()
        {
            return PreferredAccount != default
                ? PreferredAccount
                : base.GetCurrentAccount();
        }
    }

    public enum AccountType
    {
        None,
        SmartAccount,
        Eoa
    }

    public static class AccountTypeExtensions
    {
        public static string ToFriendlyString(this AccountType accountType)
        {
            return accountType switch
            {
                AccountType.SmartAccount => "Smart Account",
                AccountType.Eoa => "EOA",
                _ => "Unknown"
            };
        }

        public static string ToShortString(this AccountType accountType)
        {
            return accountType switch
            {
                AccountType.SmartAccount => "SA",
                AccountType.Eoa => "EOA",
                _ => "Unknown"
            };
        }
    }
}