using SIO_AgendaWPF.Models;
using SIO_AgendaWPF.Properties;
using SIO_AgendaWPF.Repositories;
using SIO_AgendaWPF.Extensions;
using SIO_AgendaWPF.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Timer = System.Timers.Timer;

namespace SIO_AgendaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum Methodes { Selectionner = 0, Ajouter = 1, Modifier = 2 }
        public double _HeightPnlDevoirs;

        private IAgendaRepository _Repository;
        private bool _ShowOld;
        private Timer _Timer;
        private List<Devoir> _ActualDevoirs;
        private List<Devoir> _Devoirs;
        private List<Classe> _Classes;
        private List<Matiere> _Matieres;
        private DisplayMain _DisplayMain;
        private DisplayModal _DisplayModal;
        public List<Grid> GridElements
        {
            get { return Pnl_Devoirs.Children.Cast<UIElement>().Where(item => item is Border).Select(item => ((Border)item).Child as Grid).ToList(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            WindowState = Settings.Default.Maximized ? WindowState.Maximized : WindowState.Normal;
            Width = Settings.Default.Size.Width; Height = Settings.Default.Size.Height;
            _ShowOld = Settings.Default.ShowOld;
            _Repository = new AgendaRepository();
            _DisplayMain = new(this);
            _DisplayModal = new(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hauteur du StackPanel du menu
            _HeightPnlDevoirs = Pnl_MenuDevoirs.ActualHeight;
            ((TextBlock)Btn_AfficheOld.Child).Text = _ShowOld ? "Cacher les anciens" : "Montrer les anciens";
            Pnl_MenuDevoirs.Height = 0;

            Cursor = Cursors.Wait;
            LoadDataComponents(true);

            // Set timer
            _Timer = new Timer()
            {
                Interval = 5000,
                Enabled = true
            };
            _Timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Si page par defaut
            if (_ActualDevoirs.Equals(_Devoirs))
            {
                LoadDataComponents(false);
            }
            // Sinon juste recupérer les devoirs
            else
            {
                Task<List<Devoir>> devoirsAsync = _Repository.GetDevoirs();
                devoirsAsync.Wait();
                _Devoirs = devoirsAsync.Result;
                _Devoirs.ForEach(x => x.Date += new TimeSpan(23, 59, 59));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Timer.Dispose();
            Settings.Default.Maximized = WindowState == WindowState.Maximized ? true : false;
            Settings.Default.Size = new System.Drawing.Size((int)RenderSize.Width, (int)RenderSize.Height);
            Settings.Default.ShowOld = _ShowOld;
            Settings.Default.Save();
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_MouseDown(sender, null);
            }
        }

        private void Search_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Pnl_Devoirs.Focus();

            if (string.IsNullOrEmpty(Txb_Search.Text))
            {
                _ActualDevoirs = _Devoirs;
                _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
                return;
            }
            _ActualDevoirs = _Devoirs.Where(item => item.Matiere.Libelle.ToUpper().StartsWith(Txb_Search.Text.ToUpper())).ToList();
            if (_ActualDevoirs.Count != 0)
            {
                _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
                return;
            }
            
            _ActualDevoirs = _Devoirs.Where(item => item.Libelle.ToUpper().Contains(Txb_Search.Text.ToUpper())).ToList();
            _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
            return;
        }

        private void AddDevoir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Txb_Modal.Text = "Ajouter un devoir";
            Txb_Modal.Uid = ((int)Methodes.Ajouter).ToString();
            _DisplayModal.OpenModal(string.Empty, null, null, string.Empty, null, Methodes.Ajouter);
        }

        private void RestoreDevoir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Wait;
            var restoreAsync = Task.Run(() => _Repository.RestoreDevoir());
            restoreAsync.Wait();
            if (_ActualDevoirs.Equals(_Devoirs))
            {
                LoadDataComponents(false);
            }
            else
            {
                Task<List<Devoir>> devoirsAsync = _Repository.GetDevoirs();
                devoirsAsync.Wait();
                _Devoirs = devoirsAsync.Result;
                _Devoirs.ForEach(x => x.Date += new TimeSpan(23, 59, 59));
            }
        }

        private void AfficherOld_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ShowOld = !_ShowOld;
            ((TextBlock)Btn_AfficheOld.Child).Text = _ShowOld ? "Cacher les anciens" : "Montrer les anciens";
            _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
        }

        public void Classe_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Btn_Devoirs_MouseDown(sender, e);
            if (int.Parse(((Border)sender).Uid) == 0)
            {
                _ActualDevoirs = _Devoirs;
                _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
                return;
            }
            string libelle = _Classes.First(item => item.Id == int.Parse(((Border)sender).Uid)).Libelle;
            if (libelle.ToUpper() == "GROUPE A" || libelle.ToUpper() == "GROUPE B")
            {
                _ActualDevoirs = _Devoirs.Where(x => x.Classe.Id == int.Parse(((Border)sender).Uid)).Concat(_Devoirs.Where(item => item.Classe.Libelle.ToUpper() == "GROUPE A & B")).ToList();
                _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
                return;
            }
            _ActualDevoirs = _Devoirs.Where(x => x.Classe.Id == int.Parse(((Border)sender).Uid)).ToList();
            _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
        }

        private void Refresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Wait;
            LoadDataComponents(false);
        }

        public void Libelle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Devoir devSelected = _ActualDevoirs.First(x => x.Id == int.Parse(((TextBlock)sender).Uid));
            Txb_Modal.Text = "Affiche un devoir";
            Txb_Modal.Uid = ((int)Methodes.Selectionner).ToString();
            _DisplayModal.OpenModal(devSelected.Libelle, devSelected.Matiere, devSelected.Classe, devSelected.Description, devSelected.Date, Methodes.Selectionner);
        }

        public void Edit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Devoir devSelected = _ActualDevoirs.First(x => x.Id == int.Parse(((Border)sender).Uid));
            Txb_Modal.Text = "Modifier un devoir";
            Txb_Modal.Uid = ((int)Methodes.Modifier).ToString();
            Btn_SaveModal.Uid = devSelected.Id.ToString();
            _DisplayModal.OpenModal(devSelected.Libelle, devSelected.Matiere, devSelected.Classe, devSelected.Description, devSelected.Date, Methodes.Modifier);
        }

        public void Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Wait;
            var deletedDev = _ActualDevoirs.First(item => item.Id == int.Parse(((Border)sender).Uid));
            MessageBoxResult result = MessageBox.Show("Voulez supprimer ce devoir ?", "Supprimer ?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var deleteAsync = Task.Run(() => _Repository.DeleteDevoirs(deletedDev.Id));
                deleteAsync.Wait();
                LoadDataComponents(false);
            }
        }

        private void ModalSave_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ModalSave_MouseDown(sender, null);
            }
            if (e.Key == Key.Escape)
            {
                _DisplayModal.CloseModal();
            }
        }

        private void ModalSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(Txb_Libelle.Text))
            {
                Cvs_Libelle.Visibility = Visibility.Visible;
                return;
            }
            if (Dpc_Date.SelectedDate == null)
            {
                Cvs_Date.Visibility = Visibility.Visible;
                return;
            }
            if (Cmb_Classe.SelectedItem == null)
            {
                Cvs_Classe.Visibility = Visibility.Visible;
                return;
            }
            if (Cmb_Matiere.SelectedItem == null)
            {
                Cvs_Matiere.Visibility = Visibility.Visible;
                return;
            }

            var dev = new Devoir
            {
                Id = _Devoirs.Max(x => x.Id) + 1,
                Classe = (Classe)Cmb_Classe.SelectedItem,
                Matiere = (Matiere)Cmb_Matiere.SelectedItem,
                Libelle = Txb_Libelle.Text,
                Description = Txb_Description.Text,
                Date = Dpc_Date.SelectedDate.Value + new TimeSpan(23, 59, 59)
            };

            if (int.Parse(Txb_Modal.Uid) == (int)Methodes.Ajouter)
            {
                var postAsync = Task.Run(() => _Repository.PostDevoirs(dev)) ;
                postAsync.Wait();
                dev.Id = postAsync.Result;
                _Devoirs.Add(dev);
            }

            if (int.Parse(Txb_Modal.Uid) == (int)Methodes.Modifier)
            {
                _Devoirs.Remove(_ActualDevoirs.First(x => x.Id == int.Parse(Btn_SaveModal.Uid)));
                dev.Id = int.Parse(Btn_SaveModal.Uid);
                var updateAsync = Task.Run(() => _Repository.UpdateDevoirs(dev.Id, dev));
                updateAsync.Wait();
                _Devoirs.Add(dev);
            }

            _DisplayModal.CloseModal();
            _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);
        }

        private void Btn_CloseModal(object sender, MouseButtonEventArgs e) => _DisplayModal.CloseModal();

        private void LoadDataComponents(bool chargement)
        {
            Application.Current.ExecOnUiThread(() =>
            {
                if (chargement)
                {
                    Pnl_Devoirs.Children.RemoveRange(0, Pnl_Devoirs.Children.Count);
                }
                Txb_Chargement.Visibility = chargement ? Visibility.Visible : Visibility.Collapsed;
            });

            var taskData = Task.Run(() => UpdateData());
                
            taskData.ContinueWith(t =>
            {
                Application.Current.ExecOnUiThread(() =>
                {
                    _DisplayMain.Devoirs(_ActualDevoirs, _ShowOld);

                    _DisplayMain.Classes(_Classes.Where(x => x.Libelle.ToUpper() != "GROUPE A & B").OrderBy(x => x.Libelle).Append(new Classe { Id = 0, Libelle = "Tous" }).ToArray());

                    int indexClasse = Cmb_Classe.SelectedIndex;
                    Cmb_Classe.ItemsSource = _Classes.OrderBy(x => x.Libelle).ToArray();
                    Cmb_Classe.SelectedIndex = indexClasse;

                    int indexMatiere = Cmb_Matiere.SelectedIndex;
                    Cmb_Matiere.ItemsSource = _Matieres.OrderBy(x => x.Libelle).ToArray();
                    Cmb_Matiere.SelectedIndex = indexMatiere;

                    Txb_Chargement.Visibility = Visibility.Collapsed;
                    Cursor = Cursors.Arrow;
                });
            });
        }

        private async Task UpdateData()
        {
            var devoirsAsync = _Repository.GetDevoirs();
            var classesAsync = _Repository.GetClasses();
            var matieresAsync = _Repository.GetMatieres();

            List<Task> tasks = new List<Task>() { devoirsAsync, classesAsync, matieresAsync };

            while (tasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny(tasks);
                if (finishedTask == devoirsAsync)
                {
                    _Devoirs = await devoirsAsync;
                    _ActualDevoirs = _Devoirs;
                }
                if (finishedTask == classesAsync)
                {
                    _Classes = classesAsync.Result;
                }
                if (finishedTask == matieresAsync)
                {
                    _Matieres = matieresAsync.Result;
                }
                tasks.Remove(finishedTask);
            }
        }

        #region Style
        private void Txb_Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "Recherche...")
            {
                ((TextBox)sender).Text = "";
            }
            ((TextBox)sender).Foreground = (SolidColorBrush)Resources["TextColor"];
        }

        private void Txb_Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
            {
                ((TextBox)sender).Text = "Recherche...";
            }
            ((TextBox)sender).Foreground = (SolidColorBrush)Resources["TextLightColor"];
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e) => Txb_Search.Focus();

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Bdr_FenModal.Width = 580;
            Txb_Search.Width = 260;
            Col_Menu.Width = new GridLength(250);
            foreach (Grid item in GridElements)
            {
                ((TextBlock)((StackPanel)item.Children[0]).Children[0]).IsEnabled = false;
                ((ScrollViewer)item.Children[2]).Visibility = Visibility.Visible;
            }

            if (ActualWidth <= 1000)
            {
                Col_Menu.Width = new GridLength(250);
                Bdr_FenModal.Width = 540;
                Txb_Search.Width = 260;
                foreach (Grid item in GridElements)
                {
                    ((TextBlock)((StackPanel)item.Children[0]).Children[0]).IsEnabled = true;
                    ((ScrollViewer)item.Children[2]).Visibility = Visibility.Hidden;
                }
            }

            if (ActualWidth < 650)
            {
                Col_Menu.Width = new GridLength(Col_Menu.MinWidth);
                Bdr_FenModal.Width = 450;
                Txb_Search.Width = 130;
            }
        }

        private void Btn_Devoirs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation
            {
                From = Pnl_MenuDevoirs.Height,
                To = Pnl_MenuDevoirs.Height == 0 ? _HeightPnlDevoirs : 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(doubleAnim);
            Storyboard.SetTarget(storyboard, Pnl_MenuDevoirs);
            Storyboard.SetTargetName(doubleAnim, Pnl_MenuDevoirs.Name);
            Storyboard.SetTargetProperty(doubleAnim, new PropertyPath(HeightProperty));
            storyboard.Begin();
        }
        #endregion
    }
}
