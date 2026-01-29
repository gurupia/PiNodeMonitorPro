namespace PiNodeMonitorWinForm.Core.MVP.Models
{
    public class WalletData
    {
        public decimal Balance { get; set; }
        public double PriceUsd { get; set; }
        public double PriceKrw { get; set; }
        public double TotalValueUsd { get; set; }
        public double TotalValueKrw { get; set; }
        public string PublicKey { get; set; }
    }
}
