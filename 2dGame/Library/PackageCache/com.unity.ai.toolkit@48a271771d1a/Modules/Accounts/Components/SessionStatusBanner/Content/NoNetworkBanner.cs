using System;
using UnityEngine.UIElements;

namespace Unity.AI.Toolkit.Accounts.Components
{
    [UxmlElement]
    public partial class NoNetworkBanner : BasicBannerContent
    {
        public NoNetworkBanner() : base("No internet connection. Check your network settings and try again.") { }
    }
}
