using System;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using FaceRecognition_Database.Models;
using FaceRecognition_Database.Views;

namespace FaceRecogintion_Database
{
	/// <summary>
	/// Логика взаимодействия для StudentControl.xaml
	/// </summary>
	public partial class StudentControl : UserControl
	{
		private Student CurrentStudent;
		private object Sender;
		public StudentControl(object sender, Student student)
		{
			CurrentStudent = student;
			Sender = sender;
			InitializeComponent();
			this.lblStudentDB.Content = $"{CurrentStudent.Surname} {CurrentStudent.Name} {CurrentStudent.Patronymic}";
			this.imgStudentDB.Source = BitmapToImageSource.Get(CurrentStudent.Photo.ToBitmap());
			this.btnDeleteStudentDB.Click += OnStudentDeleteClick;
		}

		private void OnStudentDeleteClick(object sender, RoutedEventArgs e)
		{
			SQLCommands.DeleteStudent(CurrentStudent);
			try
			{
				(Sender as PageMain).LoadDB();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}
	}
}
