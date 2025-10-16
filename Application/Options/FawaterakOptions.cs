using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Enums.EInvoiceRequestModel;

namespace Application.Options
{
    public sealed class FawaterakOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;


        //public RedirectionUrlsModel RedirectionUrls { get; set; }

        //public string successUrl { get; set; }
        //public string failUrl { get; set; }
        //public string pendingUrl { get; set; }
    }
}
