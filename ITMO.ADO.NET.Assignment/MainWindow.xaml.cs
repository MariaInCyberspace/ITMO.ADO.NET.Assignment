using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ITMO.ADO.NET.Assignment
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        northwndEntities context;
        CollectionViewSource customersViewSource, ordersViewSource;
        public MainWindow()
        {
            InitializeComponent();
            customersViewSource = FindResource("customersViewSource") as CollectionViewSource;
            ordersViewSource = FindResource("customersOrdersViewSource") as CollectionViewSource;
            DataContext = this;
        }

        private void GoToPreviousEntryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            customersViewSource.View.MoveCurrentToPrevious();
        }

        private void GoToNextEntryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            customersViewSource.View.MoveCurrentToNext();
        }

        private void DeleteCustomerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Customers displayedCustomer = customersViewSource.View.CurrentItem as Customers;
            Customers customerInDb = (Customers)(from custom in context.Customers.Local where custom.CustomerID == displayedCustomer.CustomerID select custom).FirstOrDefault();
            if (customerInDb != null)
            {
                // Delete order of a customer chosen to be deleted
                foreach (Orders order in customerInDb.Orders.ToList())
                {
                    DeleteOrder(order);
                }
                // Delete the chosen customer from the database
                context.Customers.Remove(customerInDb);
            }
            // Save the changes made to the database
            context.SaveChanges();
            customersViewSource.View.Refresh();
        }

        private void DeleteOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Get selected order or the first order in the view and pass it as a parameter to the DeleteOrder method
            DeleteOrder(ordersViewSource.View.CurrentItem as Orders);
        }

        private void DeleteOrder(Orders delOrder)
        {
            // Find the order in the DBSet of Orders in the context that matches the order passed as a parameter
            Orders order = (from ord in context.Orders.Local
                       where ord.OrderID == delOrder.OrderID
                       select ord).FirstOrDefault();

            // Remove order details for the selected order
            foreach (Order_Details od in order.Order_Details.ToList())
            {
                context.Order_Details.Remove(od);
            }
            // Remove the order from the database and save changes made
            context.Orders.Remove(order);
            context.SaveChanges();
 
            ordersViewSource.View.Refresh();
        }

        private void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (addNewCustomerGrid.Visibility == Visibility.Visible)
            {
                // Declaration and initialization of string builders that are going to be used to define new customer's ID
                StringBuilder stringBuilder_CustomerID = new StringBuilder();
                StringBuilder stringBuilder_ContactName = new StringBuilder();

                // Create a new Customer to add to the context
                Customers newCustomer = new Customers
                {
                    Address = newAddressTextBox.Text,
                    City = newCityTextBox.Text,
                    CompanyName = newCompanyNameTextBox.Text,
                    ContactName = newContactNameTextBox.Text,
                    ContactTitle = newContactTitleTextBox.Text,
                    Country = newCountryTextBox.Text,
                    Fax = newFaxTextBox.Text,
                    Phone = newPhoneTextBox.Text,
                    PostalCode = newPostalCodeTextBox.Text,
                    Region = newRegionTextBox.Text
                };

                // Split the new customer's contact name by a space character and store substrings in an array
                string[] stringArr = newCustomer.ContactName.Split(' ');
                for (int i = 0; i < stringArr.Length; i++)
                {
                    stringBuilder_ContactName.Append(stringArr[i]);
                }
                // Get an array of characters of the new customer's name
                char[] contName = stringBuilder_ContactName.ToString().ToCharArray();

                // Make customer's ID based on contact name
                for (int i = 0; i < contName.Length; i += 2)
                {
                    stringBuilder_CustomerID.Append(contName[i]);
                }

                // Make sure that the new customer's ID doesn't have more than 5 characters
                if (stringBuilder_CustomerID.Length > 5)
                {
                    newCustomer.CustomerID = stringBuilder_CustomerID.ToString().Remove(5).ToUpper();
                }
                else
                {
                    newCustomer.CustomerID = stringBuilder_CustomerID.ToString().ToUpper();
                }

                // Insert the new customer at correct position:  
                int length = context.Customers.Local.Count();
                int position = length;
                for (int i = 0; i < length; i++)
                {
                    if (String.CompareOrdinal(newCustomer.CustomerID, context.Customers.Local[i].CustomerID) < 0)
                    {
                        position = i;
                        break;
                    }
                }
                context.Customers.Local.Insert(position, newCustomer);
                context.SaveChanges();
                customersViewSource.View.Refresh();
                customersViewSource.View.MoveCurrentTo(newCustomer);
               

                addNewCustomerGrid.Visibility = Visibility.Collapsed;
                customersGrid.Visibility = Visibility.Visible;
            }
            else if (addNewOrderGrid.Visibility == Visibility.Visible)
            {
                // Get the currently selected customer to use their ID, Address, etc. in initializing new order object
                Customers currentCustomer = customersViewSource.View.CurrentItem as Customers;
                IFormatProvider formatProvider = CultureInfo.CurrentCulture;
                StringBuilder stringBuilder = new StringBuilder();
                string s1, s2, s3;
                string[] stringArr = Array.Empty<string>();

                Orders newOrder = new Orders()
                {
                    OrderDate = newOrderDatePicker.SelectedDate,
                    RequiredDate = newOrderRequiredDatePicker.SelectedDate,
                    ShippedDate = newOrderShippedDatePicker.SelectedDate,
                    CustomerID = currentCustomer.CustomerID,
                    ShipAddress = currentCustomer.Address,
                    ShipCity = currentCustomer.City,
                    ShipCountry = currentCustomer.Country,
                    ShipName = currentCustomer.CompanyName,
                    ShipPostalCode = currentCustomer.PostalCode,
                    ShipRegion = currentCustomer.Region
                };
                try
                {
                    // Make sure the freight value is defined correctly
                    stringArr = newOrderFreightTextBox.Text.Contains(".") ? newOrderFreightTextBox.Text.Split('.') : newOrderFreightTextBox.Text.Split(',');

                    if (stringArr.Count() >= 3) 
                    {
                        throw new FormatException();
                    }
                    
                    else if (newOrderFreightTextBox.Text.Contains("."))
                    {
                        s1 = stringArr[0];
                        s2 = ",";
                        s3 = stringArr[1];

                        stringBuilder.Append(s1); stringBuilder.Append(s2); stringBuilder.Append(s3);
                        newOrder.Freight = Decimal.Parse(stringBuilder.ToString(), formatProvider);
                    }
                    else
                    {
                        newOrder.Freight = Decimal.Parse(newOrderFreightTextBox.Text, formatProvider);
                    }


                    try
                    {
                        newOrder.EmployeeID = Int32.Parse(newOrderEmployeeID.Text, formatProvider);
                        newOrder.ShipVia = Int32.Parse(newOrderShipViaTextBox.Text, formatProvider);
                        context.Orders.Add(newOrder);
                        context.SaveChanges();
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        // Remove the added order from the view if exception was caught
                        context.Orders.Remove(newOrder); 
                        MessageBox.Show(ex.Message + " ShipVia column value can only be 1, 2 or 3");
                    }

                    // Refresh the view and hide the addNewOrder grid
                    ordersViewSource.View.Refresh();
                    addNewOrderGrid.Visibility = Visibility.Collapsed;
                    customersGrid.Visibility = Visibility.Visible;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            context.SaveChanges();
        }

        private void AddCustomerCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            customersGrid.Visibility = Visibility.Hidden;
            ordersDataGrid.Visibility = Visibility.Hidden;
            addNewOrderGrid.Visibility = Visibility.Collapsed;
            addNewCustomerGrid.Visibility = Visibility.Visible;
        }

        private void addOrderButton_Click(object sender, RoutedEventArgs e)
        {
            customersGrid.Visibility = Visibility.Hidden;
            addNewCustomerGrid.Visibility = Visibility.Collapsed;
            addNewOrderGrid.Visibility = Visibility.Visible;
        }

        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            addNewOrderGrid.Visibility = Visibility.Collapsed;
            addNewCustomerGrid.Visibility = Visibility.Collapsed;
            customersGrid.Visibility = Visibility.Visible;
            ordersDataGrid.Visibility = Visibility.Visible;

            // Checking what object sent the event
            var button = e.Source as Button;

            // Empty proper textboxes based on button's identifier
            if (button.Name == "cancelCustomerButton")
            {
                newAddressTextBox.Text = "";
                newCityTextBox.Text = "";
                newCompanyNameTextBox.Text = "";
                newContactNameTextBox.Text = "";
                newContactTitleTextBox.Text = "";
                newCountryTextBox.Text = "";
                newFaxTextBox.Text = "";
                newPhoneTextBox.Text = "";
                newPostalCodeTextBox.Text = "";
                newRegionTextBox.Text = "";
                newCustomerIDTextBox.Text = "";
            }
            else
            {
                newOrderEmployeeID.Text = "";
                newOrderShipViaTextBox.Text = "";
                newOrderFreightTextBox.Text = "";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Data.CollectionViewSource customersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customersViewSource")));
            // Загрузите данные, установив свойство CollectionViewSource.Source:
            // customersViewSource.Source = [универсальный источник данных]

            context = new northwndEntities("name = NorthwindEntities");
            context.Customers.Load();
            customersViewSource.Source = context.Customers.Local;
        }
    }
}
