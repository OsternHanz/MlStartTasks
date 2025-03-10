﻿using ClassLibrary;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для LogInPage.xaml
    /// </summary>
    public partial class LogInPage : Page
    {
        public MainWindow mainWindow;
        public static string? login;
        public LogInPage(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            InitializeComponent();
        }
        private void enter_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.areWeLogin)
            {
                MessageBox.Show("Well you already logIn");
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox_login.Text))
            {
                MessageBox.Show("Enter login");
                return;
            }
            if (string.IsNullOrWhiteSpace(password.Password))
            {
                MessageBox.Show("Enter password");
                return;
            }
            login = textBox_login.Text;
            if (SqlCore.DatabaseExists("MLstartDataBase") && SqlCore.TableExists("MLstartDataBase", "Userss"))
            {
                if (SqlCore.CheckHashAndLog(textBox_login.Text, password.Password, MainWindow.connectionString))
                {
                    MessageBox.Show("You Log successfully");
                    mainWindow.areWeLogin = true;
                    mainWindow.UpdateButtonVisibility(mainWindow.areWeLogin);
                }
                else
                {
                    MessageBox.Show("User does not exist");
                }
            }
            else
            {
                SqlCore.CreateDatabase();
                string[] usersColumns =
                {
                    "Personid INT PRIMARY KEY IDENTITY",
                    "Login VARCHAR(255) NOT NULL",
                    "PassWord VARCHAR(255) NOT NULL"
                };
                SqlCore.CreateTable(MainWindow.connectionString, "Userss", usersColumns);
                MessageBox.Show("DataBase or Table does not exist and was created, repeat process");
            }
        }

    }
}

