using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Icarus_Drone_Management
{
    /// <summary>
    /// Holds the data for a drone being serviced
    /// </summary>

    public class Drone
    {
        private string clientName;
        private string droneModel;
        private int serviceTag;
        private string serviceProblem;
        private double serviceCost;

        // Defult constructor
        public Drone()
        {

            clientName = string.Empty;
            droneModel = string.Empty;
            serviceTag = 0;
            serviceProblem = string.Empty;
            serviceCost = 0.00;

        }

        //overloaded constructor

        /// <param name="clientName">The client name.</param>
        /// <param name="droneModel">The drone model.</param>
        /// <param name="serviceTag">The service tag.</param>
        /// <param name="serviceProblem">The service problem.</param>
        /// <param name="serviceCost">The service cost.</param

        public Drone(string clientName, string droneModel, int serviceTag, string serviceProblem, double serviceCost)
        {
            SetClientName(clientName);
            SetDroneModel(droneModel);
            SetServiceTag(serviceTag);
            SetServiceProblem(serviceProblem);
            SetServiceCost(serviceCost);

        }

        // private attributes with public getter and setter methods
        public string GetClientName()
        {
            return clientName;
        }

        public void SetClientName(string clientName)
        {
            this.clientName = clientName;
        }

        public string GetDroneModel()
        {
            return droneModel;

        }

        public void SetDroneModel(string droneModel)
        {
            this.droneModel = droneModel;
        }

        public int GetServiceTag() 
        {
            return serviceTag;
        }

        public void SetServiceTag(int value)
        {
            serviceTag = value;
        }

        public string GetServiceProblem()
        {
            return serviceProblem;
        }

        public void SetServiceProblem(string value)
        {
            serviceProblem = value.Substring(0, 1).ToUpper() + value.Substring(1);
        }

        public double GetServiceCost()
        {
            return serviceCost;
        }

        public void SetServiceCost(double value)
        {
            serviceCost = value;
        }

        // Display method that returns Client NAme and Service cost
        public string Display()
        {
            return $"Client Name: {GetClientName()}, Service Cost: {GetServiceCost():0.00}";
        }

        // Override ToString method to return all the data for a drone
        public override string ToString()
        {
            return $"Client Name: {GetClientName()}, Drone Model: {GetDroneModel()}, Service Tag: {GetServiceTag()}, Service Problem: {GetServiceProblem()}, Service Cost: {GetServiceCost():0.00}";
        }

        // Read-only properties for WPF bunding
        // These properties allow the ListView to display the data cleanly
        public string ClientNameDisplay => GetClientName();
        public string DroneModelDisplay => GetDroneModel();
        public int ServiceTagDisplay => GetServiceTag();
        public string ServiceProblemDisplay => GetServiceProblem();
        public string ServiceCostDisplay => GetServiceCost().ToString("0.00");


        private string ConvertToTitleCase(string value)
        {

            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(value.Trim().ToLower());
        }

        private string ConvertToSentenceCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmedValue = value.Trim().ToLower();
            return char.ToUpper(trimmedValue[0]) + trimmedValue.Substring(1);

        }
    }

}
