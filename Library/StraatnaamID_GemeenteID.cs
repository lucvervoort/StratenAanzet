namespace Domain.Library
{
    public class StraatnaamID_GemeenteID
    {
        public int StraatnaamID { get; set; }
        public int GemeenteID { get; set; }

        public StraatnaamID_GemeenteID(int straatnaamID, int gemeenteID)
        {
            StraatnaamID = straatnaamID;
            GemeenteID = gemeenteID;
        }
    }
}
