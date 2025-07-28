using System;
using System.Collections.Generic;

namespace NeonLadder.Mechanics.Progression
{
    public interface IUpgradeSystem
    {
        bool PurchaseUpgrade(string upgradeId, CurrencyType currencyType);
        bool CanAffordUpgrade(string upgradeId, CurrencyType currencyType);
        bool HasUpgrade(string upgradeId);
        IUpgrade GetUpgrade(string upgradeId);
        IEnumerable<IUpgrade> GetAvailableUpgrades(CurrencyType currencyType);
        void ResetMetaUpgrades();
        void ApplyUpgradeEffects();
        
        event Action<IUpgrade> OnUpgradePurchased;
    }
}