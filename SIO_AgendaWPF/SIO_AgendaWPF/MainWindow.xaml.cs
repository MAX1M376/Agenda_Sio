using SIO_AgendaWPF.Models;
using SIO_AgendaWPF.Properties;
using SIO_AgendaWPF.Repositories;
using SIO_AgendaWPF.Extensions;
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
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace SIO_AgendaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Methodes { Selectionner = 0, Ajouter = 1, Modifier = 2 }
        private IAgendaRepository _Repository;
        private double _HeightPnlDevoirs;
        private bool _ShowOld;
        private Timer _Timer;
        private List<Devoir> _ActualDevoirs;
        private List<Devoir> _Devoirs;
        private List<Classe> _Classes;
        private List<Matiere> _Matieres;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hauteur du StackPanel du menu
            _HeightPnlDevoirs = Pnl_MenuDevoirs.ActualHeight;
            ((TextBlock)Btn_AfficheOld.Child).Text = _ShowOld ? "Cacher les anciens" : "Montrer les anciens";
            Pnl_MenuDevoirs.Height = 0;

            LoadDataComponents(true);

            // Set timer
            _Timer = new Timer()
            {
                Interval = 10000,
                Enabled = true
            };
            _Timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_ActualDevoirs.Equals(_Devoirs))
            {
                Application.Current.ExecOnUiThread(() => 
                {
                    LoadDataComponents(false);
                });
                
            }
            else
            {
                var taskDevoirs = Task.Run(() => _Repository.GetDevoirs());
                while (taskDevoirs.Status != TaskStatus.RanToCompletion) { }
                _Devoirs = taskDevoirs.Result;
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

        #region Search
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
                AfficherDevoirs(_ActualDevoirs);
                return;
            }
            _ActualDevoirs = _Devoirs.Where(item => item.Matiere.Libelle.ToUpper().StartsWith(Txb_Search.Text.ToUpper())).ToList();
            if (_ActualDevoirs.Count != 0)
            {
                AfficherDevoirs(_ActualDevoirs);
                return;
            }
            
            _ActualDevoirs = _Devoirs.Where(item => item.Libelle.ToUpper().Contains(Txb_Search.Text.ToUpper())).ToList();
            AfficherDevoirs(_ActualDevoirs);
            return;
        }
        #endregion

        #region Header
        private void AddDevoir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Txb_Modal.Text = "Ajouter un devoir";
            Txb_Modal.Uid = ((int)Methodes.Ajouter).ToString();
            OpenModal(string.Empty, null, null, string.Empty, null, Methodes.Ajouter);
        }

        private void RestoreDevoir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var taskDevoir = Task.Run(() => _Repository.RestoreDevoir());
            while (taskDevoir.Status != TaskStatus.RanToCompletion) { }
            if (_ActualDevoirs.Equals(_Devoirs))
            {
                Application.Current.ExecOnUiThread(() =>
                {
                    MainWindow wind = Application.Current.MainWindow as MainWindow;
                    wind.Refresh_MouseDown(sender, null);
                });
            }
            else
            {
                var taskDevoirs = Task.Run(() => _Repository.GetDevoirs());
                while (taskDevoirs.Status != TaskStatus.RanToCompletion) { }
                _Devoirs = taskDevoirs.Result;
                _Devoirs.ForEach(x => x.Date += new TimeSpan(23, 59, 59));
            }
        }

        private void AfficherOld_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ShowOld = !_ShowOld;
            ((TextBlock)Btn_AfficheOld.Child).Text = _ShowOld ? "Cacher les anciens" : "Montrer les anciens";
            AfficherDevoirs(_ActualDevoirs);
        }
        #endregion

        #region Menu
        private void Classe_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Btn_Devoirs_MouseDown(sender, e);
            if (int.Parse(((Border)sender).Uid) == 0)
            {
                _ActualDevoirs = _Devoirs;
                AfficherDevoirs(_ActualDevoirs);
                return;
            }
            string libelle = _Classes.First(item => item.Id == int.Parse(((Border)sender).Uid)).Libelle;
            if (libelle.ToUpper() == "GROUPE A" || libelle.ToUpper() == "GROUPE B")
            {
                _ActualDevoirs = _Devoirs.Where(x => x.Classe.Id == int.Parse(((Border)sender).Uid)).Concat(_Devoirs.Where(item => item.Classe.Libelle.ToUpper() == "GROUPE A & B")).ToList();
                AfficherDevoirs(_ActualDevoirs);
                return;
            }
            _ActualDevoirs = _Devoirs.Where(x => x.Classe.Id == int.Parse(((Border)sender).Uid)).ToList();
            AfficherDevoirs(_ActualDevoirs);
        }

        private void Refresh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadDataComponents(false);
        }

        #endregion

        #region Content
        private void Libelle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Devoir devSelected = _ActualDevoirs.First(x => x.Id == int.Parse(((TextBlock)sender).Uid));
            Txb_Modal.Text = "Affiche un devoir";
            Txb_Modal.Uid = ((int)Methodes.Selectionner).ToString();
            OpenModal(devSelected.Libelle, devSelected.Matiere, devSelected.Classe, devSelected.Description, devSelected.Date, Methodes.Selectionner);
        }

        private void Edit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Devoir devSelected = _ActualDevoirs.First(x => x.Id == int.Parse(((Border)sender).Uid));
            Txb_Modal.Text = "Modifier un devoir";
            Txb_Modal.Uid = ((int)Methodes.Modifier).ToString();
            Btn_SaveModal.Uid = devSelected.Id.ToString();
            OpenModal(devSelected.Libelle, devSelected.Matiere, devSelected.Classe, devSelected.Description, devSelected.Date, Methodes.Modifier);
        }

        private void Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var deletedDev = _ActualDevoirs.First(item => item.Id == int.Parse(((Border)sender).Uid));
            MessageBoxResult result = MessageBox.Show("Voulez supprimer ce devoir ?", "Supprimer ?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var taskDel = Task.Run(() => _Repository.DeleteDevoirs(deletedDev.Id));
                while (taskDel.Status != TaskStatus.RanToCompletion) { }
                _Devoirs.Remove(deletedDev);
                AfficherDevoirs(_ActualDevoirs);
            }
        }
        #endregion

        #region Modal
        private void ModalSave_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ModalSave_MouseDown(sender, null);
            }
            if (e.Key == Key.Escape)
            {
                Btn_CloseModal(sender, null);
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
                var taskDevoirs = Task.Run(() => _Repository.PostDevoirs(dev));
                while (taskDevoirs.Status != TaskStatus.RanToCompletion) { }
                dev.Id = taskDevoirs.Result;
                _Devoirs.Add(dev);
            }

            if (int.Parse(Txb_Modal.Uid) == (int)Methodes.Modifier)
            {
                _Devoirs.Remove(_ActualDevoirs.First(x => x.Id == int.Parse(Btn_SaveModal.Uid)));
                dev.Id = int.Parse(Btn_SaveModal.Uid);
                var taskDevoirs = Task.Run(() => _Repository.UpdateDevoirs(dev.Id, dev));
                while (taskDevoirs.Status != TaskStatus.RanToCompletion) { }
                _Devoirs.Add(dev);
            }

            Cvs_Libelle.Visibility = Visibility.Hidden;
            Cvs_Description.Visibility = Visibility.Hidden;
            Cvs_Date.Visibility = Visibility.Hidden;
            Cvs_Classe.Visibility = Visibility.Hidden;
            Cvs_Matiere.Visibility = Visibility.Hidden;
            Bdr_Modal.Visibility = Visibility.Hidden;
            AfficherDevoirs(_ActualDevoirs);
        }

        private void Btn_CloseModal(object sender, MouseButtonEventArgs e)
        {
            Cvs_Libelle.Visibility = Visibility.Hidden;
            Cvs_Description.Visibility = Visibility.Hidden;
            Cvs_Date.Visibility = Visibility.Hidden;
            Cvs_Classe.Visibility = Visibility.Hidden;
            Cvs_Matiere.Visibility = Visibility.Hidden;
            Bdr_Modal.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Privee
        private void LoadDataComponents(bool chargement)
        {
            Txb_Chargement.Visibility = chargement ? Visibility.Visible : Visibility.Collapsed;

            var taskOnLoad = Task.Factory.StartNew(() =>
            {
                var taskDevoirs = Task.Run(() => _Repository.GetDevoirs());
                while (taskDevoirs.Status != TaskStatus.RanToCompletion) { }
                _Devoirs = taskDevoirs.Result;
                _Devoirs.ForEach(x => x.Date += new TimeSpan(23, 59, 59));
                _ActualDevoirs = _Devoirs;

                var taskClasses = Task.Run(() => _Repository.GetClasses());
                while (taskClasses.Status != TaskStatus.RanToCompletion) { }
                _Classes = taskClasses.Result;

                var taskMatieres = Task.Run(() => _Repository.GetMatieres());
                while (taskMatieres.Status != TaskStatus.RanToCompletion) { }
                _Matieres = taskMatieres.Result;
            });

            Task.Factory.ContinueWhenAll(new[] { taskOnLoad }, x =>
            {
                Application.Current.ExecOnUiThread(() =>
                {
                    // Ajouter une classe dans le menu
                    AddClasse(_Classes.Where(x => x.Libelle.ToUpper() != "GROUPE A & B").OrderBy(x => x.Libelle).Append(new Classe { Id = 0, Libelle = "Tous" }).ToArray());

                    // Button d'afficher les afficher ou pas
                    AfficherDevoirs(_Devoirs);
                    _ActualDevoirs = _Devoirs;

                    // Ajouter itmes aux combobox
                    int indexClasse = Cmb_Classe.SelectedIndex;
                    Cmb_Classe.ItemsSource = _Classes.OrderBy(x => x.Libelle).ToArray();
                    Cmb_Classe.SelectedIndex = indexClasse;
                    int indexMatiere = Cmb_Matiere.SelectedIndex;
                    Cmb_Matiere.ItemsSource = _Matieres.OrderBy(x => x.Libelle).ToArray();
                    Cmb_Matiere.SelectedIndex = indexMatiere;

                    Txb_Chargement.Visibility = Visibility.Collapsed;
                });
            });
        }
        private void OpenModal(string libelle, Matiere matiere, Classe classe, string description, DateTime? date, Methodes editable)
        {
            Bdr_Modal.Visibility = Visibility.Visible;
            ContentModal.Focus();
            Bdr_FenModal.Uid = ((int)editable).ToString();
            Btn_SaveModal.Visibility = editable != Methodes.Selectionner ? Visibility.Visible : Visibility.Collapsed;
            Txb_Libelle.Text = libelle;
            Dpc_Date.SelectedDate = matiere == null ? null : new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
            Cmb_Matiere.SelectedItem = matiere == null ? null : ((Matiere[])Cmb_Matiere.ItemsSource).First(x => x.Id == matiere.Id);
            Cmb_Classe.SelectedItem = classe == null ? null : ((Classe[])Cmb_Classe.ItemsSource).First(x => x.Id == classe.Id);
            Txb_Description.Text = description;
            Txb_Libelle.IsReadOnly = editable == Methodes.Selectionner;
            Dpc_Date.IsEnabled = editable != Methodes.Selectionner;
            Cmb_Classe.IsEnabled = editable != Methodes.Selectionner;
            Cmb_Matiere.IsEnabled = editable != Methodes.Selectionner;
            Txb_Description.IsReadOnly = editable == Methodes.Selectionner;
        }
        private void AddClasse(Classe[] classes)
        {
            Pnl_MenuDevoirs.Children.RemoveRange(0, Pnl_MenuDevoirs.Children.Count);

            Border btn;
            foreach (var item in classes)
            {
                // Ajout des elements a Pnl_Devoirs
                btn = new Border
                {
                    Uid = item.Id.ToString(),
                    Style = (Style)Resources["ButtonMenuStyle"],
                    Child = new TextBlock() { Style = (Style)Resources["TextBlockMenuStyle"], Text = item.Libelle }
                };
                btn.MouseDown += Classe_MouseDown;
                Pnl_MenuDevoirs.Children.Add(btn);
            }

            // Resize de Pnl_Devoirs
            Pnl_MenuDevoirs.UpdateLayout();
            _HeightPnlDevoirs = Pnl_MenuDevoirs.ActualHeight;
        }
        private Grid AddDevoir(int id, string libelle, string classe, string matiere, string description, string date, bool isLast)
        {
            // Creation de la Grille
            Grid grid;
            UIElement gridElement;
            grid = new Grid() { Height = 60 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            var stackPanelLibelle = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            // Creation du TextBlock du Libelle
            gridElement = new TextBlock()
            {
                Uid = id.ToString(),
                Text = libelle,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Foreground = (SolidColorBrush)Resources["TextColor"],
                Style = (Style)Resources["StyleTextblockLibelle"]
            };
            gridElement.MouseDown += Libelle_MouseDown;
            stackPanelLibelle.Children.Add(gridElement);

            gridElement = new TextBlock()
            {
                Uid = id.ToString(),
                Text = $" - {classe}",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14,
                Foreground = (SolidColorBrush)Resources["TextLightColor"]
            };
            stackPanelLibelle.Children.Add(gridElement);

            Grid.SetColumn(stackPanelLibelle, 0);
            grid.Children.Add(stackPanelLibelle);

            // Creation du TextBlock de la Matiere
            gridElement = new TextBlock()
            {
                Uid = id.ToString(),
                Text = matiere,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = 12,
                Foreground = (SolidColorBrush)Resources["TextLightColor"]
            };
            Grid.SetColumn(gridElement, 0);
            grid.Children.Add(gridElement);

            // Creation du TextBlock de la desciption
            gridElement = new TextBlock()
            {
                Uid = id.ToString(),
                Text = description,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextAlignment = TextAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = (SolidColorBrush)Resources["TextColor"]
            };
            gridElement = new ScrollViewer()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Visibility = Visibility.Hidden,
                Margin = new Thickness(5),
                Style = (Style)Resources["ScrollBarStyleLightBis"],
                Content = gridElement
            };
            Grid.SetColumn(gridElement, 1);
            grid.Children.Add(gridElement);

            // Creation du TextBlock de la Date
            gridElement = new TextBlock()
            {
                Uid = id.ToString(),
                Text = date,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Foreground = (SolidColorBrush)Resources["TextLightColor"],
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(gridElement, 2);
            grid.Children.Add(gridElement);

            // Creation du bouton Edition
            gridElement = new Border()
            {
                Uid = id.ToString(),
                Height = 35, Width = 35,
                Margin = new Thickness(10, 0, 10, 0),
                Style = (Style)Resources["ButtonStyleInvert"],
                Child = new TextBlock() { Text = "\xE70F", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF165ACC")), Style = (Style)Resources["StyleIconContent"] }
            };
            gridElement.MouseDown += Edit_MouseDown;
            Grid.SetColumn(gridElement, 3);
            grid.Children.Add(gridElement);

            // Creation du bouton Delete
            gridElement = new Border()
            {
                Uid = id.ToString(),
                Height = 35,
                Width = 35,
                Style = (Style)Resources["ButtonStyleInvert"],
                Child = new TextBlock() { Text = "\xE74D", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE0281D")), Style = (Style)Resources["StyleIconContent"] }
            };
            gridElement.MouseDown += Delete_MouseDown;
            Grid.SetColumn(gridElement, 4);
            grid.Children.Add(gridElement);

            Pnl_Devoirs.Children.Add(new Border() 
            {
                BorderBrush = (SolidColorBrush)Resources["BorderColor"], 
                BorderThickness = new Thickness(0, 0, 0, isLast ? 0 : 1),
                Margin = new Thickness(30, 0, 0, isLast ? 30 : 0),
                Child = grid 
            });
            return grid;
        }
        private void RefreshDevoir(List<Devoir> devoirs)
        {
            int test = Mod((int)DateTime.Now.DayOfWeek + 1, 7);
            var finSemaine = DateTime.Now.AddDays(6 - test);
            DateTime dateFinSem = new DateTime(finSemaine.Year, finSemaine.Month, finSemaine.Day) + new TimeSpan(23, 59, 59);
            devoirs = devoirs.OrderBy(x => x, new Devoir()).ToList();

            AddListDevoirs("Déjà fais", devoirs.Where(x => x.Date < DateTime.Now).ToList(), "d MMM yy");
            AddListDevoirs("Cette semaine", devoirs.Where(x => x.Date >= DateTime.Now && x.Date <= dateFinSem).ToList(), "dddd");
            AddListDevoirs("Plus tard", devoirs.Where(x => x.Date > dateFinSem).ToList(), "d MMM yy");
        }
        private void AddListDevoirs(string titre, List<Devoir> devoirs, string format)
        {
            if (devoirs.Count() != 0)
            {
                Pnl_Devoirs.Children.Add(new TextBlock()
                {
                    Text = titre,
                    Foreground = (SolidColorBrush)Resources["TextColor"],
                    FontSize = 18
                });
            }
            for (int i = 0; i < devoirs.Count(); i++)
            {
                AddDevoir(devoirs[i].Id, devoirs[i].Libelle, devoirs[i].Classe.Libelle, devoirs[i].Matiere.Libelle, devoirs[i].Description, char.ToUpper(devoirs[i].Date.ToString(format)[0]) + devoirs[i].Date.ToString(format).Substring(1), i == devoirs.Count() - 1);
            }
        }
        private void AfficherDevoirs(List<Devoir> devoirs)
        {
            Pnl_Devoirs.Children.RemoveRange(0, Pnl_Devoirs.Children.Count);

            if (!_ShowOld)
            {
                var dev_temp = devoirs.Where(x => x.Date >= DateTime.Now).ToList();
                if (dev_temp.Count == 0)
                {
                    _ShowOld = true;
                    ((TextBlock)Btn_AfficheOld.Child).Text = _ShowOld ? "Cacher les anciens" : "Montrer les anciens";
                    RefreshDevoir(devoirs);
                }
                else
                {
                    RefreshDevoir(dev_temp);
                }
            }
            else
            {
                RefreshDevoir(devoirs);
            }
            Window_SizeChanged(null, null);
        }
        private int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
        #endregion

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
            #region Style
            if (((TextBox)sender).Text == "")
            {
                ((TextBox)sender).Text = "Recherche...";
            }
            ((TextBox)sender).Foreground = (SolidColorBrush)Resources["TextLightColor"];
            #endregion
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
