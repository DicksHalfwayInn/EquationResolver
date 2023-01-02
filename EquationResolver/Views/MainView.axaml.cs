using Avalonia.Controls;
using EquationResolver.ViewModels;

namespace EquationResolver.Views
{
    public partial class MainView : UserControl
    {
        // The ViewModel that will be used for the DataContext of the Window
        private MainViewModel vm;

        // Flag determining if this is the first time that the textbox text gets changed
        //      Just ignore the first pass for verification of characters and bracket matching
        private bool firstPass = true;

        // Default Constructor
        public MainView()
        {
            InitializeComponent();

            // Set the vm property to a new MainWindowViewModel 
            vm = new MainViewModel();

            // Set the DataContext of the Window to the new MainWindowViewModel
            DataContext = vm;
        }

        /// <summary>
        ///      Event:  Any change in the text of the Equation Textbox will call this method
        /// </summary>
        /// <param name="sender">The textbox that the text changed in</param>
        /// <param name="e">The args for the text changed event</param>
        private void EquationEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If this is the first time the text has changed then ignore the verification in the ViewModel
            if (!firstPass)
            {
                // If the event sender is a textbox...
                if (sender is TextBox)
                {
                    // Declare local property of sender as a textbox
                    var textBox = sender as TextBox;

                    // Grab the current text from the Equation textbox
                    var currentTextInEquationTextbox = textBox.Text;

                    // Call the method to verify the current text is all legal and show errors if it isn't
                    // TODO:  Currently this doesn't work
                    vm.TextboxTextChangedEventMethodInVM(currentTextInEquationTextbox);
                }
            }
            // If this was the first pass then set the firstPass flag to false
            else firstPass = false;
        }
    }
}