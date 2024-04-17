namespace StratenApp.Models
{
    public class GemeenteProvincieInfo
    {
        #region Properties
        public int GemeenteID { get; set; }
        public int ProvincieID { get; set; }
        public string? TaalCode { get; set; }
        public string? ProvincieNaam { get; set; }
        #endregion

        public GemeenteProvincieInfo(int gemeenteID, int provincieID, string taalCode, string provincieNaam)
        {
            GemeenteID = gemeenteID;
            ProvincieID = provincieID;
            TaalCode = taalCode;
            ProvincieNaam = provincieNaam;
        }

    }
}
