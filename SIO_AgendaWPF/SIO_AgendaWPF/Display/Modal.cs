using SIO_AgendaWPF.Models;
using System;
using System.Linq;
using System.Windows;
using static SIO_AgendaWPF.MainWindow;

namespace SIO_AgendaWPF.Display
{
    public class DisplayModal
    {
        private static readonly MainWindow window = new();

        // Public

        public static void CloseModal()
        {
            window.Cvs_Libelle.Visibility = Visibility.Hidden;
            window.Cvs_Description.Visibility = Visibility.Hidden;
            window.Cvs_Date.Visibility = Visibility.Hidden;
            window.Cvs_Classe.Visibility = Visibility.Hidden;
            window.Cvs_Matiere.Visibility = Visibility.Hidden;
            window.Bdr_Modal.Visibility = Visibility.Hidden;
        }

        public static void OpenModal(string libelle, Matiere matiere, Classe classe, string description, DateTime? date, Methodes editable)
        {
            window.Bdr_Modal.Visibility = Visibility.Visible;
            window.ContentModal.Focus();
            window.Bdr_FenModal.Uid = ((int)editable).ToString();
            window.Btn_SaveModal.Visibility = editable != Methodes.Selectionner ? Visibility.Visible : Visibility.Collapsed;
            window.Txb_Libelle.Text = libelle;
            window.Dpc_Date.SelectedDate = matiere == null ? null : new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
            window.Cmb_Matiere.SelectedItem = matiere == null ? null : ((Matiere[])window.Cmb_Matiere.ItemsSource).First(x => x.Id == matiere.Id);
            window.Cmb_Classe.SelectedItem = classe == null ? null : ((Classe[])window.Cmb_Classe.ItemsSource).First(x => x.Id == classe.Id);
            window.Txb_Description.Text = description;
            window.Txb_Libelle.IsReadOnly = editable == Methodes.Selectionner;
            window.Dpc_Date.IsEnabled = editable != Methodes.Selectionner;
            window.Cmb_Classe.IsEnabled = editable != Methodes.Selectionner;
            window.Cmb_Matiere.IsEnabled = editable != Methodes.Selectionner;
            window.Txb_Description.IsReadOnly = editable == Methodes.Selectionner;
        }
    }
}
