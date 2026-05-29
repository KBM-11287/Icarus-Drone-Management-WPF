using Icarus_Drone_Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IcarusDroneService
{
    public partial class MainWindow : Window
    {
        // 6.2 Create a global List<T> of type Drone called FinishedList.
        private readonly List<Drone> FinishedList = new List<Drone>();

        // 6.3 Create a global Queue<T> of type Drone called RegularService.
        private readonly Queue<Drone> RegularService = new Queue<Drone>();

        // 6.4 Create a global Queue<T> of type Drone called ExpressService.
        private readonly Queue<Drone> ExpressService = new Queue<Drone>();

        public MainWindow()
        {
            InitializeComponent();

            // Supports validation when the user pastes into the Service Cost textbox.
            DataObject.AddPastingHandler(txtCost, OnTxtCostPaste);

            DisplayRegularService();
            DisplayExpressService();
            DisplayFinishedItems();
            UpdateStatusBar("Ready");
        }

        #region Helper View Models

        /// <summary>
        /// Internal helper class for displaying Drone items in the ListViews
        /// without requiring public properties on the Drone class itself.
        /// </summary>
        private class DroneListViewItem
        {
            public string ClientName { get; set; }
            public string DroneModel { get; set; }
            public int ServiceTag { get; set; }
            public string ServiceProblem { get; set; }
            public string ServiceCost { get; set; }
        }

        /// <summary>
        /// Internal helper class for displaying completed items in the ListBox
        /// and allowing removal from FinishedList.
        /// </summary>
        private class FinishedListBoxItem
        {
            public Drone SourceDrone { get; set; }
            public string ClientName { get; set; }
            public double ServiceCost { get; set; }

            public override string ToString()
            {
                return $"{ClientName} - ${ServiceCost:0.00}";
            }
        }

        #endregion

        #region Validation and Formatting

        /// <summary>
        /// 6.10 Create a custom method to ensure the Service Cost textbox
        /// can only accept a double value with two decimal point.
        /// </summary>
        /// <param name="proposedText">The full textbox value after the attempted edit.</param>
        /// <returns>True if valid; otherwise false.</returns>
        private bool IsValidServiceCostText(string proposedText)
        {
            if (string.IsNullOrWhiteSpace(proposedText))
            {
                return true;
            }

            return Regex.IsMatch(proposedText, @"^\d+(\.\d{0,2})?$");
        }

        /// <summary>
        /// 6.10 Prevent invalid characters from being entered into the Service Cost textbox.
        /// </summary>
        private void txtCost_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = txtCost.Text;
            int caretIndex = txtCost.CaretIndex;
            string proposedText = currentText.Insert(caretIndex, e.Text);

            e.Handled = !IsValidServiceCostText(proposedText);
        }

        /// <summary>
        /// Prevent invalid pasted content in the Service Cost textbox.
        /// </summary>
        private void OnTxtCostPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string));
            string proposedText = txtCost.Text.Insert(txtCost.CaretIndex, pastedText);

            if (!IsValidServiceCostText(proposedText))
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        /// Validates the form fields before adding a new service item.
        /// </summary>
        /// <returns>True if valid; otherwise false.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Please enter the Client Name.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClientName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDroneModel.Text))
            {
                MessageBox.Show("Please enter the Drone Model.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDroneModel.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProblem.Text))
            {
                MessageBox.Show("Please enter the Service Problem.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtProblem.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCost.Text))
            {
                MessageBox.Show("Please enter the Service Cost.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCost.Focus();
                return false;
            }

            if (!double.TryParse(txtCost.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show("Service Cost must be a valid number with up to 2 decimal places.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCost.Focus();
                return false;
            }

            if (numServiceTag.Value == null)
            {
                MessageBox.Show("Please enter a valid Service Tag.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                numServiceTag.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Core Custom Methods

        /// <summary>
        /// 6.7 Create a custom method called GetServicePriority which returns
        /// the value of the priority radio group.
        /// </summary>
        /// <returns>Regular or Express.</returns>
        private string GetServicePriority()
        {
            if (radExpress.IsChecked == true)
            {
                return "Express";
            }

            return "Regular";
        }

        /// <summary>
        /// 6.11 Create a custom method to increment the Service Tag control.
        /// This method must be called inside AddNewItem before the new service item is added.
        /// </summary>
        private void IncrementServiceTag()
        {
            if (numServiceTag.Value == null)
            {
                numServiceTag.Value = 100;
                return;
            }

            if (numServiceTag.Value < 900)
            {
                numServiceTag.Value += 10;
            }
            else
            {
                numServiceTag.Value = 100;
            }
        }

        /// <summary>
        /// 6.17 Create a custom method that will clear all the textboxes
        /// after each service item has been added.
        /// </summary>
        private void ClearInputFields()
        {
            txtClientName.Clear();
            txtDroneModel.Clear();
            txtProblem.Clear();
            txtCost.Clear();

            radRegular.IsChecked = true;
            txtClientName.Focus();
        }

        /// <summary>
        /// 6.8 Create a custom method that will display all the elements
        /// in the RegularService queue.
        /// </summary>
        private void DisplayRegularService()
        {
            lvRegular.ItemsSource = null;
            lvRegular.ItemsSource = RegularService
                .Select(drone => new DroneListViewItem
                {
                    ClientName = drone.GetClientName(),
                    DroneModel = drone.GetDroneModel(),
                    ServiceTag = drone.GetServiceTag(),
                    ServiceProblem = drone.GetServiceProblem(),
                    ServiceCost = drone.GetServiceCost().ToString("0.00")
                })
                .ToList();
        }

        /// <summary>
        /// 6.9 Create a custom method that will display all the elements
        /// in the ExpressService queue.
        /// </summary>
        private void DisplayExpressService()
        {
            lvExpress.ItemsSource = null;
            lvExpress.ItemsSource = ExpressService
                .Select(drone => new DroneListViewItem
                {
                    ClientName = drone.GetClientName(),
                    DroneModel = drone.GetDroneModel(),
                    ServiceTag = drone.GetServiceTag(),
                    ServiceProblem = drone.GetServiceProblem(),
                    ServiceCost = drone.GetServiceCost().ToString("0.00")
                })
                .ToList();
        }

        /// <summary>
        /// Displays the finished service items in the completed ListBox.
        /// The ListBox must display the Client Name and Service Cost.
        /// </summary>
        private void DisplayFinishedItems()
        {
            lbCompleted.ItemsSource = null;
            lbCompleted.ItemsSource = FinishedList
                .Select(drone => new FinishedListBoxItem
                {
                    SourceDrone = drone,
                    ClientName = drone.GetClientName(),
                    ServiceCost = drone.GetServiceCost()
                })
                .ToList();
        }

        /// <summary>
        /// Updates the status bar text.
        /// </summary>
        /// <param name="message">Message to display.</param>
        private void UpdateStatusBar(string message)
        {
            txtStatusBar.Text = message;
        }

        #endregion

        #region Button Event Handlers

        /// <summary>
        /// 6.5 Create a button method called AddNewItem that will add a new service item
        /// to a Queue<T> based on the priority.
        /// 6.6 Before a new service item is added to the Express Queue
        /// the service cost must be increased by 15%.
        /// 6.7 Call GetServicePriority() inside this method.
        /// 6.11 Call IncrementServiceTag() inside this method before the item is added.
        /// 6.17 Clear all textboxes after each service item has been added.
        /// </summary>
        private void AddNewItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            int currentServiceTag = numServiceTag.Value ?? 100;
            double serviceCost = double.Parse(txtCost.Text, CultureInfo.InvariantCulture);
            string priority = GetServicePriority();

            // 6.11 Increment the service tag control before adding the new item.
            IncrementServiceTag();

            Drone newDrone = new Drone();
            newDrone.SetClientName(txtClientName.Text);
            newDrone.SetDroneModel(txtDroneModel.Text);
            newDrone.SetServiceTag(currentServiceTag);
            newDrone.SetServiceProblem(txtProblem.Text);

            if (priority == "Express")
            {
                // 6.6 Increase service cost by 15% for Express items.
                serviceCost = Math.Round(serviceCost * 1.15, 2);
                newDrone.SetServiceCost(serviceCost);
                ExpressService.Enqueue(newDrone);

                DisplayExpressService();
                UpdateStatusBar($"Express item added for {newDrone.GetClientName()}.");
            }
            else
            {
                newDrone.SetServiceCost(serviceCost);
                RegularService.Enqueue(newDrone);

                DisplayRegularService();
                UpdateStatusBar($"Regular item added for {newDrone.GetClientName()}.");
            }

            ClearInputFields();
        }

        /// <summary>
        /// 6.14 Create a button click method that will remove a service item
        /// from the regular ListView and dequeue the RegularService Queue<T>.
        /// The dequeued item must be added to the FinishedList and displayed in the ListBox.
        /// </summary>
        private void btnProcessRegular_Click(object sender, RoutedEventArgs e)
        {
            if (RegularService.Count == 0)
            {
                MessageBox.Show("There are no regular service items to process.", "Queue Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateStatusBar("Regular queue is empty.");
                return;
            }

            Drone completedDrone = RegularService.Dequeue();
            FinishedList.Add(completedDrone);

            DisplayRegularService();
            DisplayFinishedItems();

            UpdateStatusBar($"Processed regular item for {completedDrone.GetClientName()}.");
        }

        /// <summary>
        /// 6.15 Create a button click method that will remove a service item
        /// from the express ListView and dequeue the ExpressService Queue<T>.
        /// The dequeued item must be added to the FinishedList and displayed in the ListBox.
        /// </summary>
        private void btnProcessExpress_Click(object sender, RoutedEventArgs e)
        {
            if (ExpressService.Count == 0)
            {
                MessageBox.Show("There are no express service items to process.", "Queue Empty", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateStatusBar("Express queue is empty.");
                return;
            }

            Drone completedDrone = ExpressService.Dequeue();
            FinishedList.Add(completedDrone);

            DisplayExpressService();
            DisplayFinishedItems();

            UpdateStatusBar($"Processed express item for {completedDrone.GetClientName()}.");
        }

        #endregion

        #region Mouse Event Handlers

        /// <summary>
        /// 6.12 Create a mouse click method for the regular service ListView
        /// that will display the Client Name and Service Problem in the related textboxes.
        /// </summary>
        private void lvRegular_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (lvRegular.SelectedItem is DroneListViewItem selectedItem)
            {
                txtClientName.Text = selectedItem.ClientName;
                txtProblem.Text = selectedItem.ServiceProblem;
                UpdateStatusBar($"Selected regular item for {selectedItem.ClientName}.");
            }
        }

        /// <summary>
        /// 6.13 Create a mouse click method for the express service ListView
        /// that will display the Client Name and Service Problem in the related textboxes.
        /// </summary>
        private void lvExpress_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (lvExpress.SelectedItem is DroneListViewItem selectedItem)
            {
                txtClientName.Text = selectedItem.ClientName;
                txtProblem.Text = selectedItem.ServiceProblem;
                UpdateStatusBar($"Selected express item for {selectedItem.ClientName}.");
            }
        }

        /// <summary>
        /// 6.16 Create a double mouse click method that will delete
        /// a service item from the finished ListBox and remove the same item from the List<T>.
        /// </summary>
        private void lbCompleted_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbCompleted.SelectedItem is FinishedListBoxItem selectedItem)
            {
                FinishedList.Remove(selectedItem.SourceDrone);
                DisplayFinishedItems();

                UpdateStatusBar($"Removed completed item for {selectedItem.ClientName}.");
            }
        }

        #endregion
    }
}