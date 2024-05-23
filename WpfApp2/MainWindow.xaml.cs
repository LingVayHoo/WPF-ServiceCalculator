﻿using ServiceCalculator;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DeliveryCalculator? deliveryCalculator;
        AssemblyCalculator? assemblyCalculator;
        DataSettings? settings;
        DefaultPriceChecker? priceChecker;
        FloorAscent? floorAscent;

        public MainWindow()
        {
            InitializeComponent();
            Preparing();
        }

        private void Preparing()
        {
            SaveToFile s = new SaveToFile("dataBase.txt");
            settings = s.Read();
            deliveryCalculator = new DeliveryCalculator(settings);
            assemblyCalculator = new AssemblyCalculator(settings);
            priceChecker = new DefaultPriceChecker(settings);
            floorAscent = new FloorAscent(settings);
            if (settings != null) FillAll();
            
        }

        private void FillAll()
        {
            ProductTypeComboBox.ItemsSource = new string[]
            {
                "Мелкогабаритный",
                "Крупногабаритный"
            };
            if (settings.IsSmallType) ProductTypeComboBox.SelectedItem = "Мелкогабаритный";
            else ProductTypeComboBox.SelectedItem = "Крупногабаритный";


        }

        private void ElevatorCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ResultButton_Click(object sender, RoutedEventArgs e)
        {
            var km = KmCountText.Text != string.Empty ? KmCountText.Text : 0.ToString();
            if (Int32.TryParse(km, out var result))
            {
                var res = deliveryCalculator.Calculate(result);
                res *= CheckWeight();
                res = priceChecker.Check(settings.IsSmallType, result) >= 0 ? priceChecker.Check(settings.IsSmallType, result) : res;
                if ((bool)NeedAscent.IsChecked) res += CalculateAscent();
                ResultText.Text = res.ToString();
            }            
        }

        private float CheckWeight()
        {
            
            if (float.TryParse(WeightText.Text, out var result))
            {                
                
                if (settings.MaxWeight > 0 && (result / settings.MaxWeight) > 1)
                {
                    var r = (float)Math.Floor(result / settings.MaxWeight) + 1;
                    return r;
                }
            }
            return 1;
        }

        private float CalculateAscent()
        {
            float result = 0;
            if (float.TryParse(FloorNumberText.Text,out var floor) && float.TryParse(WeightText.Text, out var weight))
            {
                result = floorAscent.Calculate(
                    (bool)ElevatorCheckBox.IsChecked,
                    (bool)LargeCheckBox.IsChecked,
                    floor,
                    weight);
            }
            return result;
        }

        private void ProductTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductTypeComboBox.SelectedIndex == 0) settings.IsSmallType = true;
            else if (ProductTypeComboBox.SelectedIndex == 1) settings.IsSmallType = false;
        }

        private void AssemblyCalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (assemblyCalculator == null) return;
            double result = -1;
            var c = GoodsCostText.Text;
            var d = DistanceText_Assembly.Text; // пока не используется
            if (float.TryParse(c, out float cost)) 
            {
                result = Math.Floor(assemblyCalculator.Calculate((bool)IsKitchenCheckBox.IsChecked, cost));
            }
            ResultText_Assembly.Text = result.ToString();
        }
    }
}