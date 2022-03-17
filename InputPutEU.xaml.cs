﻿using Dll_Calcul;
using System;
using System.Globalization;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace PriceurXAML
{
    /// <summary>
    /// Logique d'interaction pour Input.xaml
    /// </summary>
    public partial class InputPutEU : Window
    {
        public InputPutEU()
        {
            InitializeComponent();
        }

        string Spot = "";
        string Strike = "";
        string Taux = "";
        string Vol = "";
        string Mat = "";
        Utilitaire cal = new Utilitaire();
        private ModelView.ModelView viewModel;
        private ModelView.PayoffView payoffview;
        private void Price_Click(object sender, RoutedEventArgs e)
        {
            string err = "";
            Spot = SpotInp.Text;
            Strike = StrikeInp.Text;
            Taux = TauxInp.Text;
            Vol = VolInp.Text;
            Mat = MatInp.Text;
            if (cal.CheckInput("Spot", Spot) != "")
            {
                string caption = "Erreur détectée";
                MessageBoxButtons Ok = MessageBoxButtons.OK;
                DialogResult result;
                err = "La valeur pour 'Spot' n'est pas correct. \n";
                result = System.Windows.Forms.MessageBox.Show(err + cal.CheckInput("Spot", Spot), caption, Ok);
            }
            else if (cal.CheckInput("Strike", Strike) != "")
            {
                string caption = "Erreur détectée";
                MessageBoxButtons Ok = MessageBoxButtons.OK;
                DialogResult result;
                err = "La valeur pour 'Strike' n'est pas correct. \n";
                result = System.Windows.Forms.MessageBox.Show(err + cal.CheckInput("Strike", Strike), caption, Ok);
            }
            else if (cal.CheckInput("Taux", Taux) != "")
            {
                string caption = "Erreur détectée";
                MessageBoxButtons Ok = MessageBoxButtons.OK;
                DialogResult result;
                err = "La valeur pour 'taux' n'est pas correct. \n";
                result = System.Windows.Forms.MessageBox.Show(err + cal.CheckInput("Taux", Taux), caption, Ok);
            }
            else if (cal.CheckInput("Vol", Vol) != "")
            {
                string caption = "Erreur détectée";
                MessageBoxButtons Ok = MessageBoxButtons.OK;
                DialogResult result;
                err = "La valeur pour 'Volatility' n'est pas correct. \n";
                result = System.Windows.Forms.MessageBox.Show(err + cal.CheckInput("Vol", Vol), caption, Ok);
            }
            else if (cal.CheckInput("Mat", Mat) != "")
            {
                string caption = "Erreur détectée";
                MessageBoxButtons Ok = MessageBoxButtons.OK;
                DialogResult result;
                err = "La valeur pour 'Maturité' n'est pas correct. \n";
                result = System.Windows.Forms.MessageBox.Show(err + cal.CheckInput("Mat", Mat), caption, Ok);
            }
            else
            {
                if (Euler.IsChecked == false && MBG.IsChecked == false && Milstein.IsChecked == false)
                {
                    string caption = "Erreur détectée";
                    MessageBoxButtons Ok = MessageBoxButtons.OK;
                    DialogResult result;
                    err = "Aucune méthode de simulation selectionée \n";
                    result = System.Windows.Forms.MessageBox.Show(err, caption, Ok);
                }
                else
                {
                    DisplayDataCallEU();
                }

            }

        }

        private async void DisplayDataCallEU()
        {
            PriceInp.Visibility = Visibility.Hidden;
            Spot = SpotInp.Text;
            Strike = StrikeInp.Text;
            Taux = TauxInp.Text;
            Vol = VolInp.Text;
            Mat = MatInp.Text;

            double S = double.Parse(Spot, CultureInfo.InvariantCulture);
            double K = double.Parse(Strike, CultureInfo.InvariantCulture);
            double r = double.Parse(Taux, CultureInfo.InvariantCulture);
            double v = double.Parse(Vol, CultureInfo.InvariantCulture);
            double T = double.Parse(Mat, CultureInfo.InvariantCulture);

            double Prix = cal.put_price(S, K, r, v, T);
            Prix = Math.Round(Prix, 6);

            decimal Prime = 0.0m;

            PbStatus.Visibility = Visibility.Visible;
            PbCal.Visibility = Visibility.Visible;
            PbStatus.Value = 0;
            var points = new List<Simu>();
            if (Euler.IsChecked == true)
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                PbStatus.Value = (int)i / 1000;
                            });
                            decimal[] traji = cal.Euler(S, r, v, T);
                            for (int s = 0; s < traji.Length - 1; s++)
                            {
                                points.Add(new Simu() { id = i, pas = s, Value = traji[s] });
                            }
                            Prime = Prime + (decimal)Math.Max((decimal)K - traji[traji.Length - 1], 0.0m);
                        }
                        else
                        {
                            decimal[] traj = cal.Euler(S, r, v, T);

                            Prime = Prime + (decimal)Math.Max((decimal)K - traj[traj.Length - 1], 0.0m);
                        }

                    }
                    var AllData = points.GroupBy(m => m.id).ToList();
                    viewModel = new ModelView.ModelView();
                    viewModel.SetBackground(190, 190, 190);
                    foreach (var data in AllData)
                    {
                        var lines = new LineSeries
                        {
                            StrokeThickness = 2,
                            MarkerSize = 3,
                            CanTrackerInterpolatePoints = false,
                        };
                        data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, Convert.ToDouble(d.Value))));
                        viewModel.AddLines(lines);
                    }
                });
            }
            else if (MBG.IsChecked == true)
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                PbStatus.Value = (int)i / 1000;
                            });
                            decimal[] traji = cal.MBG(S, r, v, T);
                            for (int s = 0; s < traji.Length - 1; s++)
                            {
                                points.Add(new Simu() { id = i, pas = s, Value = traji[s] });
                            }
                            Prime = Prime + (decimal)Math.Max((decimal)K - traji[traji.Length - 1], 0.0m);
                        }
                        else
                        {
                            decimal[] traj = cal.MBG(S, r, v, T);

                            Prime = Prime + (decimal)Math.Max((decimal)K - traj[traj.Length - 1], 0.0m);
                        }

                    }
                    var AllData = points.GroupBy(m => m.id).ToList();
                    viewModel = new ModelView.ModelView();
                    viewModel.SetBackground(190, 190, 190);
                    foreach (var data in AllData)
                    {
                        var lines = new LineSeries
                        {
                            StrokeThickness = 2,
                            MarkerSize = 3,
                            CanTrackerInterpolatePoints = false,
                        };
                        data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, Convert.ToDouble(d.Value))));
                        viewModel.AddLines(lines);
                    }
                });
            }
            else
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                PbStatus.Value = (int)i / 1000;
                            });
                            decimal[] traji = cal.Milstein2(S, r, v, T);
                            for (int s = 0; s < traji.Length - 1; s++)
                            {
                                points.Add(new Simu() { id = i, pas = s, Value = traji[s] });
                            }
                            Prime = Prime + (decimal)Math.Max((decimal)K - traji[traji.Length - 1], 0.0m);
                        }
                        else
                        {
                            decimal[] traj = cal.Milstein2(S, r, v, T);

                            Prime = Prime + (decimal)Math.Max((decimal)K - traj[traj.Length - 1], 0.0m);
                        }

                    }
                    var AllData = points.GroupBy(m => m.id).ToList();
                    viewModel = new ModelView.ModelView();
                    viewModel.SetBackground(190, 190, 190);
                    foreach (var data in AllData)
                    {
                        var lines = new LineSeries
                        {
                            StrokeThickness = 2,
                            MarkerSize = 3,
                            CanTrackerInterpolatePoints = false,
                        };
                        data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, Convert.ToDouble(d.Value))));
                        viewModel.AddLines(lines);
                    }
                });
            }
            PlotSimu.DataContext = viewModel;

            // PLOT PAYOFF
            var point2 = cal.PlotPayoff(S, K, Prime / 100000m, "PutEu");
            var AllData = point2.GroupBy(m => m.id).ToList();
            payoffview = new ModelView.PayoffView();
            payoffview.SetBackground(190, 190, 190);
            payoffview.SetUpAxis(Prime / 100000m, K);
            foreach (var data in AllData)
            {
                if (data.Key == 0)
                {
                    var lines = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 3,
                        CanTrackerInterpolatePoints = false,
                        Title = "Long Position"
                    };
                    data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, d.Value)));
                    payoffview.AddLines(lines);
                }
                else if (data.Key == 1)
                {
                    var lines = new LineSeries
                    {
                        StrokeThickness = 2,
                        MarkerSize = 3,
                        CanTrackerInterpolatePoints = false,
                        Title = "Short Position"
                    };
                    data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, d.Value)));
                    payoffview.AddLines(lines);
                }
                else if (data.Key == 2)
                {
                    var lines = new LineSeries
                    {
                        StrokeThickness = 0.5,
                        MarkerSize = 0.5,
                        CanTrackerInterpolatePoints = false,
                        Color = OxyColor.FromRgb(0, 0, 0),

                    };
                    data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, d.Value)));
                    payoffview.AddLines(lines);
                }
                else
                {
                    var lines = new LineSeries
                    {
                        StrokeThickness = 0.5,
                        MarkerSize = 0.5,
                        CanTrackerInterpolatePoints = false,
                        Color = OxyColor.FromRgb(0, 0, 0),

                    };
                    data.ToList().ForEach(d => lines.Points.Add(new DataPoint(d.pas, d.Value)));
                    payoffview.AddLines(lines);
                }
            }

            PlotPayoff.DataContext = payoffview;
            PlotPayoff.Visibility = Visibility.Hidden;

            DisplaySimu.IsEnabled = true;
            DisplayPayoff.IsEnabled = true;

            PbStatus.Visibility = Visibility.Collapsed;
            PbCal.Visibility = Visibility.Collapsed;
            PriceInp.Visibility = Visibility.Visible;
            BSInp.Text = Prix.ToString();
            Prime = Prime / 100000m;
            Prime = Math.Round(Prime, 6);
            MCInp.Text = Prime.ToString();
            DeltaInp.Text = Math.Round(cal.put_delta(S, r, v, T, K), 5).ToString();
            GammaInp.Text = Math.Round(cal.put_gamma(S, r, v, T, K), 5).ToString();
            VegaInp.Text = Math.Round(cal.put_vega(S, r, v, T, K), 5).ToString();
            ThetaInp.Text = Math.Round(cal.put_theta(S, r, v, T, K), 5).ToString();
            RhoInp.Text = Math.Round(cal.put_rho(S, r, v, T, K), 5).ToString();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Price_Click(sender, e);
            }
        }
        public class Simu
        {
            public decimal Value { get; set; }
            public int id { get; set; }
            public int pas { get; set; }
        }

        private void ShowPayoff(object sender, RoutedEventArgs e)
        {
            if (PlotPayoff.Visibility == Visibility.Visible)
            {
                return;
            }
            else
            {
                PlotSimu.Visibility = Visibility.Hidden;
                PlotPayoff.Visibility = Visibility.Visible;
                return;
            }
        }

        private void ShowSimu(object sender, RoutedEventArgs e)
        {
            if (PlotSimu.Visibility == Visibility.Visible)
            {
                return;
            }
            else
            {
                PlotSimu.Visibility = Visibility.Visible;
                PlotPayoff.Visibility = Visibility.Hidden;
                return;
            }
        }
    }
}

