// Models klasörüne koy: PersonelOnIzlemeViewModel.cs
namespace PersonelSystem.Models
{
    public class PersonelOnIzlemeViewModel
    {
        public Personel Personel { get; set; }

        public string SehirAdi { get; set; }
        public string UnvanAdi { get; set; }
        public string IseBaslamaTarihi { get; set; }
        public string FotoUrl { get; set; }  // Eğer gerekirse
    }
}
