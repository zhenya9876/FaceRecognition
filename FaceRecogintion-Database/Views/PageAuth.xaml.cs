using System;
using System.Collections.Generic;
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
using DevExpress.Mvvm;
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
				if (con.Open(tbPassword.Password))
				{
					pageMain = new PageMain(con);
					NavigationService.Navigate(pageMain);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}
		
	}
}
