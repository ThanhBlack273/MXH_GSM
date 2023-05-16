using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MXH.MVT
{
    public class ObjectMVTExchangeVoucherSequence
    {
        public MVTAccount MVTAccount { get; set; }
        public MVTPromotionInfo PromotionInfo { get; set; }
    }
    public class MVTPromotionInfo
    {
        public int id { get; set; }
        public int partner { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string[] images { get; set; }
        public int? price { get; set; }
        public int? exchangePoint { get; set; }
        public int? originPoint { get; set; }
        public int? percent { get; set; }
        public int? giftPoint { get; set; }
        public int? billPercent { get; set; }
        public int? maxBillPoint { get; set; }
        public int? stamp { get; set; }
        public int? gift { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public int? like { get; set; }
        public int? view { get; set; }
        public int? comment { get; set; }
        public int? rate { get; set; }
        public int? totalRate { get; set; }
        public int? avgRate { get; set; }
        public bool? hot { get; set; }
        public int? codeType { get; set; }
        public int? codeTime { get; set; }
        public int? checkout { get; set; }
        public int? inventory { get; set; }
        public bool isApproved { get; set; }
        public string value { get; set; }
        public bool active { get; set; }
        public string hotline { get; set; }
        public string Provider { get; set; }
        public string[] thumbnail { get; set; }
        public bool canGetCode { get; set; }
        public string programTerm { get; set; }
    }
}
