using System;
using System.Threading.Tasks;
using Unity.AI.Toolkit.Accounts.Services.Core;
using Unity.AI.Toolkit.Accounts.Services.Data;
using Unity.AI.Toolkit.Connect;

namespace Unity.AI.Toolkit.Accounts.Services.States
{
    public class PointsBalanceState
    {
        internal readonly Signal<PointsBalanceRecord> settings;

        public event Action OnChange;
        public PointsBalanceRecord Value { get => settings.Value; internal set => settings.Value = value; }
        public void Refresh() => settings.Refresh();
        public Task RefreshPointsBalance() => RefreshInternal();

        public bool HasAny => Value?.PointsAvailable > 0;

        public PointsBalanceState()
        {
            settings = new(AccountPersistence.PointsBalanceProxy, () => _ = RefreshInternal(), () => OnChange?.Invoke());
            Refresh();
            AIDropdownBridge.ConnectProjectStateChanged(Refresh);
            AIDropdownBridge.ConnectStateChanged(Refresh);
            AIDropdownBridge.UserStateChanged(Refresh);
        }

        async Task RefreshInternal()
        {
            var result = await AccountApi.GetPointsBalance();
            if (result == null)
                Value = null;
            else
                Value = new(result);
        }
    }
}
