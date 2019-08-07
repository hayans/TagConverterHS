using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagConverter
{
    internal class DocuSignProps : Base
    {

        public DocuSignProps(ApiClient apiClient) : base(apiClient) { }

        internal void CreateCustomTabsFromCSV(String tabType, String tabLabel, String tabToolTip, String tabAnchor)
        {

            CheckToken();

            CustomTabsApi customTab = new CustomTabsApi();
            TabMetadata meta = new TabMetadata();
            meta.Type = tabType;
            meta.TabLabel = tabLabel;
            meta.Name = tabToolTip;//Optional
            meta.Anchor = tabAnchor;
         
            customTab.Create(AccountID, meta);

        }
    }
}
