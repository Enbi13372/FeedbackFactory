﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class StudentView : UserControl
    {
        public StudentView()
        {
            InitializeComponent();
        }

        // Event handler for when the UserControl is loaded
        private void StudentView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to the KeyTB (TextBox for the key) when the view is loaded
            KeyTB.Focus();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the LoginWindow
            Window loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close current view's window if it’s standalone
            Window.GetWindow(this)?.Close();
        }
        private void ContinueBTN_Click(object sender, RoutedEventArgs e)
        {
           
            string inputKey = KeyTB.Text;

            if (string.IsNullOrWhiteSpace(inputKey))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Schlüssel ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            StudentFormWindow studentFormWindow = new StudentFormWindow(inputKey);
            studentFormWindow.Show();

            Window.GetWindow(this)?.Close();
        }

        
    }
}
