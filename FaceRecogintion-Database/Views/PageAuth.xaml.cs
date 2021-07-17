using System;
using System.Windows;
using System.Windows.Controls;
using FaceRecognition_Database.Views;

namespace FaceRecognition_Database
{
	/// <summary>
	/// Логика взаимодействия для PageAuth.xaml
	/// </summary>
	public partial class PageAuth : Page
	{
		private PageMain pageMain;
		public DBConnection con;
		public PageAuth()
		{
			InitializeComponent();
			pageMain = new PageMain(con);
		}

		private void BtnLogin_OnClick(object sender, RoutedEventArgs e)
		{
			con = new DBConnection();
			try
			{
				if (pbPassword.Password.Length == 0)
				{
					MessageBox.Show("Введите пароль");
					throw new Exception("Не введён пароль");
				}
				if (pbPassword.Password.Length < 6)
				{
					MessageBox.Show("Пароль меньше шести символов");
					throw new Exception("Пароль меньше шести символов");
				}
				if (con.Open(pbPassword.Password))
				{
					pageMain = new PageMain(con);
					NavigationService.Navigate(pageMain);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);

			}
		}
		
	}
}
