namespace SIO_AgendaWPF.Models
{
    public class Classe
    {
        public int Id { get; set; }
        public string Libelle { get; set; }

        public override string ToString()
        {
            return Libelle;
        }
    }
}