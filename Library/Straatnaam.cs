namespace Domain.Library
{
    public class Straatnaam
    {
        #region Properties
        public int Id { get; set; }
        public string? Naam { get; set; }
        #endregion

        #region Constructors
        public Straatnaam()
        {

        }

        public Straatnaam(int straatnaamId, string straat)
        {
            Id = straatnaamId;
            Naam = straat;
        }
        #endregion
    }
}
