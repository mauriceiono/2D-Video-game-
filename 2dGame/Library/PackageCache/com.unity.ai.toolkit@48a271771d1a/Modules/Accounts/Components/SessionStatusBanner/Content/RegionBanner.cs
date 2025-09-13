using System;
using UnityEngine.UIElements;

namespace Unity.AI.Toolkit.Accounts.Components
{
    [UxmlElement]
    public partial class RegionBanner : BasicBannerContent
    {
        public RegionBanner() : base("Unity AI services are not available in your region.") { }
    }
}
