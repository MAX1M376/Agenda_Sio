using SIO_AgendaWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SIO_AgendaWPF.MainWindow;

namespace SIO_AgendaWPF.Display
{
    public static class DisplayMain
    {
        private static readonly MainWindow window = new();

        public static void Classes(Classe[] classes)
        {
            window.Pnl_MenuDevoirs.Children.RemoveRange(0, window.Pnl_MenuDevoirs.Children.Count);

            Border btn;
            foreach (var item in classes)
            {
                // Ajout des elements a Pnl_Devoirs
                btn = new Border
                {
                    Uid = item.Id.ToString(),
                    Style = (Style)window.Resources["ButtonMenuStyle"],
                    Child = new TextBlock() { Style = (Style)window.Resources["TextBlockMenuStyle"], Text = item.Libelle }
                };
                btn.MouseDown += window.Classe_MouseDown;
                window.Pnl_MenuDevoirs.Children.Add(btn);
            }

            // Resize de Pnl_Devoirs
            window.Pnl_MenuDevoirs.UpdateLayout();
            window._HeightPnlDevoirs = window.Pnl_MenuDevoirs.ActualHeight;
        }

        public static void Devoirs(List<Devoir> devoirs, bool showOld)
        {
            window.Pnl_Devoirs.Children.RemoveRange(0, window.Pnl_Devoirs.Children.Count);

            var finSemaine = DateTime.Now.AddDays(6 - Mod((int)DateTime.Now.DayOfWeek + 1, 7));
            DateTime dateFinSem = new DateTime(finSemaine.Year, finSemaine.Month, finSemaine.Day) + new TimeSpan(23, 59, 59);
            devoirs = devoirs.OrderBy(x => x, new Devoir()).ToList();

            var NewDevoirs = devoirs.Where(x => x.Date >= DateTime.Now).ToList();
            if (!showOld && !NewDevoirs.Any())
            {
                showOld = true;
                ((TextBlock)window.Btn_AfficheOld.Child).Text = showOld ? "Cacher les anciens" : "Montrer les anciens";
                CategorieDevoirs("Déjà fais", devoirs.Where(x => x.Date < DateTime.Now).ToList(), "d MMM yy");
            }

            if (showOld) { CategorieDevoirs("Déjà fais", devoirs.Where(x => x.Date < DateTime.Now).ToList(), "d MMM yy"); }
            CategorieDevoirs("Cette semaine", devoirs.Where(x => x.Date >= DateTime.Now && x.Date <= dateFinSem).ToList(), "dddd");
            CategorieDevoirs("Plus tard", devoirs.Where(x => x.Date > dateFinSem).ToList(), "d MMM yy");
        }

        private static void CategorieDevoirs(string titre, List<Devoir> devoirs, string format)
        {
            if (devoirs.Count() != 0)
            {
                window.Pnl_Devoirs.Children.Add(new TextBlock()
                {
                    Text = titre,
                    Foreground = (SolidColorBrush)window.Resources["TextColor"],
                    FontSize = 18
                });
            }
            for (int i = 0; i < devoirs.Count(); i++)
            {
                AddDevoir(devoirs[i].Id, devoirs[i].Libelle, devoirs[i].Classe.Libelle, devoirs[i].Matiere.Libelle, devoirs[i].Description, char.ToUpper(devoirs[i].Date.ToString(format)[0]) + devoirs[i].Date.ToString(format).Substring(1), i == devoirs.Count() - 1);
            }
        }

        private static void AddDevoir(int id, string libelle, string classe, string matiere, string description, string date, bool isLast)
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
                Foreground = (SolidColorBrush)window.Resources["TextColor"],
                Style = (Style)window.Resources["StyleTextblockLibelle"]
            };
            gridElement.MouseDown += window.Libelle_MouseDown;
            stackPanelLibelle.Children.Add(gridElement);

            gridElement = new TextBlock()
            {
                Uid = id.ToString(),
                Text = $" - {classe}",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14,
                Foreground = (SolidColorBrush)window.Resources["TextLightColor"]
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
                Foreground = (SolidColorBrush)window.Resources["TextLightColor"]
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
                Foreground = (SolidColorBrush)window.Resources["TextColor"]
            };
            gridElement = new ScrollViewer()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Visibility = Visibility.Hidden,
                Margin = new Thickness(5),
                Style = (Style)window.Resources["ScrollBarStyleLightBis"],
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
                Foreground = (SolidColorBrush)window.Resources["TextLightColor"],
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(gridElement, 2);
            grid.Children.Add(gridElement);

            // Creation du bouton Edition
            gridElement = new Border()
            {
                Uid = id.ToString(),
                Height = 35,
                Width = 35,
                Margin = new Thickness(10, 0, 10, 0),
                Style = (Style)window.Resources["ButtonStyleInvert"],
                Child = new TextBlock() { Text = "\xE70F", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF165ACC")), Style = (Style)window.Resources["StyleIconContent"] }
            };
            gridElement.MouseDown += window.Edit_MouseDown;
            Grid.SetColumn(gridElement, 3);
            grid.Children.Add(gridElement);

            // Creation du bouton Delete
            gridElement = new Border()
            {
                Uid = id.ToString(),
                Height = 35,
                Width = 35,
                Style = (Style)window.Resources["ButtonStyleInvert"],
                Child = new TextBlock() { Text = "\xE74D", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE0281D")), Style = (Style)window.Resources["StyleIconContent"] }
            };
            gridElement.MouseDown += window.Delete_MouseDown;
            Grid.SetColumn(gridElement, 4);
            grid.Children.Add(gridElement);

            window.Pnl_Devoirs.Children.Add(new Border()
            {
                BorderBrush = (SolidColorBrush)window.Resources["BorderColor"],
                BorderThickness = new Thickness(0, 0, 0, isLast ? 0 : 1),
                Margin = new Thickness(30, 0, 0, isLast ? 30 : 0),
                Child = grid
            });
        }

        private static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
