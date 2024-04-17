namespace Domain.Library
{
    public class Gemeente
    {
        #region Properties
        public int GemeenteNaamId { get; set; }
        public int GemeenteId { get; set; }
        public string? TaalCode { get; set; }
        public string? GemeenteNaam { get; set; }
        #endregion

        #region Constructors
        public Gemeente(int gemeenteId, string gemeenteNaam)
        {
            GemeenteId = gemeenteId;
            GemeenteNaam= gemeenteNaam;

            GemeenteNaamId = 0;
            TaalCode = null;
        }

        public Gemeente(int gemeenteNaamId, int gemeenteId, string taalCode, string gemeenteNaam)
        {
            GemeenteNaamId = gemeenteNaamId;
            GemeenteId = gemeenteId;
            TaalCode = taalCode;
            GemeenteNaam = gemeenteNaam;
        }
        #endregion
    }
}